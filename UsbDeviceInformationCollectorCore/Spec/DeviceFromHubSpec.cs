using System;
using System.Text.RegularExpressions;
using UsbDeviceInformationCollectorCore.Models;

namespace UsbDeviceInformationCollectorCore.Spec
{
    internal class DeviceFromHubSpec
    {
        private const string ShortPathFormat = @"UsbHub\({0}\)#USB\(\d+\)";

        internal bool IsDeviceFromHub(UsbHubProperties hub, Device device) =>
            IsDeviceFromHub(hub)
                .Invoke(device);

        internal Func<Device, bool> IsDeviceFromHub(UsbHubProperties hub)
        {
            var shortPathPattern = GetShortPathPattern(hub.Id);
            return device => Regex.IsMatch(device.ShortPath, shortPathPattern, RegexOptions.IgnoreCase);
        }

        private string GetShortPathPattern(string hubId) => string.Format(ShortPathFormat, hubId);
    }
}