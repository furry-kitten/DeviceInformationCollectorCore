using System;

namespace UsbDeviceInformationCollectorCore.CLibs.Enums
{
    [Flags]
    public enum DbtDevice : uint
    {
        DeviceArrival = 0x8000,                 //A device has been inserted and is now available. 
        DeviceQueryRemove = 0x8001,             //Permission to remove a device is requested. Any application can deny this request and cancel the removal.
        DeviceQueryRemoveFailed = 0x8002,       //Request to remove a device has been canceled.
        DeviceRemovePending = 0x8003,           //Device is about to be removed. Cannot be denied.
        DeviceRemoveComplete = 0x8004,          //Device has been removed.
        DeviceTypeSpecific = 0x8005,            //Device-specific event.
        CustomEvent = 0x8006,                    //User-defined event
        DevNodesChanged = 0x0007,
        QueryChangeConfig = 0x0017,
        ConfigChanged = 0x0018,
        ConfigChangeCanceled = 0x0019,
        UserDefined = 0xFFFF
    }
}
