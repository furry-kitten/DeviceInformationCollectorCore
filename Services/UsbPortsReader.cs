using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using UsbDeviceInformationCollectorCore.CLibs;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class UsbPortsReader
    {
        private const string HubPattern = @"hub$";
        private const string RootUsbPathPattern = @"^PCIROOT\(\d+\)#PCI\(\d+\)#USBROOT\(\d+\)$";

        private readonly LibrariesWorker _worker = new();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        internal static UsbPortsReader Instance { get; } = new();

        internal List<DeviceProperties> GetPropertiesForAllDevices()
        {
            _worker.ResetSetupApi();
            var allProperties = new List<DeviceProperties>();
            if (!_worker.SetupApi.IsDeviceInfoValidValue)
            {
                return allProperties;
            }

            var isSuccess = true;
            for (uint i = 0; isSuccess; i++)
            {
                var canFindDevice = _worker.SetupApi.CanFindDevice(i);
                isSuccess = _worker.SetupApi.IsSuccess;
                if (canFindDevice == false)
                {
                    break;
                }

                var deviceHardwareId = _worker.SetupApi.GetDeviceHardwareId();
                var deviceProperties = GetDeviceProperties(deviceHardwareId, true);
                if (deviceProperties == null)
                {
                    continue;
                }

                allProperties.Add(deviceProperties);
            }

            return allProperties;
        }

        internal List<DeviceProperties> ReadDeviceProperties(string expectedDeviceId, bool getComPort = false)
        {
            if (_worker.SetupApi.IsDeviceInfoValidValue == false || string.IsNullOrEmpty(expectedDeviceId))
            {
                return null;
            }

            List<DeviceProperties> allProperties = new();
            var isSuccess = true;
            _worker.ResetSetupApi();
            for (uint i = 0; isSuccess; i++)
            {
                var canFindDevice = _worker.SetupApi.CanFindDevice(i);
                isSuccess = _worker.SetupApi.IsSuccess;
                if (canFindDevice == false)
                {
                    break;
                }

                var deviceHardwareId = _worker.SetupApi.GetDeviceHardwareId();
                if (deviceHardwareId.ToLowerInvariant()
                        .Contains(expectedDeviceId.ToLowerInvariant()) ==
                    false)
                {
                    continue;
                }

                var deviceProperties = GetDeviceProperties(deviceHardwareId, getComPort);
                if (deviceProperties == null)
                {
                    return null;
                }

                allProperties.Add(deviceProperties);
            }

            if (allProperties.Any() == false)
            {
                _logger.Error($"There is now one device with id \"{expectedDeviceId}\"");
            }

            return allProperties;
        }

        internal List<UsbHubProperties> ReadAllHubs()
        {
            _worker.ResetSetupApi();
            if (!_worker.SetupApi.IsDeviceInfoValidValue)
            {
                return new List<UsbHubProperties>();
            }

            List<UsbHubProperties> allProperties = new();
            var isSuccess = true;
            for (uint i = 0; isSuccess; i++)
            {
                var canFindDevice = _worker.SetupApi.CanFindDevice(i);
                isSuccess = _worker.SetupApi.IsSuccess;
                if (canFindDevice == false)
                {
                    break;
                }

                var deviceProperties = GetUsbHubProperties();
                if (IsHub(deviceProperties))
                {
                    allProperties.Add(deviceProperties);
                }
            }

            return allProperties;
        }

        private bool IsHub(UsbHubProperties properties) =>
            (!string.IsNullOrEmpty(properties?.Path) &&
             Regex.IsMatch(properties.Path, RootUsbPathPattern, RegexOptions.IgnoreCase)) ||
            (!string.IsNullOrEmpty(properties?.BusReportedDeviceDesc) &&
             Regex.IsMatch(properties.BusReportedDeviceDesc, HubPattern, RegexOptions.IgnoreCase));

        private DeviceProperties GetDeviceProperties(string deviceHardwareId, bool getComPort = false)
        {
            var deviceProperties = GetUsbHubProperties();
            if (IsHub(deviceProperties))
            {
                return null;
            }

            deviceProperties.HardwareId = deviceHardwareId;
            deviceProperties.FriendlyName = _worker.SetupApi.GetDeviceFriendlyName();
            deviceProperties.Type = _worker.SetupApi.GetDeviceType();
            deviceProperties.Manufacturer = _worker.SetupApi.GetDeviceManufacturer();
            deviceProperties.Location = _worker.SetupApi.GetDeviceLocationInformation();
            deviceProperties.Description = _worker.SetupApi.GetDeviceDescription();
            deviceProperties.Address = _worker.SetupApi.GetDeviceAddress();
            if (getComPort)
            {
                deviceProperties.ComPort = _worker.GetComPort();
            }

            return deviceProperties;
        }

        private DeviceProperties GetUsbHubProperties()
        {
            DeviceProperties deviceProperties = new()
            {
                Path = _worker.SetupApi.GetDeviceLocalPaths(),
                PhysicalObjectName = _worker.SetupApi.GetDevicePhysicalDeviceObjectName(),
                Class = _worker.SetupApi.GetDeviceClass(),
                Id = _worker.SetupApi.GetDeviceId(),
                BusReportedDeviceDesc = _worker.SetupApi.GetBusReportedDeviceDescription(),
                HardwareId = _worker.SetupApi.GetDeviceHardwareId()
            };

            deviceProperties.PnpClassesTypes = deviceProperties.Class switch
            {
                nameof(PnPDeviceClassType.AndroidUsbDeviceClass) => PnPDeviceClassType.AndroidUsbDeviceClass,
                nameof(PnPDeviceClassType.Modem) => PnPDeviceClassType.Modem,
                nameof(PnPDeviceClassType.USB) => PnPDeviceClassType.USB,
                nameof(PnPDeviceClassType.WPD) => PnPDeviceClassType.WPD,
                nameof(PnPDeviceClassType.HIDClass) => PnPDeviceClassType.HIDClass,
                nameof(PnPDeviceClassType.USBDevice) => PnPDeviceClassType.USBDevice,
                null => PnPDeviceClassType.USBDevice,
                nameof(PnPDeviceClassType.MEDIA) => PnPDeviceClassType.MEDIA,
                _ => PnPDeviceClassType.None
            };

            deviceProperties.HardwareId = deviceProperties.Id;
            return deviceProperties;
        }
    }
}