using System;
using System.Collections.Generic;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;

namespace UsbDeviceInformationCollectorCore
{
    public class UsbDeviceInfoCollectorConfiguration
    {
        public Action<(List<Device> Devices, DeviceStatus Status)> DeviceChangeAction { get; set; }
        public Action<(List<Device> Devices, DeviceStatus Status)> OtherDeviceChangeAction { get; set; }
        public Action<(List<UsbHubProperties> Devices, DeviceStatus Status)> HubChangeAction { get; set; }
        public Action<WndProcDelegate> AddHook { get; set; }
        public IntPtr WindowHandle { get; set; }
    }
}