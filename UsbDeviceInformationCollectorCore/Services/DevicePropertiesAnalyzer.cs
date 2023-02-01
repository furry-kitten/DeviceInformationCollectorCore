using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class DevicePropertiesAnalyzer
    {
        private const string VidPidInFullDeviceIdPattern = @"(.){4}vid_[\d|a-z]{4}[#&]?pid_[\d|a-z]{4}(.+)\{";
        private const string DeviceHardwareIdPattern = @"(.+)\\{1}\b";
        private const string DeviceIdPattern = @"^[\w|-]{3,}$";
        private const string VidPidPattern = @"vid_[\d|a-z]{4}[#&]?pid_[\d|a-z]{4}";
        private const string DevicePathFormat = @"{0}(#USB\(\d+\)){{1,2}}(#USBMI\(\d+\))?$";
        private const string ShortPathFormat = "UsbHub({0})#USB({1})";
        internal static readonly DevicePropertiesAnalyzer Instance = new();

        private DevicePropertiesAnalyzer() { }

        internal bool IsHardwareIdEquals(string hardwareId1, string hardwareId2) =>
            string.IsNullOrEmpty(hardwareId1) == false &&
            string.IsNullOrEmpty(hardwareId1) == false &&
            string.Equals(GetGeneralHardwareId(hardwareId1), GetGeneralHardwareId(hardwareId2),
                StringComparison.InvariantCultureIgnoreCase);

        internal string ExtractDeviceId(string hardwareDeviceId)
        {
            if (string.IsNullOrEmpty(hardwareDeviceId))
            {
                return string.Empty;
            }

            var possibleDeviceId =
                Regex.Replace(hardwareDeviceId, DeviceHardwareIdPattern, string.Empty);

            return Regex.Match(possibleDeviceId, DeviceIdPattern)
                .Value;
        }

        internal string GetGeneralHardwareId(string id) => id?.Replace("#", @"\");

        internal string GetHardwareIdFromDevicePath(string devicePath) =>
            Regex.Match(devicePath, VidPidInFullDeviceIdPattern,
                    RegexOptions.IgnoreCase)
                .Value.Replace("#{", "");

        internal string GetVidPid(string hardwareId) =>
            Regex.Match(hardwareId, VidPidPattern, RegexOptions.IgnoreCase)
                .Value;

        internal string SimplifyDevicePath(string devicePath, Dictionary<string, int> hubsAliases)
        {
            if (string.IsNullOrEmpty(devicePath))
            {
                return string.Empty;
            }

            var hub = hubsAliases.FirstOrDefault(hubAlias =>
                hubAlias.Key.Length < devicePath.Length && devicePath.StartsWith(hubAlias.Key));

            if (string.IsNullOrEmpty(hub.Key))
            {
                return devicePath;
            }

            var changedPath = hub.Key.Replace("(", "\\(")
                .Replace(")", "\\)");

            var pattern = string.Format(DevicePathFormat, changedPath);
            if (Regex.IsMatch(devicePath, pattern, RegexOptions.IgnoreCase) == false)
            {
                return devicePath;
            }

            var hubAlias = hubsAliases[hub.Key];
            if (hubAlias <= 0)
            {
                return devicePath;
            }

            var newDevicePath = devicePath.Remove(0, hub.Key.Length);
            var usbNumbers = GetUsbNumbers(newDevicePath);
            return usbNumbers.Length <= 0 ? devicePath : string.Format(ShortPathFormat, hubAlias, usbNumbers[0]);
        }

        private int[] GetUsbNumbers(string devicePath)
        {
            var usbMatch = Regex.Matches(devicePath, @"\d+");
            var usbNumberPath = usbMatch
                .Select(num => int.Parse(num?.ToString() ?? string.Empty))
                .ToArray();

            return usbNumberPath;
        }
    }
}