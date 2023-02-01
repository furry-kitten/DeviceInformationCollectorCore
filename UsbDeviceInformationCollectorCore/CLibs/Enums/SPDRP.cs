namespace UsbDeviceInformationCollectorCore.CLibs.Enums
{
    /// <summary>
    ///     Device registry property codes
    /// </summary>
    public enum Spdrp
    {
        /// <summary>
        ///     DeviceDesc (R/W)
        /// </summary>
        Devicedesc = 0x00000000,

        /// <summary>
        ///     HardwareID (R/W)
        /// </summary>
        HardwareId = 0x00000001,

        /// <summary>
        ///     CompatibleIDs (R/W)
        /// </summary>
        Compatibleids = 0x00000002,

        /// <summary>
        ///     unused
        /// </summary>
        Unused0 = 0x00000003,

        /// <summary>
        ///     Service (R/W)
        /// </summary>
        Service = 0x00000004,

        /// <summary>
        ///     unused
        /// </summary>
        Unused1 = 0x00000005,

        /// <summary>
        ///     unused
        /// </summary>
        Unused2 = 0x00000006,

        /// <summary>
        ///     Class (R--tied to ClassGUID)
        /// </summary>
        Class = 0x00000007,

        /// <summary>
        ///     ClassGUID (R/W)
        /// </summary>
        ClassGuid = 0x00000008,

        /// <summary>
        ///     Driver (R/W)
        /// </summary>
        Driver = 0x00000009,

        /// <summary>
        ///     ConfigFlags (R/W)
        /// </summary>
        ConfigFlags = 0x0000000A,

        /// <summary>
        ///     Mfg (R/W)
        /// </summary>
        Mfg = 0x0000000B,

        /// <summary>
        ///     FriendlyName (R/W)
        /// </summary>
        FriendlyName = 0x0000000C,

        /// <summary>
        ///     LocationInformation (R/W)
        /// </summary>
        LocationInformation = 0x0000000D,

        /// <summary>
        ///     PhysicalDeviceObjectName (R)
        /// </summary>
        PhysicalDeviceObjectName = 0x0000000E,

        /// <summary>
        ///     Capabilities (R)
        /// </summary>
        Capabilities = 0x0000000F,

        /// <summary>
        ///     UiNumber (R)
        /// </summary>
        UiNumber = 0x00000010,

        /// <summary>
        ///     UpperFilters (R/W)
        /// </summary>
        UpperFilters = 0x00000011,

        /// <summary>
        ///     LowerFilters (R/W)
        /// </summary>
        LowerFilters = 0x00000012,

        /// <summary>
        ///     BusTypeGUID (R)
        /// </summary>
        BusTypeGuid = 0x00000013,

        /// <summary>
        ///     LegacyBusType (R)
        /// </summary>
        LegacyBusType = 0x00000014,

        /// <summary>
        ///     BusNumber (R)
        /// </summary>
        BusNumber = 0x00000015,

        /// <summary>
        ///     Enumerator Name (R)
        /// </summary>
        EnumeratorName = 0x00000016,

        /// <summary>
        ///     Security (R/W, binary form)
        /// </summary>
        Security = 0x00000017,

        /// <summary>
        ///     Security (W, SDS form)
        /// </summary>
        SecuritySds = 0x00000018,

        /// <summary>
        ///     Device Type (R/W)
        /// </summary>
        DevType = 0x00000019,

        /// <summary>
        ///     Device is exclusive-access (R/W)
        /// </summary>
        Exclusive = 0x0000001A,

        /// <summary>
        ///     Device Characteristics (R/W)
        /// </summary>
        Characteristics = 0x0000001B,

        /// <summary>
        ///     Device Address (R)
        /// </summary>
        Address = 0x0000001C,

        /// <summary>
        ///     UiNumberDescFormat (R/W)
        /// </summary>
        UiNumberDescFormat = 0X0000001D,

        /// <summary>
        ///     Device Power Data (R)
        /// </summary>
        DevicePowerData = 0x0000001E,

        /// <summary>
        ///     Removal Policy (R)
        /// </summary>
        RemovalPolicy = 0x0000001F,

        /// <summary>
        ///     Hardware Removal Policy (R)
        /// </summary>
        RemovalPolicyHwDefault = 0x00000020,

        /// <summary>
        ///     Removal Policy Override (RW)
        /// </summary>
        RemovalPolicyOverride = 0x00000021,

        /// <summary>
        ///     Device Install State (R)
        /// </summary>
        InstallState = 0x00000022,

        /// <summary>
        ///     Device Location Paths (R)
        /// </summary>
        LocationPaths = 0x00000023
    }
}