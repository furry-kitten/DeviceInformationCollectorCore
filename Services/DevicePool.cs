using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using NLog;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Extensions;
using UsbDeviceInformationCollectorCore.Models;
using UsbDeviceInformationCollectorCore.Spec;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class DevicePool
    {
        internal static readonly DevicePool Instance = new();

        private readonly DeviceFromHubSpec _deviceFromHubSpec;
        private readonly Dictionary<string, int> _hubsAliases = new();
        private readonly IObservable<(List<Device>, DeviceStatus)> _neededDeviceCollectionEvents;
        private readonly IObservable<(List<Device>, DeviceStatus)> _otherDeviceCollectionEvents;
        private readonly IObservable<(List<UsbHubProperties>, DeviceStatus)> _hubsCollectionEvents;
        private readonly List<Device> _neededDevices = new();
        private readonly List<Device> _otherDevices = new();
        private readonly List<UsbHubProperties> _usbHubs = new();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly NeededDeviceSpec _neededDeviceSpec;
        private readonly OtherDeviceSpec _otherDeviceSpec;
        private readonly ReaderWriterLockSlim _lockerHub = new();
        private readonly ReaderWriterLockSlim _lockerNeeded = new();
        private readonly ReaderWriterLockSlim _lockerOther = new();

        private DevicePool()
        {
            _neededDeviceSpec = new NeededDeviceSpec();
            _otherDeviceSpec = new OtherDeviceSpec();
            _deviceFromHubSpec = new DeviceFromHubSpec();

            _neededDeviceCollectionEvents = Observable.FromEvent<(List<Device>, DeviceStatus)>(
                handler => OnNeededDeviceCollectionChanged += handler,
                handler => OnNeededDeviceCollectionChanged -= handler);

            _otherDeviceCollectionEvents = Observable.FromEvent<(List<Device>, DeviceStatus)>(
                handler => OnOtherDeviceCollectionChanged += handler,
                handler => OnOtherDeviceCollectionChanged -= handler);

            _hubsCollectionEvents = Observable.FromEvent<(List<UsbHubProperties>, DeviceStatus)>(
                handler => OnHubsCollectionChanged += handler,
                handler => OnHubsCollectionChanged -= handler);

            _neededDeviceCollectionEvents.Subscribe();
            _otherDeviceCollectionEvents.Subscribe();
            _hubsCollectionEvents.Subscribe();
        }

        internal Dictionary<string, int> HubsAliases =>
            new(_lockerHub.ExecInReadLock(() => _hubsAliases.ToDictionary(o => o.Key, o => o.Value)
                .OrderByDescending(pair => pair.Key.Length)
                .ThenByDescending(pair => pair.Key)));

        internal HashSet<string> DeviceIds { get; } = new();

        internal List<Device> NeededDevices => _lockerNeeded.ExecInReadLock(() => _neededDevices.ToList());

        internal List<Device> OtherDevices => _lockerOther.ExecInReadLock(() => _otherDevices.ToList());

        internal List<UsbHubProperties> UsbHubs =>
            _lockerHub.ExecInReadLock(() => _usbHubs.OrderByDescending(usbHub => usbHub.Path.Length)
                .ToList());

        internal Device GetDevice(Func<Device, bool> filter) =>
            _lockerNeeded.ExecInReadLock(() => _neededDevices.FirstOrDefault(filter)) ??
            _lockerOther.ExecInReadLock(() => _otherDevices.FirstOrDefault(filter));

        internal void AddDevices(List<Device> devices)
        {
            if (devices == null || devices.Any() == false)
            {
                return;
            }

            var neededDevices = devices.Where(_neededDeviceSpec.IsMatch)
                .ToList();

            AddNeededDevices(neededDevices);

            var otherDevices = devices.Where(_otherDeviceSpec.IsMatch)
                .ToList();

            AddOtherDevices(otherDevices);
            _lockerHub.ExecInWriteLock(() =>
            {
                foreach (var deviceProperties in devices.SelectMany(device => device.Properties))
                {
                    DeviceIds.Add(deviceProperties.HardwareId);
                }
            });
        }

        internal void AddHub(params UsbHubProperties[] hubs)
        {
            if (hubs == null || hubs.Length == 0)
            {
                return;
            }

            hubs = hubs.OrderBy(hub => hub.Path.Length)
                .ThenBy(hub => hub.Path)
                .ToArray();

            var newHubs = new List<UsbHubProperties>();
            foreach (var hub in hubs)
            {
                _lockerHub.ExecInWriteLock(() =>
                {
                    if (_hubsAliases.ContainsKey(hub.Path))
                    {
                        hub.IsRemoved = false;
                        return;
                    }

                    var alias = _hubsAliases.Count == 0 ? 0 : _hubsAliases.Values.Max();
                    _hubsAliases.Add(hub.Path, ++alias);
                    hub.Id = alias.ToString();
                    newHubs.Add(hub);
                    _usbHubs.Add(hub);
                });
            }

            OnHubsCollectionChanged?.Invoke((newHubs, DeviceStatus.Add));
        }

        internal void ClearAll()
        {
            OnOtherDeviceCollectionChanged?.Invoke((_otherDevices, DeviceStatus.Remove));
            OnNeededDeviceCollectionChanged?.Invoke((_neededDevices, DeviceStatus.Remove));
            OnHubsCollectionChanged?.Invoke((_usbHubs, DeviceStatus.Remove));

            _otherDevices.Clear();
            _neededDevices.Clear();
            _usbHubs.Clear();
            _hubsAliases.Clear();
        }

        internal void ClearAllSilent()
        {
            OnOtherDeviceCollectionChanged?.Invoke((_otherDevices, DeviceStatus.SilentRemove));
            OnNeededDeviceCollectionChanged?.Invoke((_neededDevices, DeviceStatus.SilentRemove));
            OnHubsCollectionChanged?.Invoke((_usbHubs, DeviceStatus.SilentRemove));

            _otherDevices.Clear();
            _neededDevices.Clear();
            _usbHubs.Clear();
            _hubsAliases.Clear();
        }

        internal void HubsSubscribe(Action<(List<UsbHubProperties>, DeviceStatus)> hubsChangeAction)
        {
            _hubsCollectionEvents.Where(info => info.Item1.Any())
                .Subscribe(info =>
                {
                    var description = new { info.Item1.Count, Devices = info.Item1, Status = info.Item2 };
                    _logger.Debug($"Sending hub message: {description.ToJson()}");
                    Task.Run(() => hubsChangeAction?.Invoke(info))
                        .ConfigureAwait(false);
                });
        }

        internal void NeededDeviceSubscribe(Action<(List<Device>, DeviceStatus)> neededDeviceChangeAction)
        {
            _neededDeviceCollectionEvents
                .Where(info => info.Item1.Any())
                .Subscribe(info =>
                {
                    var description = new { info.Item1.Count, Devices = info.Item1, Status = info.Item2 };
                    _logger.Debug($"Sending needed device message: {description.ToJson()}");
                    Task.Run(() => neededDeviceChangeAction?.Invoke(info))
                        .ConfigureAwait(false);
                });
        }

        internal void OtherDeviceSubscribe(Action<(List<Device>, DeviceStatus)> otherDeviceChangeAction)
        {
            _otherDeviceCollectionEvents.Where(info => info.Item1.Any())
                .Subscribe(info =>
                {
                    var description = new { info.Item1.Count, Devices = info.Item1, Status = info.Item2 };
                    _logger.Debug($"Sending other device message: {description.ToJson()}");
                    Task.Run(() => otherDeviceChangeAction?.Invoke(info))
                        .ConfigureAwait(false);
                });
        }

        internal void RemoveDevices(params Device[] devices)
        {
            var neededDevices = devices.Where(_neededDeviceSpec.IsMatch)
                .ToList();

            RemoveNeededDevices(neededDevices);

            var otherDevices = devices.Where(_otherDeviceSpec.IsMatch)
                .ToList();

            RemoveOtherDevices(otherDevices);
        }

        internal void RemoveHub(UsbHubProperties hub)
        {
            _lockerHub.ExecInWriteLock(() =>
            {
                RemoveDevicesByHub(hub);
                _usbHubs.Remove(hub);
                _hubsAliases.Remove(hub.Path);
            });

            var hubs = new List<UsbHubProperties> { hub };
            OnHubsCollectionChanged?.Invoke((hubs, DeviceStatus.Remove));
        }

        internal void RemoveMarkedDevices()
        {
            var neededDevicesToRemove = NeededDevices.Where(device =>
                    device.IsRemoved ||
                    device.Properties.Any() == false ||
                    device.Properties.Count(properties => properties.IsRemoved) > 1 ||
                    device.Properties.All(properties => properties.IsRemoved) ||
                    _otherDeviceSpec.IsMatch(device))
                .ToList();

            var otherDevicesToRemove = OtherDevices.Where(device =>
                    device.IsRemoved ||
                    device.Properties.Any() == false ||
                    device.Properties.Count(properties => properties.IsRemoved) > 1 ||
                    device.Properties.All(properties => properties.IsRemoved) ||
                    _neededDeviceSpec.IsMatch(device))
                .ToList();

            RemoveNeededDevices(neededDevicesToRemove);
            RemoveOtherDevices(otherDevicesToRemove);
        }

        internal void RemoveMarkedHubs()
        {
            var hubs = UsbHubs.Where(hub => hub.IsRemoved)
                .ToList();

            hubs.ForEach(RemoveHub);
        }

        internal void RemoveMarkedProperties()
        {
            var neededDevicesToUpdate = RemoveMarkedPropertiesFromNeededDevices();
            UpdateNeededDevices(neededDevicesToUpdate);
            var otherDevicesToUpdate = RemoveMarkedPropertiesFromOtherDevices();
            UpdateOtherDevices(otherDevicesToUpdate);
        }

        internal void UpdateDevice(List<Device> devices)
        {
            var neededDevices = devices.Where(_neededDeviceSpec.IsMatch)
                .ToList();

            UpdateNeededDevices(neededDevices);

            var otherDevices = devices.Where(_otherDeviceSpec.IsMatch)
                .ToList();

            UpdateOtherDevices(otherDevices);
        }

        private event Action<(List<Device>, DeviceStatus)> OnNeededDeviceCollectionChanged;
        private event Action<(List<Device>, DeviceStatus)> OnOtherDeviceCollectionChanged;
        private event Action<(List<UsbHubProperties>, DeviceStatus)> OnHubsCollectionChanged;

        private List<Device> RemoveMarkedPropertiesFromNeededDevices()
        {
            var devicesToUpdate = NeededDevices
                .Where(device => device.Properties.Any(properties => properties.IsRemoved))
                .ToList();

            foreach (var device in devicesToUpdate)
            {
                var itemsToRemove = device.Properties.Where(properties => properties.IsRemoved)
                    .ToList();

                var deviceProperties = device.Properties.ToList();
                deviceProperties.Remove(itemsToRemove);
                device.Properties = deviceProperties.ToArray();
            }

            return devicesToUpdate.ToList();
        }

        private List<Device> RemoveMarkedPropertiesFromOtherDevices()
        {
            var devicesToUpdate = OtherDevices
                .Where(device => device.Properties.Any(properties => properties.IsRemoved))
                .ToList();

            foreach (var device in devicesToUpdate)
            {
                var itemsToRemove = device.Properties.Where(properties => properties.IsRemoved)
                    .ToList();

                var deviceProperties = device.Properties.ToList();
                deviceProperties.Remove(itemsToRemove);
                device.Properties = deviceProperties.ToArray();
            }

            return devicesToUpdate.ToList();
        }

        private void AddNeededDevices(List<Device> neededDevices)
        {
            foreach (var device in neededDevices)
            {
                _lockerNeeded.ExecInWriteLock(() =>
                {
                    if (_neededDevices.FirstOrDefault(existDevice =>
                            device.ShortPath == existDevice.ShortPath && device.Id == existDevice.Id) ==
                        null)
                    {
                        var existDevice = _neededDevices.Where(existDevice =>
                                (device.ShortPath != existDevice.ShortPath && device.Id == existDevice.Id) ||
                                (device.ShortPath == existDevice.ShortPath && device.Id != existDevice.Id))
                            .ToList();

                        if (existDevice.Any())
                        {
                            existDevice.ForEach(extraDevice => extraDevice.IsRemoved = true);
                        }

                        _neededDevices.Add(device);
                    }
                    else
                    {
                        neededDevices.Remove(device);
                    }
                });
            }

            OnNeededDeviceCollectionChanged?.Invoke((neededDevices, DeviceStatus.Add));
        }

        private void AddOtherDevices(List<Device> otherDevices)
        {
            foreach (var device in otherDevices)
            {
                _lockerOther.ExecInWriteLock(() =>
                {
                    if (_otherDevices.FirstOrDefault(existDevice =>
                            device.ShortPath == existDevice.ShortPath && device.Id == existDevice.Id) ==
                        null)
                    {
                        var existDevice = _otherDevices.Where(existDevice =>
                                string.IsNullOrEmpty(existDevice.Id) == false &&
                                ((device.ShortPath != existDevice.ShortPath && device.Id == existDevice.Id) ||
                                 (device.ShortPath == existDevice.ShortPath && device.Id != existDevice.Id)))
                            .ToList();

                        if (existDevice.Any())
                        {
                            existDevice.ForEach(extraDevice => extraDevice.IsRemoved = true);
                        }

                        _otherDevices.Add(device);
                    }
                    else
                    {
                        otherDevices.Remove(device);
                    }
                });
            }

            OnOtherDeviceCollectionChanged?.Invoke((otherDevices, DeviceStatus.Add));
        }

        private void RemoveDevicesByHub(UsbHubProperties hub)
        {
            var isDeviceFromHub = _deviceFromHubSpec.IsDeviceFromHub(hub);

            var neededDevices = _neededDevices.Where(isDeviceFromHub)
                .ToList();

            if (neededDevices.Count > 0)
            {
                RemoveNeededDevices(neededDevices);
            }

            var otherDevices = _otherDevices.Where(isDeviceFromHub)
                .ToList();

            if (otherDevices.Count > 0)
            {
                RemoveOtherDevices(otherDevices);
            }
        }

        private void RemoveNeededDevices(List<Device> neededDevices)
        {
            if (neededDevices.Count == 0)
            {
                return;
            }

            var shortPaths = new HashSet<string>(neededDevices.Select(o => o.ShortPath));
            _lockerNeeded.ExecInWriteLock(() =>
            {
                var devicesToRemove = _neededDevices.FindAll(device => shortPaths.Contains(device.ShortPath));
                _neededDevices.Remove(devicesToRemove);
            });

            OnNeededDeviceCollectionChanged?.Invoke((neededDevices, DeviceStatus.Remove));
        }

        private void RemoveOtherDevices(List<Device> otherDevices)
        {
            if (otherDevices.Count == 0)
            {
                return;
            }

            var shortPaths = new HashSet<string>(otherDevices.Select(o => o.ShortPath));
            _lockerOther.ExecInWriteLock(() =>
            {
                var devicesToRemove = _otherDevices.FindAll(device => shortPaths.Contains(device.ShortPath));
                _otherDevices.Remove(devicesToRemove);
            });

            OnOtherDeviceCollectionChanged?.Invoke((otherDevices, DeviceStatus.Remove));
        }

        private void UpdateNeededDevices(List<Device> neededDevices)
        {
            if (neededDevices.Count == 0)
            {
                return;
            }

            var shortPaths = new HashSet<string>(neededDevices.Select(o => o.ShortPath));
            _lockerNeeded.ExecInWriteLock(() =>
            {
                var devicesToRemove = _neededDevices.FindAll(device => shortPaths.Contains(device.ShortPath));
                _neededDevices.Remove(devicesToRemove);
                _neededDevices.Add(neededDevices);
            });

            OnNeededDeviceCollectionChanged?.Invoke((neededDevices, DeviceStatus.Update));
        }

        private void UpdateOtherDevices(List<Device> otherDevices)
        {
            if (otherDevices.Count == 0)
            {
                return;
            }

            var shortPaths = new HashSet<string>(otherDevices.Select(o => o.ShortPath));
            _lockerOther.ExecInWriteLock(() =>
            {
                var devicesToRemove = _otherDevices.FindAll(device => shortPaths.Contains(device.ShortPath));
                _otherDevices.Remove(devicesToRemove);
                _otherDevices.Add(otherDevices);
            });

            OnOtherDeviceCollectionChanged?.Invoke((otherDevices, DeviceStatus.Update));
        }
    }
}