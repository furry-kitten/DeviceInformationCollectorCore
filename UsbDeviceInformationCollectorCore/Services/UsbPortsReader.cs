using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using UsbDeviceInformationCollectorCore.CLibs;
using UsbDeviceInformationCollectorCore.CLibs.SetupApiDll;
using UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class UsbPortsReader
    {
        private const string HubPattern = @"hub$";
        private const string RootUsbPathPattern = @"^PCIROOT\(\d+\)#PCI\(\d+\)#USBROOT\(\d+\)$";

        private const ulong CM_GETIDLIST_FILTER_PRESENT = 0x00000100;
        private const uint CM_GET_DEVICE_INTERFACE_LIST_PRESENT = 0x0;
        private const int CR_SUCCESS = 0x0;
        private static Guid GUID_DEVINTERFACE_COMPORT = new("{86E0D1E0-8089-11D0-9CE4-08003E301F73}");
        private static Guid GUID_DEVINTERFACE_USB_DEVICE = new("{A5DCBF10-6530-11D2-901F-00C04FB951ED}");

        private readonly LibrariesWorker _worker = new();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        internal static UsbPortsReader Instance { get; } = new();

        public static string[] GetPortNames()
        {
            var cr = SetupApiDll.CM_Get_Device_Interface_List_Size(out var size, ref GUID_DEVINTERFACE_USB_DEVICE, null,
                CM_GET_DEVICE_INTERFACE_LIST_PRESENT);

            if (cr != CR_SUCCESS || size == 0)
            {
                return null;
            }

            var data = new char[size];
            cr = SetupApiDll.CM_Get_Device_Interface_List(ref GUID_DEVINTERFACE_USB_DEVICE, null, data,
                (uint)data.Length, CM_GET_DEVICE_INTERFACE_LIST_PRESENT);

            return cr == CR_SUCCESS ?
                new string(data).Split('\0')
                    .ToList()
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToArray() :
                null;
        }

        public List<string> BytesToStrings(byte[] rawData)
        {
            var strings = new List<string>();
            var limit = rawData.Length;
            var idx = 0;
            var processing = idx < limit;

            while (processing)
            {
                var x = idx;
                while (x < limit && 0 != rawData[x])
                {
                    ++x;
                }

                if (x >= limit)
                {
                    break;
                }

                var sz = x - idx;
                var bytes = new byte[sz];
                Array.Copy(rawData, idx, bytes, 0, sz);
                try
                {
                    var str = Encoding.Default.GetString(bytes);
                    strings.Add(str);
                    idx = x + 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    idx = limit;
                }

                processing = idx < limit;
            }

            return strings;
        }

        //internal List<DeviceProperties> GetPropertiesForAllDevicesByClasses()
        //{
        //    SetupApiByGiud setupApiByGiud = new();
        //    var ids = setupApiByGiud.Foo();
        //    return ReadDeviceProperties(ids.Select(id => id.ToString()).ToList());
        //}

        internal List<DeviceProperties> GetPropertiesForAllDevices()
        {
            _worker.SetupApi.SetGeneralData();
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

                var isSuccessDriver = true;
                for (uint j = 0; isSuccessDriver; j++)
                {
                    var data1 = _worker.SetupApi.GetDrvInfoDataV1(0, j, out isSuccessDriver);
                    if (isSuccessDriver)
                    {

                    }

                    var data2 = _worker.SetupApi.GetDrvInfoDataV2(0, j, out isSuccessDriver);
                    if (isSuccessDriver)
                    {

                    }
                }

                allProperties.Add(deviceProperties);
            }

            _worker.SetupApi.SetupDiDestroyDeviceInfoList();
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
            _worker.SetupApi.SetGeneralData();
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

            _worker.SetupApi.SetupDiDestroyDeviceInfoList();
            return allProperties;
        }

        internal List<DeviceProperties> ReadDeviceProperties(List<string> devicesPaths)
        {
            var r = GetPortNames();
            const ulong CM_GETIDLIST_FILTER_PRESENT = 0x00000100;

            uint idListLen = 0;
            string filter = null;
            var deviceInstanceIdStrs = new List<string>();

            var cmRet = SetupApiDll.CM_Get_Device_ID_List_Size(ref idListLen, 0, (uint)CM_GETIDLIST_FILTER_PRESENT);
            if (0 == cmRet)
            {
                var data = new byte[idListLen];
                cmRet = SetupApiDll.CM_Get_Device_ID_List(filter, data, idListLen, (uint)CM_GETIDLIST_FILTER_PRESENT);
                if (0 == cmRet)
                {
                    deviceInstanceIdStrs = BytesToStrings(data);
                }
            }

            var s = new SetupApiByGiud();
            var ids = devicesPaths.Select(devicePath => Regex
                    .Match(devicePath, @"({)?\w{8}-\w{4}-\w{4}-\w{4}-\w{12}(})?")
                    .Value)
                .Select(id => new Guid(id))
                .ToArray();

            //foreach (var interfaceId in interfacesIds)
            //{
            //    var guid = new Guid(interfaceId);
            //    SetupApiByGiud.Foo(guid);
            //    s.SetGeneralData(guid);
            //}

            List<DeviceProperties> allProperties = new();

            foreach (var interfaceId in ids)
            {
                s.SetGeneralData(interfaceId);
                //s.SetGeneralData(new Guid(interfaceId));
                if (s.IsDeviceInfoValidValue)
                {
                    continue;
                }

                for (uint i = 0; s.IsSuccess; i++)
                {
                    if (s.GetDeviceInfoByIndex(i) == false)
                    {
                        break;
                    }

                    var path = s.GetDevicePath();
                    if (devicesPaths.Any(devicePath => devicePath.ToLower()
                            .StartsWith(path.ToLower())) ==
                        false)
                    {
                        continue;
                    }

                    var deviceProperties = new DeviceProperties
                    {
                        FriendlyName = s.GetDeviceFriendlyName(),
                        Type = s.GetDeviceType(),
                        Manufacturer = s.GetDeviceManufacturer(),
                        Location = s.GetDeviceLocationInformation(),
                        Description = s.GetDeviceDescription(),
                        Address = s.GetDeviceAddress(),
                        Path = s.GetDeviceLocalPaths(),
                        PhysicalObjectName = s.GetDevicePhysicalDeviceObjectName(),
                        Class = s.GetDeviceClass(),
                        Id = s.GetDeviceId(),
                        BusReportedDeviceDesc = s.GetBusReportedDeviceDescription(),
                        HardwareId = s.GetDeviceHardwareId(),
                        ParentId = s.GetParentId()
                    };

                    allProperties.Add(deviceProperties);
                    var id = s.GetDeviceId();
                    var pid = s.GetParentId();
                    var driver = s.GetDeviceDriver();
                    //var e = _worker.SetupApi.Foo(deviceProperties.ComPort);

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
                }

                s.SetupDiDestroyDeviceInfoList();
            }

            return allProperties;
        }

        internal List<UsbHubProperties> ReadAllHubs()
        {
            _worker.SetupApi.SetGeneralData();
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

            _worker.SetupApi.SetupDiDestroyDeviceInfoList();
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
            //var test1 = _worker.SetupApi.GetDeviceExclusive();
            //var test2 = _worker.SetupApi.GetDeviceEnumeratorName();
            //var test3 = _worker.SetupApi.GetDeviceInstallState();
            //var test5 = _worker.SetupApi.GetDeviceNumberDescriptionFormat();
            //var test6 = _worker.SetupApi.GetDeviceNumberFormat();
            //var test7 = _worker.SetupApi.GetDeviceService();
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
            deviceProperties.Driver = _worker.SetupApi.GetDeviceDriver();
            if (!getComPort)
            {
                return deviceProperties;
            }

            deviceProperties.ComPort = _worker.GetComPort();
            //_worker.SetupApi.Foo(deviceProperties.ComPort);
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
                HardwareId = _worker.SetupApi.GetDeviceHardwareId(),
                ParentId = _worker.SetupApi.GetParentId()
            };

            //_worker.SetupApi.Foo();
            //SetupApi.GetPortNames();
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