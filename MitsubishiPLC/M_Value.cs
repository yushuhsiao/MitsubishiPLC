using System.Diagnostics;
using System.Threading;

namespace Mitsubishi.PLC
{
    [DebuggerDisplay("{Name} : {Value}")]
    public class M_Value
    {
        public const string M = "M";
        public const string X = "X";
        public const string Y = "Y";

        public string Type { get; set; }
        public int Index { get; set; }
        public string Name => $"{Type}{Index}";

        private object _oldValue;
        private int _value;

        public int? OldValue => Interlocked.CompareExchange(ref _oldValue, null, null) as int?;
        public int Value => Interlocked.CompareExchange(ref _value, 0, 0);

        /// <summary>
        /// 是否為正緣訊號
        /// </summary>
        public bool IsRisingEdge => OldValue == 0 && Value == 1;

        /// <summary>
        /// 是否為負緣訊號
        /// </summary>
        public bool IsFallingEdge => OldValue == 1 && Value == 0;

        /// <summary>
        /// 是否有邊緣訊號
        /// </summary>
        public bool IsEdge => OldValue.HasValue && OldValue.Value != Value;

        public void SetValue(int value)
        {
            var oldValue = Interlocked.Exchange(ref _value, value);
            Interlocked.Exchange(ref _oldValue, oldValue);
        }

        public M_Value(int value)
        {
            Interlocked.Exchange(ref _value, value);
        }

        public M_Value(string type, int index, int value) : this(value)
        {
            Type = type;
            Index = index;
        }
    }
}