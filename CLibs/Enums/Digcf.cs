using System;

namespace UsbDeviceInformationCollectorCore.CLibs.Enums
{
    /// <summary>
    /// Flags controlling what is included in the device information set built by SetupDiGetClassDevs
    /// </summary>
    [Flags]
    public enum Digcf : int
    {
        Default = 0x00000001,    // only valid with DIGCF_DEVICEINTERFACE
        Present = 0x00000002,
        AllClasses = 0x00000004,
        Profile = 0x00000008,
        DeviceInterface = 0x00000010,
    }
}
