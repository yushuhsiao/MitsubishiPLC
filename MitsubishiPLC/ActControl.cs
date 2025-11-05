using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mitsubishi.PLC
{
    public static class ActControl
    {
        public static ErrorCode ErrorCode { get; set; } = ErrorCode.Unknown;
        public static Exception Exception { get; set; }
        public static bool Lock_DecodeM { get; set; } = true;

        public static bool Open(out IActControl act, int ActLogicalStationNumber, ILogger _logger)
        {
            ErrorCode = ErrorCode.Unknown;
            Exception = null;
            try
            {
                act = new ActUtlType64Class { ActLogicalStationNumber = ActLogicalStationNumber };
                ErrorCode = act.Open();
                if (ErrorCode == 0)
                {
                    _logger?.LogInformation($"PLC Open, ActLogicalStationNumber = {ActLogicalStationNumber}");
                    return true;
                }
                else
                {
                    _logger?.LogError($"PLC Open Error, Code = {ErrorCode.ToString("X")} {ErrorCode}");
                }
            }
            catch (Exception ex)
            {
                Exception = ex;
                _logger?.LogError($"PLC Open Error, Code = {ErrorCode.ToString("X")} {ErrorCode}, {ex.Message}");
            }
            act = default;
            return false;
        }

        public static void Close(this IActControl act, ILogger _logger)
        {
            try
            {
                if (act != null)
                {
                    act.Close();
                    _logger?.LogInformation("PLC Close");
                }
            }
            catch { }
        }

        private static Dictionary<int, Queue<int[]>> buffers = new Dictionary<int, Queue<int[]>>();

        private static int[] AllocBuffer_Bits(int length)
        {
            int len = length / 16;
            if (length % 16 > 0) len++;
            if (len < 1)
                len = 1;
            return AllocBuffer(len);
        }

        private static int[] AllocBuffer(int size)
        {
            if (size < 0)
                size = 1;
            lock (buffers)
            {
                if (buffers.TryGetValue(size, out var q) == false)
                    buffers[size] = q = new Queue<int[]>();
                if (q.Count == 0)
                    return new int[size];
                return q.Dequeue();
            }
        }
        private static void ReleaseBuffer(int[] buf)
        {
            lock (buffers)
                if (buffers.TryGetValue(buf.Length, out var q))
                    q.Enqueue(buf);
        }


        /// <summary>
        /// 讀取 M 值
        /// </summary>
        /// <param name="data">陣列大小 x 16 = 可取得的 X 直</param>
        /// <returns></returns>
        public static ErrorCode ReadM(this IActControl act, int begin, int[] data, Dictionary<int, M_Value> values, out TimeSpan time, bool lock_decode = true)
        {
            begin /= 16;
            begin *= 16;
            DateTime t = DateTime.Now;
            ErrorCode = act.ReadDeviceBlock($"{M_Value.M}{begin}", data.Length, out data[0]);
            //ErrorCode = ErrorCode.COM_port_handle_error;
            time = DateTime.Now - t;
            if (ErrorCode == 0)
            {
                if (Lock_DecodeM || lock_decode)
                    lock (values)
                        Decode_M(begin, data, values);
                else
                    Decode_M(begin, data, values);
            }
            return ErrorCode;
        }
        public static ErrorCode ReadM(this IActControl act, int begin, int length, Dictionary<int, M_Value> values, out TimeSpan time, bool lock_decode = true)
        {
            var buff = AllocBuffer_Bits(length);
            try { return ReadM(act, begin, buff, values, out time, lock_decode); }
            finally { ReleaseBuffer(buff); }
        }

        private static void Decode_M(int begin, int[] data, Dictionary<int, M_Value> values)
        {
            for (int n1 = 0; n1 < data.Length; n1++)
            {
                int value_tmp = data[n1];
                for (int n2 = 0; n2 < 16; n2++)
                {
                    int value = (value_tmp & 1 << n2) != 0 ? 1 : 0;
                    int index = begin + n2 + n1 * 16;
                    if (values.TryGetValue(index, out var m_Value))
                        m_Value.SetValue(value);
                    else
                        values[index] = new M_Value(M_Value.M, index, value);
                }
            }
        }

        // 寫入區塊
        public static ErrorCode WriteM(this IActControl act, int begin, int length, Dictionary<int, M_Value> values)
        {
            begin /= 16; begin *= 16;           // 對齊到 16 的倍數
            length += 15; length /= 16;         // 計算需要的區塊數（每區塊 16 bits）
            var data = AllocBuffer_Bits(length);
            for (int i = 0, offset = begin; i < length; i++, offset += 16)
                data[i] = Encode_M(offset, values);
            try { return act.WriteDeviceBlock($"{M_Value.M}{begin}", data); }
            finally { ReleaseBuffer(data); }
        }

        // 寫入單一區塊
        public static ErrorCode WriteM(this IActControl act, int begin, Dictionary<int, M_Value> values)
        {
            begin = (begin / 16) * 16;          // 對齊到16的倍數
            var data = AllocBuffer_Bits(1);
            data[0] = Encode_M(begin, values);
            try { return act.WriteDeviceBlock($"{M_Value.M}{begin}", data); }
            finally { ReleaseBuffer(data); }
        }

        // bit 組合成 int, begin 必須是對齊到 16 的倍數
        private static int Encode_M(int begin, Dictionary<int, M_Value> values)
        {
            int result = 0;
            for (int bit = 0; bit < 16; bit++)
                if (values.TryGetValue(begin + bit, out var m_Value) && m_Value.Value != 0)
                    result |= (1 << bit);
            return result;
        }


        /// <summary>
        /// 讀取 X 值或 Y 值
        /// </summary>
        private static ErrorCode Read_XY(this IActControl act, string type, int begin, int[] data, Dictionary<int, M_Value> values, out TimeSpan time, bool lock_decode, bool release_data)
        {
            try
            {
                begin /= 20;
                begin *= 20;
                DateTime t = DateTime.Now;
                ErrorCode = act.ReadDeviceBlock($"{type}{begin}", data.Length, out data[0]);
                time = DateTime.Now - t;
                if (ErrorCode == 0)
                {
                    if (lock_decode)
                        lock (values)
                            Decode_XY(type, begin, data, values);
                    else
                        Decode_XY(type, begin, data, values);
                }
                return ErrorCode;
            }
            finally
            {
                if (release_data) ReleaseBuffer(data);
            }
        }

        private static void Decode_XY(string type, int begin, int[] data, Dictionary<int, M_Value> values)
        {
            for (int n1 = 0; n1 < data.Length; n1++)
            {
                var value_tmp = data[n1];
                for (int n2 = 0; n2 < 16; n2++)
                {
                    int value = (value_tmp & 1 << n2) != 0 ? 1 : 0;
                    int index = begin;
                    if (n2 < 8)
                        index += n2;
                    else
                        index += 10 + n2 - 8;
                    if (values.TryGetValue(index, out var m_Value))
                        m_Value.SetValue(value);
                    else
                        values[index] = new M_Value(type, index, value);
                }
                begin += 20;
                if (begin % 100 == 80)
                    begin += 20;
            }
        }

        /// <summary>
        /// 讀取 X 值
        /// </summary>
        /// <param name="begin">起始位址</param>
        /// <param name="data">陣列大小x16 = 可取得的 X 值</param>
        public static ErrorCode ReadX(this IActControl act, int begin, int[] data, Dictionary<int, M_Value> values, out TimeSpan time, bool lock_decode = true) => Read_XY(act, M_Value.X, begin, data, values, out time, lock_decode, false);

        /// <summary>
        /// 讀取 X 值
        /// </summary>
        /// <param name="begin">起始位址</param>
        /// <param name="length">讀取數量</param>
        public static ErrorCode ReadX(this IActControl act, int begin, int length, Dictionary<int, M_Value> values, out TimeSpan time, bool lock_decode = true) => Read_XY(act, M_Value.X, begin, AllocBuffer_Bits(length), values, out time, lock_decode, true);

        /// <summary>
        /// 讀取 X 值
        /// </summary>
        public static ErrorCode ReadX(this IActControl act, Dictionary<int, M_Value> values, out TimeSpan time, bool lock_decode = true) => ReadX(act, 0, 256, values, out time, lock_decode);

        /// <summary>
        /// 讀取 Y 值
        /// </summary>
        /// <param name="begin">起始位址</param>
        /// <param name="data">陣列大小x16 = 可取得的 Y 值</param>
        public static ErrorCode ReadY(this IActControl act, int begin, int[] data, Dictionary<int, M_Value> values, out TimeSpan time, bool lock_decode = true) => Read_XY(act, M_Value.Y, begin, data, values, out time, lock_decode, false);

        /// <summary>
        /// 讀取 Y 值
        /// </summary>
        /// <param name="begin">起始位址</param>
        /// <param name="length">讀取數量</param>
        public static ErrorCode ReadY(this IActControl act, int begin, int length, Dictionary<int, M_Value> values, out TimeSpan time, bool lock_decode = true) => Read_XY(act, M_Value.Y, begin, AllocBuffer_Bits(length), values, out time, lock_decode, true);

        /// <summary>
        /// 讀取 Y 值
        /// </summary>
        public static ErrorCode ReadY(this IActControl act, Dictionary<int, M_Value> values, out TimeSpan time, bool lock_decode = true) => ReadY(act, 0, 256, values, out time, lock_decode);



        public static ErrorCode ReadDeviceBlock(this IActControl act, string szDevice, int[] data) => ReadDeviceBlock(act, szDevice, data, out var time);
        public static ErrorCode ReadDeviceBlock(this IActControl act, string szDevice, int[] data, out TimeSpan time)
        {
            DateTime t = DateTime.Now;
            var err = act.ReadDeviceBlock(szDevice, data.Length, out data[0]);
            time = DateTime.Now - t;
            return err;
        }

        public static ErrorCode WriteDeviceBlock(this IActControl act, string szDevice, int[] data) => WriteDeviceBlock(act, szDevice, data, out var time);
        public static ErrorCode WriteDeviceBlock(this IActControl act, string szDevice, int[] data, out TimeSpan time)
        {
            DateTime t = DateTime.Now;
            var err = act.WriteDeviceBlock(szDevice, data.Length, ref data[0]);
            time = DateTime.Now - t;
            return err;
        }

        public static ErrorCode SetDevice(this IActControl act, M_Value m, int? value = null)
        {
            if (m == null) return ErrorCode.Unknown;
            if (value.HasValue)
                return act.SetDevice(m.Name, value.Value);
            else
                return act.SetDevice(m.Name, m.Value == 0 ? 1 : 0);
        }

        public static ErrorCode GetDevice(this IActControl act, M_Value m, out int value)
        {
            value = 0;
            if (m == null) return ErrorCode.Unknown;
            return act.GetDevice(m.Name, out value);
        }

        public static bool IsSuccess(this ErrorCode err) => err == 0;
        public static bool IsNotSuccess(this ErrorCode err) => err != 0;
    }
}