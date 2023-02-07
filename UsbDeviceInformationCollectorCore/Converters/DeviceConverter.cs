using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DynamicData;
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
        private readonly RegistrySearcher _searcher = new();

        private DeviceConverter() { }

        internal Device ConvertDevice(IGrouping<string, DeviceProperties> groupedDeviceProperties)
        {
            var deviceProperties = groupedDeviceProperties.ToArray();
            Device device = new();
            SetDevice(device, deviceProperties);
            device.Properties = deviceProperties;
            ResetModelNumberIfNeeded(device);
            return device;
        }

        internal List<Device> ConvertDevices(List<DeviceProperties> allDevicesProperties,
            Dictionary<string, int> hubsAliases)
        {
            var devicesProperties = GroupByShortPath(allDevicesProperties, hubsAliases);
            return ConvertDevices(devicesProperties);
        }

        internal List<Device> ConvertDevices(string originalId, List<DeviceProperties> allDevicesProperties,
            Dictionary<string, int> hubsAliases)
        {
            var devicesProperties = GroupByShortPath(allDevicesProperties, hubsAliases);
            return ConvertDevices(devicesProperties);
        }

        internal List<Device> ConvertDevices(List<IGrouping<string, DeviceProperties>> devicesProperties) =>
            devicesProperties.Select(ConvertDevice)
                .ToList();

        internal List<Device> ConvertDevices1(List<DeviceProperties> allDevicesProperties,
            Dictionary<string, int> hubsAliases)
        {
            var ids = allDevicesProperties.Select(properties => _analyzer.GetVidPid(properties.Id))
                .Distinct()
                .ToList();

            var allProperties = allDevicesProperties.ToList();
            List<(string Id, DeviceProperties Properties)> groupedProperties = new();
            foreach (var id in ids)
            {
                groupedProperties.AddRange(allProperties.Where(properties => Predicate(properties, id))
                    .Select(properties => (id, properties)));

                allProperties.RemoveAll(properties => Predicate(properties, id));
            }

            var devicesProperties = groupedProperties.GroupBy(tuple => tuple.Id, tuple => tuple.Properties)
                .ToList();

            List<Device> devices = new();
            foreach (var byId in devicesProperties.Select(grouping => grouping.ToList()))
            {
                var groupByShortPath = GroupByShortPath(byId, hubsAliases);
                devices.Add(ConvertDevices(groupByShortPath));
            }

            return devices;
        }


        bool Predicate(DeviceProperties properties, string id) =>
            string.Equals(_analyzer.GetVidPid(properties.Id), id, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(_analyzer.GetVidPid(properties.ParentId), id, StringComparison.OrdinalIgnoreCase);

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
                    case PnPDeviceClassType.HIDClass:
                    case PnPDeviceClassType.WPD:
                        SetModelInformation(device, properties);
                        break;

                    case PnPDeviceClassType.Modem:
                        if (string.IsNullOrEmpty(properties.ComPort) == false)
                        {
                            device.ComPort = properties.ComPort;
                            //var tuple = Foo(properties.ComPort);
                            //if (tuple == default)
                            //{
                            //    return;
                            //}

                            //device.SerialNumber = tuple.SerialNumber;
                            //device.Imei = tuple.Imei;
                        }

                        break;
                }

                TrySetDeviceId(device, properties);
                if (string.IsNullOrEmpty(properties.Manufacturer) == false)
                {
                    device.Manufacture ??= properties.Manufacturer;
                }

                if (string.IsNullOrEmpty(properties.ShortPath) == false)
                {
                    device.ShortPath ??= properties.ShortPath;
                }
            }

            if ((allDeviceProperties.Length > 1 || device.ModelName != device.ModelNumber) && string.IsNullOrWhiteSpace(device.ModelNumber) == false)
            {
                return;
            }

            device.ModelNumber = allDeviceProperties.First()
                .BusReportedDeviceDesc;
        }

        internal (string Models, string SerialNumber, string Imei) Foo(string PortName)
        {
            if (string.IsNullOrEmpty(PortName))
            {
                return default;
            }

            string key = "";
            //SerialPort serialPort = new (PortName);
            //if (!serialPort.IsOpen)
            //    serialPort.Open();
            //serialPort.Write("AT+DEVCONINFO\r\n");
            //Task.Delay(3000).GetAwaiter().GetResult();
            //key = serialPort.ReadExisting();
            //serialPort.Close();
            var model = Regex.Match(key, @"mn\(\w*-\w*\)", RegexOptions.IgnoreCase).Value;
            var serialNumber = Regex.Match(key, @"sn\(\w*\)", RegexOptions.IgnoreCase).Value;
            var imei = Regex.Match(key, @"imei\(\d*\)", RegexOptions.IgnoreCase).Value;
            model = GetValue(model);
            serialNumber = GetValue(serialNumber);
            imei = GetValue(imei);
            return (Models: model, SerialNumber: serialNumber, Imei: imei);
            //var deviceHandle = ti.hSource; // a handle obtained from WM_TOUCH message.
            //if (devBufer == null)
            //    devBufer = new StringBuilder(4096 * 2);
            //devBufer.Clear();

            //uint returnedDataSize = (uint)devBufer.Capacity;
            //var firstCall = GetRawInputDeviceInfo(deviceHandle, DeviceInfoTypes.RIDI_DEVICENAME, devBufer, ref returnedDataSize);
            //var firstError = Marshal.GetLastWin32Error();
            //var firtsDataSize = returnedDataSize;
            //var devName = devBufer.ToString();
            //if (string.IsNullOrEmpty(devName))
            //    devName = "No name retrieved";

            //var devInfo = new RID_DEVICE_INFO();
            //var structureSize = Convert.ToUInt32(Marshal.SizeOf<RID_DEVICE_INFO>());
            //devInfo.cbSize = structureSize;
            //returnedDataSize = structureSize;
            //var secondCall = GetRawInputDeviceInfo(deviceHandle, DeviceInfoTypes.RIDI_DEVICEINFO, ref devInfo, ref returnedDataSize);
            //var secondError = Marshal.GetLastWin32Error();
            //string hidData = "ERROR: hid data retrieval failed";
            //if (devInfo.dwType == 2)
            //{
            //    hidData = devInfo.hid.ToString();
            //}

            //Debug.LogWarning($"New touch device detected with name '{devName}' and handle '{ti.hSource}'. " +
            //                 $"\nWinapi calls returned '{(int)firstCall}' per name and '{(int)secondCall}' per data " +
            //                 $"\nError codes were '{firstError}' per name and '{secondError}' per data" +
            //                 $"\nData sizes were '{firtsDataSize}' per name and '{returnedDataSize}' per data" +
            //                 $"\nHid data structure size: '{structureSize}'; Structure type is '{devInfo.dwType}'" +
            //                 $"\nHid data: {hidData}");
        }

        private static string GetValue(string data)
        {
            return !string.IsNullOrEmpty(data) ? data.Remove(data.Length - 1)[(data.LastIndexOf('(') + 1)..] : null;
        }

        private static bool IsDeviceUsbPath(UsbHubProperties device) =>
            Regex.IsMatch(device.Path, DeviceUsbPathPattern);

        internal static string ExtractDeviceId(string hardwareDeviceId)
        {
            var possibleDeviceId =
                Regex.Replace(hardwareDeviceId, @"(.+)\\{1}\b", string.Empty);

            return Regex.Match(possibleDeviceId, @"^[\w|-]{3,}$")
                .Value;
        }

        private static void ResetModelNumberIfNeeded(Device device)
        {
            var properties = device.Properties.FirstOrDefault(deviceClass =>
                deviceClass.PnpClassesTypes is PnPDeviceClassType.USB or PnPDeviceClassType.USBDevice &&
                string.IsNullOrEmpty(device.Id) == false &&
                string.IsNullOrEmpty(deviceClass.Id) == false &&
                string.IsNullOrEmpty(device.ModelName) == false &&
                string.IsNullOrEmpty(deviceClass.Description) == false &&
                deviceClass.Id.ToLower()
                    .EndsWith(device.Id.ToLower()));

            if (properties == null ||
                device.ModelName != device.ModelNumber ||
                properties.BusReportedDeviceDesc == device.ModelNumber)
            {
                return;
            }

            device.ModelNumber = properties.BusReportedDeviceDesc;
        }

        private static void SetModelInformation(Device device, DeviceProperties properties)
        {
            if (string.IsNullOrEmpty(properties.FriendlyName) == false)
            {
                device.ModelName ??= properties.FriendlyName;
            }

            if (string.IsNullOrEmpty(properties.Description) == false)
            {
                device.ModelNumber ??= properties.Description;
            }
        }

        private void TrySetDeviceId(Device device, DeviceProperties properties)
        {
            if (string.IsNullOrEmpty(properties.Id))
            {
                return;
            }

            var extractedId = ExtractDeviceId(properties.Id);
            if (string.IsNullOrEmpty(extractedId))
            {
                return;
            }

            device.Id ??= extractedId;
            foreach (var diskId in _searcher.GetDisksIds().Where(diskId => extractedId.Equals(diskId, StringComparison.InvariantCultureIgnoreCase)))
            {
                    device.Type = DevicesTypes.Disk;
            }
        }
    }
}