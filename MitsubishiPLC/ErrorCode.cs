namespace Mitsubishi.PLC
{
    public enum ErrorCode : uint
    {
        Unknown = 0xFFFFFFFF,
        Normal_end = 0x00000000,
        Timeout_error = 0x01010002,
        Message_error = 0x01010005,
        COM_port_handle_error = 0x01808009,
        Time_out_error = 0x0180840B,
        Character_code_conversion_error = 0xF1000001
    }
}
