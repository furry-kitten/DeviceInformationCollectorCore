namespace UsbDeviceInformationCollectorCore.CLibs.Enums
{
    /// <summary>
    ///     Values specifying the scope of a device property change.
    /// </summary>
    public enum DicsFlag : uint
    {
        /// <summary>
        ///     Make change in all hardware profiles
        /// </summary>
        FlagGlobal = 0x0000001,

        /// <summary>
        ///     Make change in specified profile only
        /// </summary>
        FlagConfigSpecific = 0x00000002,

        /// <summary>
        ///     1 or more hardware profile-specific
        /// </summary>
        FlagConfigGeneral = 0x00000004
    }
}