using System;

namespace UsbDeviceInformationCollectorCore.Enums
{
    [Flags]
    public enum PnPDeviceClassType
    {
        None = 1,
        WPD = 2,
        AndroidUsbDeviceClass = 4,
        USB = 8,
        Modem = 16,
        USBDevice = 32,
        HIDClass = 64,
        MEDIA = 128
    }
}