using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using DynamicData;
using NLog;
using UsbDeviceInformationCollectorCore.Converters;
using UsbDeviceInformationCollectorCore.Extensions;
using UsbDeviceInformationCollectorCore.Models;
using UsbDeviceInformationCollectorCore.Utils;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class DeviceManager
    {
        private const string DeviceUsbPathPattern = @"#USBROOT\(\d+\)(#USB\(\d+\))+";
        internal static DeviceManager Instance = new();
        private readonly DelayEraser _eraser;
        private readonly DeviceConverter _deviceConverter = DeviceConverter.Instance;
        private readonly DevicePool _devicePool = DevicePool.Instance;
        private readonly DevicePropertiesAnalyzer _analyzer = DevicePropertiesAnalyzer.Instance;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ReaderWriterLockSlim _fullResetLocker = new();
        private readonly UsbPortsReader _usbPortReader = UsbPortsReader.Instance;

        private DeviceManager()
        {
            _eraser = new DelayEraser(this);
        }

        internal bool IsFullResetInProgress => _fullResetLocker.IsWriteLockHeld;

        public bool HasDeviceId(string eventKey) => _devicePool.DeviceIds.Contains(_analyzer.ExtractDeviceId(eventKey));

        internal bool? RemoveDeviceUnSafe(string hardwareId)
        {
            if (string.IsNullOrWhiteSpace(hardwareId))
            {
                return null;
            }

            var existDeviceInfo = _devicePool.GetDevice(device =>
                device.Properties.Any(properties => _analyzer.IsHardwareIdEquals(properties.Id, hardwareId)));

            if (existDeviceInfo == null)
            {
                _logger.Debug($"Removing hub: {hardwareId}");
                RemoveHub(hardwareId);
                return false;
            }

            var existDeviceProperties = existDeviceInfo.Properties.FirstOrDefault(properties =>
                _analyzer.IsHardwareIdEquals(properties.Id, hardwareId));

            if (existDeviceProperties == null || existDeviceProperties.IsRemoved)
            {
                return true;
            }

            _logger.Debug($"Removing device: {existDeviceInfo.ToJson()}");
            existDeviceProperties.IsRemoved = true;
            existDeviceInfo.IsRemoved = existDeviceInfo.Properties.Count(properties => properties.IsRemoved) > 1 ||
                                        existDeviceInfo.Properties.Length == 1;

            _eraser.OnDevicePoolErase();
            return true;
        }

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
                var existDeviceInfo = _devicePool
                    .GetDevice(existDevice => existDevice.Properties.Any(existDeviceProperties =>
                        IsPathsEquals(existDeviceProperties, device.Properties[0])));

                if (existDeviceInfo != null)
                {
                    if (device.Properties.Length ==
                        existDeviceInfo.Properties.Count(properties => properties.IsRemoved == false) &&
                        existDeviceInfo.IsRemoved == false)
                    {
                        continue;
                    }

                    var result = AddPropertiesToDevice(device.Properties.ToList());
                    if (result is null)
                    {
                        _logger.Fatal("Device was removed from pool");
                        continue;
                    }

                    updatedDevices.Add(device);
                    _logger.Debug($"Update device: {device.ToJson()}");
                    continue;
                }

                newDevices.Add(device);
                _logger.Debug($"Add new device: {device.ToJson()}");
            }

            _devicePool.AddDevices(newDevices);
            _devicePool.UpdateDevice(updatedDevices);
        }


        /// <summary>
        ///     Enumerate all USB devices and look for the device whose VID and PID are provided.
        /// </summary>
        /// <returns>True the Device is found.</returns>
        internal void AddDeviceToList(/*string devicePath,*/ string vidPid, bool getComPort = false)
        {
            if (string.IsNullOrEmpty(vidPid))
            {
                return;
            }

            if (!_fullResetLocker.TryEnterWriteLock(0))
            {
                return;
            }

            try
            {
                _logger.Debug($"Serching a device {vidPid}");
                var deviceProperties = _usbPortReader.ReadDeviceProperties(vidPid, getComPort);

                _logger.Debug($"Found: {deviceProperties.ToJson()}");
                if (deviceProperties == null)
                {
                    _logger.Debug($"Hub found with {vidPid}. Start getting all devices.");
                    SetHubs();
                    UpdateDevices();
                    return;
                }

                var devices = _deviceConverter.ConvertDevices(/*devicePath,*/ deviceProperties, _devicePool.HubsAliases);
                _logger.Debug($"Adding devices: {devices.ToJson()}");
                AddDevices(devices);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            finally
            {
                _fullResetLocker.ExitWriteLock();
            }
        }
        
        /// <summary>
        ///     Enumerate all USB devices and look for the device whose VID and PID are provided.
        /// </summary>
        /// <returns>True the Device is found.</returns>
        internal void AddDeviceToList(List<string> interfacesIds, string vidPid)
        {
            if (string.IsNullOrEmpty(vidPid))
            {
                return;
            }

            if (!_fullResetLocker.TryEnterWriteLock(0))
            {
                return;
            }

            try
            {
                _logger.Debug($"Serching a device {vidPid}");
                var deviceProperties = _usbPortReader.ReadDeviceProperties(vidPid, true);
                //deviceProperties = _usbPortReader.ReadDeviceProperties(interfacesIds);

                _logger.Debug($"Found: {deviceProperties.ToJson()}");
                if (deviceProperties == null)
                {
                    _fullResetLocker.EnterWriteLock();
                    _logger.Debug($"Hub found with {vidPid}. Start getting all devices.");
                    SetHubs();
                    UpdateDevices();
                    _fullResetLocker.ExitWriteLock();
                    return;
                }

                var devices = _deviceConverter.ConvertDevices(/*devicePath,*/ deviceProperties, _devicePool.HubsAliases);
                _logger.Debug($"Adding devices: {devices.ToJson()}");
                AddDevices(devices);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            finally
            {
                _fullResetLocker.ExitWriteLock();
            }
        }

        internal void AddHub(params UsbHubProperties[] hubs)
        {
            _devicePool.AddHub(hubs);
        }

        internal void Clear()
        {
            _devicePool.ClearAll();
        }

        internal void EraseDevicePool()
        {
            _devicePool.RemoveMarkedHubs();
            _devicePool.RemoveMarkedProperties();
            _devicePool.RemoveMarkedDevices();
        }

        internal void FullResetUsbDevices()
        {
            _logger.Info("FullResetUsbDevices started");

            _fullResetLocker.EnterWriteLock();
            try
            {
                Clear();
                SetHubs();
                SetDevices();
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            finally
            {
                _fullResetLocker.ExitWriteLock();
            }
        }

        internal void RemoveDevice(string hardwareId)
        {
            if (string.IsNullOrEmpty(hardwareId))
            {
                return;
            }

            //skip call when full reset in progress
            if (!_fullResetLocker.TryEnterWriteLock(0))
            {
                return;
            }

            try
            {
                _logger.Debug($"Removing device with hardware id {hardwareId}");
                RemoveDeviceUnSafe(hardwareId);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            finally
            {
                _fullResetLocker.ExitWriteLock();
            }
        }

        internal void UpdateDevices()
        {
            var deviceProperties = _devicePool.NeededDevices.Select(device => device.Properties)
                .ToList();

            deviceProperties.Add(_devicePool.OtherDevices.Select(device => device.Properties)
                .ToList());

            var allProperties = deviceProperties.SelectMany(props => props)
                .ToList();

            var allDevices = _deviceConverter.ConvertDevices(allProperties, _devicePool.HubsAliases);
            var devicesId = allProperties
                .Where(properties => string.IsNullOrEmpty(_analyzer.ExtractDeviceId(properties.Id)) == false)
                .ToList();

            var neededDevices = devicesId
                .Select(properties =>
                    _devicePool.GetDevice(device => device.Id == _analyzer.ExtractDeviceId(properties.Id)))
                .Where(device => device != null)
                .ToList();

            neededDevices.ForEach(device => device.Properties = new[]
            {
                allProperties.First(properties => _analyzer.ExtractDeviceId(properties.Id) == device.Id)
            });

            foreach (var device in neededDevices)
            {
                device.Id = null;
                device.ComPort = null;
                device.Manufacture = null;
                device.ModelName = null;
                device.ModelNumber = null;
                device.ShortPath = null;
                _deviceConverter.SetDevice(device, device.Properties);
            }

            var devices = allDevices.ToList();
            _devicePool.RemoveDevices(_devicePool.OtherDevices.ToArray());
            AddDevices(devices);
        }

        private static bool IsDeviceUsbPath(DeviceProperties device) =>
            Regex.IsMatch(device.Path, DeviceUsbPathPattern);

        private bool IsPathsEquals(DeviceProperties existDeviceProperties, DeviceProperties newDeviceProperties)
        {
            var isShortPathEquals = existDeviceProperties.ShortPath == newDeviceProperties.ShortPath ||
                                    existDeviceProperties.Path == newDeviceProperties.Path;

            if (isShortPathEquals)
            {
                return true;
            }

            existDeviceProperties.ShortPath =
                _analyzer.SimplifyDevicePath(existDeviceProperties.Path, _devicePool.HubsAliases);

            newDeviceProperties.ShortPath =
                _analyzer.SimplifyDevicePath(newDeviceProperties.Path, _devicePool.HubsAliases);

            isShortPathEquals = existDeviceProperties.ShortPath == newDeviceProperties.ShortPath;
            if (isShortPathEquals)
            {
                return true;
            }

            if (newDeviceProperties.Path == newDeviceProperties.ShortPath)
            {
                return existDeviceProperties.Path.Contains(newDeviceProperties.Path) ||
                       newDeviceProperties.Path.Contains(existDeviceProperties.Path);
            }

            return false;
        }

        private bool? AddPropertiesToDevice(List<DeviceProperties> propertiesArray)
        {
            if (propertiesArray == null ||
                propertiesArray.All(properties => string.IsNullOrEmpty(properties.ShortPath)))
            {
                return null;
            }

            var shortPath = propertiesArray.First(x => !string.IsNullOrEmpty(x.ShortPath))
                .ShortPath;

            var device = _devicePool.GetDevice(d => d.ShortPath == shortPath);
            if (device is null)
            {
                return null;
            }

            bool? result = false;
            foreach (var deviceProperties in propertiesArray)
            {
                result |= AddPropertiesToDevice(device, deviceProperties);
            }

            return result;
        }

        private bool? AddPropertiesToDevice(Device device, DeviceProperties properties)
        {
            if (IsDeviceUsbPath(properties) == false || string.IsNullOrEmpty(properties.ShortPath))
            {
                return null;
            }

            var existProperties = device.Properties.FirstOrDefault(existDeviceProperties =>
                existDeviceProperties.PnpClassesTypes == properties.PnpClassesTypes &&
                existDeviceProperties.ShortPath == properties.ShortPath);

            if (existProperties != null)
            {
                if (existProperties.IsRemoved == false)
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
                    existProperties.Driver ??= properties.Driver;
                }
                else
                {
                    device.IsRemoved = false;
                    existProperties.IsRemoved = false;
                    existProperties.Path = properties.Path;
                    existProperties.Address = properties.Address;
                    existProperties.ComPort = properties.ComPort;
                    existProperties.Class = properties.Class;
                    existProperties.Description = properties.Description;
                    existProperties.Id = properties.Id;
                    existProperties.Location = properties.Location;
                    existProperties.Manufacturer = properties.Manufacturer;
                    existProperties.PhysicalObjectName = properties.PhysicalObjectName;
                    existProperties.Type = properties.Type;
                    existProperties.FriendlyName = properties.FriendlyName;
                    existProperties.Driver = properties.Driver;
                }

                _logger.Debug(
                    $"Adding/updating propeties ({properties.ToJson()}) to device {device.Properties[0].Path} ({device.ShortPath})");

                return false;
            }

            _logger.Debug(
                $"Adding/updating propeties ({properties.ToJson()}) to device {device.Properties[0].Path} ({device.ShortPath})");

            var propertiesList = device.Properties.ToList();
            propertiesList.Add(properties);
            device.Properties = propertiesList.ToArray();
            return true;
        }

        private void RemoveHub(string hubHardwareId)
        {
            var changedHardwareId = _analyzer.GetGeneralHardwareId(hubHardwareId);
            var hub = _devicePool.UsbHubs.FirstOrDefault(hubProperties =>
                DevicePropertiesAnalyzer.Instance.IsHardwareIdEquals(hubProperties.HardwareId, changedHardwareId));

            if (hub is null)
            {
                return;
            }

            hub.IsRemoved = true;
            _eraser.OnDevicePoolErase();
        }

        private void SetDevices()
        {
            _logger.Info(
                $"(after adding hubs)DevicePool in FullSetUsbDevice:{Environment.NewLine}\tUsbHubs: {DevicePool.Instance.UsbHubs.ToJson()}{Environment.NewLine}\tHubsAliases: {DevicePool.Instance.HubsAliases.ToJson()}{Environment.NewLine}\tNeededDevices: {DevicePool.Instance.NeededDevices.ToJson()}{Environment.NewLine}\tOtherDevices: {DevicePool.Instance.OtherDevices.ToJson()}");

            var allProperties = _usbPortReader.GetPropertiesForAllDevices();
            var devices = _deviceConverter.ConvertDevices(allProperties, _devicePool.HubsAliases);
            AddDevices(devices);

            _logger.Info($"All devices properties:{Environment.NewLine}\t{devices.ToJson()}");
            _logger.Debug(
                $"DevicePool in FullSetUsbDevice:{Environment.NewLine}\tUsbHubs: {DevicePool.Instance.UsbHubs.ToJson()}{Environment.NewLine}\tHubsAliases: {DevicePool.Instance.HubsAliases.ToJson()}{Environment.NewLine}\tNeededDevices: {DevicePool.Instance.NeededDevices.ToJson()}{Environment.NewLine}\tOtherDevices: {DevicePool.Instance.OtherDevices.ToJson()}");
        }

        private void SetHubs()
        {
            var hubsProperties = _usbPortReader.ReadAllHubs()
                .OrderBy(hub => hub.Path)
                .ToArray();

            AddHub(hubsProperties);
        }
    }
}