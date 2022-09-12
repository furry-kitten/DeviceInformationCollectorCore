using System;

namespace UsbDeviceInformationCollectorCore.CLibs.Enums
{
    [Flags]
    public enum DeviceNotify : uint
    {
        WindowHandle = 0x00000000,
        ServiceHandle = 0x00000001,
        AllInterfaceClasses = 0x00000004
    }
}
