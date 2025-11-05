namespace Mitsubishi.PLC
{
    public interface IActControl
    {
        int ActLogicalStationNumber { get; set; }
        string ActPassword { get; set; }

        ErrorCode Open();
        ErrorCode Close();

        ErrorCode ReadDeviceBlock(string szDevice, int lSize, out int lplData);
        ErrorCode WriteDeviceBlock(string szDevice, int lSize, ref int lplData);
        ErrorCode ReadDeviceRandom(string szDeviceList, int lSize, out int lplData);
        ErrorCode WriteDeviceRandom(string szDeviceList, int lSize, ref int lplData);
        ErrorCode SetDevice(string szDevice, int lData);
        ErrorCode GetDevice(string szDevice, out int lplData);
    }
}
