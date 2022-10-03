using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;
using UsbDeviceInformationCollectorCore.Services;

namespace UsbDeviceInformationCollectorCore.Converters
{
    internal class DeviceConverter
    {
        private const string DeviceUsbPathPattern = @"#USBROOT\(\d+\)(#USB\(\d+\))+";
        public static DeviceConverter Instance = new();
        private readonly DevicePropertiesAnalyzer _analyzer = DevicePropertiesAnalyzer.Instance;

        private DeviceConverter() { }

        internal Device ConvertDevice(IGrouping<string, DeviceProperties> groupedDeviceProperties)
        {
            var deviceProperties = groupedDeviceProperties.ToList();
            Device device = new();
            foreach (var deviceClass in deviceProperties)
            {
                if (IsDeviceUsbPath(deviceClass) == false || string.IsNullOrEmpty(deviceClass.ShortPath))
                {
                    return null;
                }

                if (string.IsNullOrEmpty(deviceClass.Id) == false)
                {
                    var extractedId = ExtractDeviceId(deviceClass.Id);
                    if (string.IsNullOrEmpty(extractedId) == false)
                    {
                        device.Id ??= extractedId;
                    }
                }

                if (string.IsNullOrEmpty(deviceClass.ShortPath) == false)
                {
                    device.ShortPath ??= deviceClass.ShortPath;
                }

                switch (deviceClass.PnpClassesTypes)
                {
                    case PnPDeviceClassType.WPD:
                    {
                        if (string.IsNullOrEmpty(deviceClass.FriendlyName) == false)
                        {
                            device.ModelName ??= deviceClass.FriendlyName;
                        }

                        if (string.IsNullOrEmpty(deviceClass.Description) == false)
                        {
                            device.ModelNumber ??= deviceClass.Description;
                        }

                        if (string.IsNullOrEmpty(deviceClass.Manufacturer) == false)
                        {
                            device.Manufacture ??= deviceClass.Manufacturer;
                        }

                        break;
                    }

                    case PnPDeviceClassType.Modem:
                    {
                        if (string.IsNullOrEmpty(deviceClass.ComPort) == false)
                        {
                            device.ComPort = deviceClass.ComPort;
                        }

                        break;
                    }
                }
            }

            device.Properties = deviceProperties.ToArray();
            ResetModelNumberIfNeeded(device);
            return device;
        }

        internal List<Device> ConvertDevices(List<DeviceProperties> allDevicesProperties,
            Dictionary<string, int> hubsAliases)
        {
            var devicesProperties = GroupByShortPath(allDevicesProperties, hubsAliases);
            return ConvertDevices(devicesProperties);
        }

        internal List<Device> ConvertDevices(List<IGrouping<string, DeviceProperties>> devicesProperties) =>
            devicesProperties.Select(ConvertDevice)
                .ToList();

        internal List<IGrouping<string, DeviceProperties>> GroupByShortPath(List<DeviceProperties> allDevicesProperties,
            Dictionary<string, int> hubsAliases)
        {
            foreach (var properties in allDevicesProperties)
            {
                properties.ShortPath = _analyzer.SimplifyDevicePath(properties.Path, hubsAliases);
            }

            return allDevicesProperties.GroupBy(properties => properties.ShortPath)
                .ToList();
        }

        internal void SetDevice(Device device, DeviceProperties[] allDeviceProperties)
        {
            if (device == null || allDeviceProperties.Length <= 0)
            {
                return;
            }

            foreach (var properties in allDeviceProperties)
            {
                switch (properties.PnpClassesTypes)
                {
                    case PnPDeviceClassType.WPD:
                    {
                        if (string.IsNullOrEmpty(properties.FriendlyName) == false)
                        {
                            device.ModelName ??= properties.FriendlyName;
                        }

                        if (string.IsNullOrEmpty(properties.Description) == false)
                        {
                            device.ModelNumber ??= properties.Description;
                        }

                        if (string.IsNullOrEmpty(properties.Manufacturer) == false)
                        {
                            device.Manufacture = properties.Manufacturer;
                        }

                        break;
                    }

                    case PnPDeviceClassType.Modem:
                    {
                        if (string.IsNullOrEmpty(properties.ComPort) == false)
                        {
                            device.ComPort = properties.ComPort;
                        }

                        break;
                    }
                }

                if (string.IsNullOrEmpty(properties.Id) == false)
                {
                    var extractedId = ExtractDeviceId(properties.Id);
                    if (string.IsNullOrEmpty(extractedId) == false)
                    {
                        device.Id ??= extractedId;
                    }
                }

                if (string.IsNullOrEmpty(properties.Manufacturer) == false)
                {
                    device.Manufacture ??= properties.Manufacturer;
                }

                if (string.IsNullOrEmpty(properties.ShortPath) == false)
                {
                    device.ShortPath ??= properties.ShortPath;
                }
            }
        }

        private static bool IsDeviceUsbPath(UsbHubProperties device) =>
            Regex.IsMatch(device.Path, DeviceUsbPathPattern);

        private string ExtractDeviceId(string hardwareDeviceId)
        {
            var possibleDeviceId =
                Regex.Replace(hardwareDeviceId, @"(.+)\\{1}\b", string.Empty);

            return Regex.Match(possibleDeviceId, @"^[\w|-]{3,}$")
                .Value;
        }

        private void ResetModelNumberIfNeeded(Device device)
        {
            if (device.ModelName != device.ModelNumber)
            {
                return;
            }

            var properties = device.Properties.FirstOrDefault(deviceClass =>
                deviceClass.PnpClassesTypes is PnPDeviceClassType.USB or PnPDeviceClassType.USBDevice &&
                string.IsNullOrEmpty(device.Id) == false &&
                string.IsNullOrEmpty(deviceClass.Id) == false &&
                string.IsNullOrEmpty(device.ModelName) == false &&
                string.IsNullOrEmpty(deviceClass.Description) == false &&
                deviceClass.Id.EndsWith(device.Id));

            if (properties == null)
            {
                return;
            }

            device.ModelNumber = properties.BusReportedDeviceDesc;
        }
    }
}