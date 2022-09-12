using DynamicData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;
using UsbDeviceInformationCollectorCore.Utils;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class DeviceService
    {
        private const string HubPattern = @"hub$";
        private const string RootUsbPathPattern = @"^PCIROOT\(\d+\)#PCI\(\d+\)#USBROOT\(\d+\)$";
        private const string DeviceUsbPathPattern = @"#USBROOT\(\d+\)(#USB\(\d+\))+";
        private readonly DeviceCache _deviceCache = DeviceCache.Instance;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private DeviceService() { }

        internal static DeviceService Instance { get; } = new DeviceService();

        public string ChangeDevicePath(string devicePath)
        {
            if (string.IsNullOrEmpty(devicePath))
            {
                return string.Empty;
            }

            var hub = _deviceCache.UsbHubs.FirstOrDefault(h => h.Path.Length < devicePath.Length && devicePath.StartsWith(h.Path));
            var pattern = $@"{hub?.Path.Replace("(", "\\(").Replace(")", "\\)")}(#USB\(\d+\)){{1,2}}(#USBMI\(\d+\))?$";
            if (hub is null || Regex.IsMatch(devicePath, pattern, RegexOptions.IgnoreCase) == false)
            {
                return devicePath;
            }

            var hubAlias = _deviceCache.HubsAliases[hub.Path];
            if (hubAlias <= 0)
            {
                return devicePath;
            }

            var newDevicePath = devicePath.Remove(0, hub.Path.Length);
            var usbNumbers = GetUsbNumbers(newDevicePath);
            return usbNumbers.Length <= 0 ? devicePath : $"UsbHub({hubAlias})#USB({usbNumbers[0]})";
        }

        internal static bool IsDeviceHub(DeviceProperties properties) =>
            !string.IsNullOrEmpty(properties?.Path) &&
            Regex.IsMatch(properties.Path, RootUsbPathPattern, RegexOptions.IgnoreCase) ||
            !string.IsNullOrEmpty(properties?.BusReportedDeviceDesc) &&
            Regex.IsMatch(properties.BusReportedDeviceDesc, HubPattern, RegexOptions.IgnoreCase);

        internal static int[] GetUsbNumbers(string devicePath)
        {
            var usbMatch = Regex.Matches(devicePath, @"\d+");
            var usbNumberPath = (from object number
                                     in usbMatch
                                 select int.Parse(number?.ToString() ?? string.Empty))
                .ToList();
            return usbNumberPath
                .ToArray();
        }

        internal static bool IsDeviceUsbPath(DeviceProperties device) => Regex.IsMatch(device.Path, DeviceUsbPathPattern);

        internal bool? RemoveDevice(DeviceProperties properties)
        {
            if (properties == null)
            {
                return null;
            }

            var existDeviceInfo = _deviceCache.GetDevice(existDevice => existDevice.Properties.Any(existDeviceProperties => IsIdsEquals(existDeviceProperties, properties)));
            if (existDeviceInfo == null)
            {
                _logger.Debug($"Removing hub: {properties.Id}");
                RemoveHub(properties.Id);
                return false;
            }

            _logger.Debug($"Removing device: {existDeviceInfo.ToJson()}");
            _deviceCache.RemoveDevices(new List<Device>
            {
                existDeviceInfo
            });
            return true;
        }

        internal void RemoveHub(string hubHardwareId)
        {
            var changedHardwareId = hubHardwareId.Replace("#", @"\");
            var hub = _deviceCache.UsbHubs.FirstOrDefault(h => string.Equals(h.HardwareId, changedHardwareId, StringComparison.CurrentCultureIgnoreCase));
            if (hub is null)
            {
                return;
            }

            _deviceCache.RemoveHub(hub);
        }

        internal void AddHub(UsbHubProperties hub) => _deviceCache.AddHub(hub);

        internal void AddHub(List<UsbHubProperties> hub) => _deviceCache.AddHub(hub);

        internal void AddDevices(List<Device> devices)
        {
            if (devices == null || devices.Any() == false)
            {
                return;
            }

            devices.RemoveMany(devices.Where(device =>
                device.Properties.Any() == false ||
                device.Properties.All(properties => string.IsNullOrEmpty(properties.Path))));
            var newDevices = new List<Device>();
            var updatedDevices = new List<Device>();
            foreach (var device in devices)
            {
                var existDeviceInfo = _deviceCache.GetDevice(existDevice => existDevice.Properties.Any(existDeviceProperties => IsPathsEquals(existDeviceProperties, device.Properties[0]))) ??
                    newDevices.FirstOrDefault(newDevice => newDevice.Properties.Any(newDeviceProperties => IsPathsEquals(newDeviceProperties, device.Properties[0])));
                if (existDeviceInfo != null)
                {
                    if (device.Properties.Length == existDeviceInfo.Properties.Length)
                    {
                        continue;
                    }

                    AddPropertiesToDevice(device.Properties.ToList());
                    SetDevice(device, device.Properties);
                    updatedDevices.Add(device);
                    _logger.Debug($"Update device: {device.ToJson()}");
                    continue;
                }

                SetDevice(device, device.Properties);
                newDevices.Add(device);
                _logger.Debug($"Add new device: {device.ToJson()}");
            }

            _deviceCache.AddDevices(newDevices);
            _deviceCache.UpdateDevice(updatedDevices);
        }

        internal bool? AddPropertiesToDevice(List<DeviceProperties> propertiesArray)
        {
            if (propertiesArray == null || propertiesArray.All(properties => string.IsNullOrEmpty(properties.ShortPath)))
            {
                return null;
            }

            var device = _deviceCache.GetDevice(d => d.ShortPath == propertiesArray.First(properties => string.IsNullOrEmpty(properties.ShortPath) == false).ShortPath) ??
                new Device
                {
                    Properties = Array.Empty<DeviceProperties>()
                };
            bool? result = false;
            foreach (var deviceProperties in propertiesArray)
            {
                result |= AddPropertiesToDevice(device, deviceProperties);
                _logger.Debug($"Adding/updating propeties ({deviceProperties.ToJson()}) to device {device.Properties[0].Path} ({device.ShortPath})");
            }

            return result;
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

        internal bool? AddPropertiesToDevice(Device device, DeviceProperties properties)
        {
            if (IsDeviceUsbPath(properties) == false || string.IsNullOrEmpty(properties.ShortPath))
            {
                return null;
            }

            device ??= new Device();
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
                        device.Manufacture ??= properties.Manufacturer;
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

            if (string.IsNullOrEmpty(properties.ShortPath) == false)
            {
                device.ShortPath ??= properties.ShortPath;
            }

            var existProperties = device.Properties.FirstOrDefault(existDeviceProperties =>
                existDeviceProperties.PnpClassesTypes == properties.PnpClassesTypes &&
                (existDeviceProperties.Path == properties.Path ||
                    existDeviceProperties.Path.Contains(properties.Path) ||
                    properties.Path.Contains(existDeviceProperties.Path)));
            if (existProperties != null)
            {
                existProperties.Path ??= properties.Path;
                existProperties.Address ??= properties.Address;
                existProperties.ComPort ??= properties.ComPort;
                existProperties.Class ??= properties.Class;
                existProperties.Description ??= properties.Description;
                existProperties.Id ??= properties.Id;
                existProperties.Location ??= properties.Location;
                existProperties.Manufacturer ??= properties.Manufacturer;
                existProperties.PhysicalObjectName ??= properties.PhysicalObjectName;
                existProperties.Type ??= properties.Type;
                existProperties.FriendlyName ??= properties.FriendlyName;
                existProperties.ShortPath = properties.ShortPath;
                return false;
            }

            var propertiesList = device.Properties.ToList();
            propertiesList.Add(properties);
            device.Properties = propertiesList.ToArray();
            return true;
        }

        private bool IsIdsEquals(DeviceProperties existDeviceProperties, DeviceProperties device)
        {
            var deviceId = device.Id.Replace("#", @"\");
            return existDeviceProperties.Id.ToLower() == deviceId.ToLower();
        }

        private bool IsPathsEquals(DeviceProperties existDeviceProperties, DeviceProperties newDeviceProperties)
        {
            var isShortPathEquals = existDeviceProperties.ShortPath == newDeviceProperties.ShortPath;
            if (isShortPathEquals)
            {
                return true;
            }

            if (newDeviceProperties.Path == newDeviceProperties.ShortPath)
            {
                return existDeviceProperties.Path == newDeviceProperties.Path ||
                    existDeviceProperties.Path.Contains(newDeviceProperties.Path) ||
                    newDeviceProperties.Path.Contains(existDeviceProperties.Path);
            }

            return false;
        }

        private string ExtractDeviceId(string hardwareDeviceId)
        {
            var possibleDeviceId =
                Regex.Replace(hardwareDeviceId, @"(.+)\\{1}\b", string.Empty);
            return Regex.Match(possibleDeviceId, @"^[\w|-]{3,}$").Value;
        }

        internal void Clear() => _deviceCache.ClearAll();

        internal void SilentClear() => _deviceCache.ClearAllSilent();

        internal void UpdateDevices()
        {
            var deviceProperties = _deviceCache.NeededDevices.Select(device => device.Properties).ToList();
            deviceProperties.Add(_deviceCache.OtherDevices.Select(device => device.Properties).ToList());
            var allProperties = deviceProperties.SelectMany(props => props).ToList();
            var allDevices = allProperties.GroupBy(properties => properties.ShortPath).ToList();
            foreach (var properties in allProperties)
            {
                properties.ShortPath = ChangeDevicePath(properties.Path);
            }

            var devicesId = allProperties.Where(properties => string.IsNullOrEmpty(ExtractDeviceId(properties.Id)) == false)
                .ToList();
            var neededDevices = devicesId.Select(properties => _deviceCache.GetDevice(device => device.Id == ExtractDeviceId(properties.Id)))
                .Where(device => device != null)
                .ToList();
            neededDevices.ForEach(device => device.Properties = new[]
            {
                allProperties.First(properties => ExtractDeviceId(properties.Id) == device.Id)
            });
            foreach (var device in neededDevices)
            {
                device.Id = null;
                device.ComPort = null;
                device.Manufacture = null;
                device.ModelName = null;
                device.ModelNumber = null;
                device.ShortPath = null;
                SetDevice(device, device.Properties);
            }

            var devices = allDevices
                .Select(propertiesGroup => new Device
                {
                    ShortPath = propertiesGroup.Key,
                    Properties = propertiesGroup.ToArray()
                })
                .ToList();
            _deviceCache.RemoveDevices(_deviceCache.OtherDevices);
            AddDevices(devices);
        }
    }
}