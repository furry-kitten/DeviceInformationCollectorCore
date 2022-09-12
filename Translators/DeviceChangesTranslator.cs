using NLog;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using UsbDeviceInformationCollectorCore.Models;
using UsbDeviceInformationCollectorCore.Services;
using UsbDeviceInformationCollectorCore.Utils;

namespace UsbDeviceInformationCollectorCore.Translators
{
    internal class DeviceChangesTranslator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _devicePath = string.Empty;
        private readonly DeviceService _deviceService = DeviceService.Instance;

        internal DeviceChangesTranslator(IntPtr deviceInfoHandle)
        {
            var devType = (DbtDevTyp) Marshal.ReadIntPtr(deviceInfoHandle, 4);
            switch (devType)
            {
                case DbtDevTyp.DevTypDeviceInterface:
                    DeviceInfo = Marshal.PtrToStructure(deviceInfoHandle, typeof(DevBroadcastDeviceInterface));
                    var pathArray = (DeviceInfo as DevBroadcastDeviceInterface)?.Name ?? Array.Empty<char>();
                    _devicePath = string.Join("", pathArray);
                    HardwareId = Regex.Match(_devicePath, @"(.){4}vid_[\d|a-z]{4}[#&]?pid_[\d|a-z]{4}(.+)\{", RegexOptions.IgnoreCase).Value.Replace("#{", "");
                    break;
                default:
                    DeviceInfo = new DevBroadcastHdr();
                    Logger.Debug($"Unhandled {nameof(DbtDevTyp)}: {DeviceInfo}");
                    break;
            }
        }

        internal object DeviceInfo { get; }
        internal static bool IsFullSet { get; private set; }

        internal string HardwareId { get; }

        internal string VidPid => Regex.Match(_devicePath, @"vid_[\d|a-z]{4}[#&]?pid_[\d|a-z]{4}", RegexOptions.IgnoreCase).Value;

        public override string ToString()
        {
            if (DeviceInfo is DevBroadcastDeviceInterface)
            {
                var temp = (DevBroadcastDeviceInterface) DeviceInfo;
                return $"size:{temp.Size}, type: {temp.DeviceType}, reserved: {temp.Reserved}, name:{new string(temp.Name)}";
            }
            else
            {
                var temp = (DevBroadcastHdr) DeviceInfo;
                return $"size:{temp.Size}, type: {temp.DeviceType}, reserved: {temp.Reserved}";
            }
        }

        internal void RemoveDevice()
        {
            if (string.IsNullOrEmpty(HardwareId) || IsFullSet)
            {
                return;
            }

            var device = new DeviceProperties
            {
                Id = HardwareId
            };
            Logger.Debug($"Removing device with hardware id {HardwareId}");
            _deviceService.RemoveDevice(device);
        }

        internal static void FullSetUsbDevice()
        {
            Logger.Info($"{nameof(FullSetUsbDevice)} started");
            IsFullSet = true;
            DeviceService.Instance.Clear();
            SetHubs();
            SetDevices();
            IsFullSet = false;
        }

        private static void SetHubs()
        {
            var deviceService = DeviceService.Instance;
            var reader = new UsbPortsReader();
            var hubsProperties = reader.ReadAllHubs().OrderBy(hub => hub.Path).ToList();
            deviceService.AddHub(hubsProperties);
        }

        internal static void SetDevices()
        {
            var reader = UsbPortsReader.Instance;
            var deviceService = DeviceService.Instance;
            Logger.Info(
                $"(after adding hubs){nameof(DeviceCache)} in {nameof(FullSetUsbDevice)}:{Environment.NewLine}\t{nameof(DeviceCache.Instance.UsbHubs)}: {DeviceCache.Instance.UsbHubs.ToJson()}{Environment.NewLine}\t{nameof(DeviceCache.Instance.HubsAliases)}: {DeviceCache.Instance.HubsAliases.ToJson()}{Environment.NewLine}\t{nameof(DeviceCache.Instance.NeededDevices)}: {DeviceCache.Instance.NeededDevices.ToJson()}{Environment.NewLine}\t{nameof(DeviceCache.Instance.OtherDevices)}: {DeviceCache.Instance.OtherDevices.ToJson()}");
            var allDevices = reader.GetPropertiesForAllDevices().GroupBy(properties => properties.ShortPath).ToList();
            var devices = allDevices
                .Select(propertiesGroup => new Device
                {
                    ShortPath = propertiesGroup.Key,
                    Properties = propertiesGroup.ToArray()
                })
                .ToList();
            deviceService.AddDevices(devices);
            Logger.Info($"All devices properties:{Environment.NewLine}\t{devices.ToJson()}");
            Logger.Debug(
                $"{nameof(DeviceCache)} in {nameof(FullSetUsbDevice)}:{Environment.NewLine}\t{nameof(DeviceCache.Instance.UsbHubs)}: {DeviceCache.Instance.UsbHubs.ToJson()}{Environment.NewLine}\t{nameof(DeviceCache.Instance.HubsAliases)}: {DeviceCache.Instance.HubsAliases.ToJson()}{Environment.NewLine}\t{nameof(DeviceCache.Instance.NeededDevices)}: {DeviceCache.Instance.NeededDevices.ToJson()}{Environment.NewLine}\t{nameof(DeviceCache.Instance.OtherDevices)}: {DeviceCache.Instance.OtherDevices.ToJson()}");
        }

        /// <summary>
        ///     Enumerate all USB devices and look for the device whose VID and PID are provided.
        /// </summary>
        /// <returns>True the Device is found.</returns>
        internal void AddDeviceToList(bool getComPort = false)
        {
            if (IsFullSet)
            {
                return;
            }

            var devicePath = Regex.Match(_devicePath, @"vid_[\d|a-z]{4}[#&]?pid_[\d|a-z]{4}", RegexOptions.IgnoreCase).Value;
            if (string.IsNullOrEmpty(devicePath))
            {
                return;
            }

            Logger.Debug($"Serching a device {devicePath}");
            var reader = new UsbPortsReader();
            var deviceProperties = reader.ReadDeviceProperties(devicePath, getComPort);
            Logger.Debug($"Found: {deviceProperties.ToJson()}");
            if (deviceProperties == null)
            {
                Logger.Debug($"Hub found with {devicePath}. Start getting all devices.");
                SetHubs();
                _deviceService.UpdateDevices();
                return;
            }

            var devices = deviceProperties.GroupBy(prop => prop.ShortPath)
                .Select(propertiesGroup => new Device
                {
                    ShortPath = propertiesGroup.Key,
                    Properties = propertiesGroup.ToArray()
                })
                .ToList();
            Logger.Debug($"Adding devices: {devices.ToJson()}");
            _deviceService.AddDevices(devices);
        }
    }
}