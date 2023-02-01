namespace UsbDeviceInformationCollectorCore.CLibs.Enums
{
    /// <summary>
    ///     KeyType values for SetupDiCreateDevRegKey, SetupDiOpenDevRegKey, and SetupDiDeleteDevRegKey.
    /// </summary>
    public enum DiReg : uint
    {
        /// <summary>
        ///     Open/Create/Delete device key
        /// </summary>
        Dev = 0x00000001,

        /// <summary>
        ///     Open/Create/Delete driver key
        /// </summary>
        Drv = 0x00000002,

        /// <summary>
        ///     Delete both driver and Device key
        /// </summary>
        Both = 0x00000004
    }
}