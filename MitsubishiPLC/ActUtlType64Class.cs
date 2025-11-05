namespace Mitsubishi.PLC
{
    public class ActUtlType64Class : IActControl
    {
        private ActUtlType64Lib.ActUtlType64Class obj = new ActUtlType64Lib.ActUtlType64Class();

        public int ActLogicalStationNumber
        {
            get => obj.ActLogicalStationNumber;
            set => obj.ActLogicalStationNumber = value;
        }

        public string ActPassword
        {
            get => obj.ActPassword;
            set => obj.ActPassword = value;
        }

        public ErrorCode Close() => (ErrorCode)obj.Close();

        public ErrorCode GetDevice(string szDevice, out int lplData) => (ErrorCode)obj.GetDevice(szDevice, out lplData);

        public ErrorCode Open() => (ErrorCode)obj.Open();

        public ErrorCode ReadDeviceBlock(string szDevice, int lSize, out int lplData) => (ErrorCode)obj.ReadDeviceBlock(szDevice, lSize, out lplData);

        public ErrorCode ReadDeviceRandom(string szDeviceList, int lSize, out int lplData) => (ErrorCode)obj.ReadDeviceRandom(szDeviceList, lSize, out lplData);

        public ErrorCode SetDevice(string szDevice, int lData) => (ErrorCode)obj.SetDevice(szDevice, lData);

        public ErrorCode WriteDeviceBlock(string szDevice, int lSize, ref int lplData) => (ErrorCode)obj.WriteDeviceBlock(szDevice, lSize, ref lplData);

        public ErrorCode WriteDeviceRandom(string szDeviceList, int lSize, ref int lplData) => (ErrorCode)obj.WriteDeviceRandom(szDeviceList, lSize, ref lplData);
    }
}
