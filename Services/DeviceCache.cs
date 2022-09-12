using DynamicData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;
using UsbDeviceInformationCollectorCore.Utils;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class DeviceCache
    {
        private readonly IObservable<(List<Device>, DeviceStatus)> _neededDeviceCollectionEvents;
        private readonly IObservable<(List<Device>, DeviceStatus)> _otherDeviceCollectionEvents;
        private readonly IObservable<(List<UsbHubProperties>, DeviceStatus)> _hubsCollectionEvents;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private DeviceCache()
        {
            _neededDeviceCollectionEvents = Observable.FromEvent<(List<Device>, DeviceStatus)>(
                handler => OnNeededDeviceCollectionChanged += handler,
                handler => OnNeededDeviceCollectionChanged -= handler);
            _otherDeviceCollectionEvents = Observable.FromEvent<(List<Device>, DeviceStatus)>(
                handler => OnOtherDeviceCollectionChanged += handler,
                handler => OnOtherDeviceCollectionChanged -= handler);
            _hubsCollectionEvents = Observable.FromEvent<(List<UsbHubProperties>, DeviceStatus)>(
                handler => OnHubsCollectionChanged += handler,
                handler => OnHubsCollectionChanged -= handler);
        }

        internal static DeviceCache Instance { get; } = new ();
        internal Dictionary<string, int> HubsAliases { get; } = new ();
        internal List<Device> NeededDevices { get; } = new ();
        internal List<Device> OtherDevices { get; } = new ();
        internal List<UsbHubProperties> UsbHubs { get; private set; } = new ();

        internal void AddHub(List<UsbHubProperties> hubs)
        {
            List<UsbHubProperties> newHubs = new ();
            foreach (var hub in hubs)
            {
                if (HubsAliases.ContainsKey(hub.Path))
                {
                    continue;
                }

                var alias = HubsAliases.Count == 0 ? 0 : HubsAliases.Values.Max();
                HubsAliases.Add(hub.Path, ++alias);
                hub.Id = alias.ToString();
                newHubs.Add(hub);
            }

            UsbHubs.AddRange(newHubs);
            UsbHubs = UsbHubs.OrderByDescending(usbHub => usbHub.Path.Length).ToList();
            OnHubsCollectionChanged?.Invoke((newHubs, DeviceStatus.Add));
        }

        internal void AddHub(UsbHubProperties hub)
        {
            if (HubsAliases.ContainsKey(hub.Path))
            {
                return;
            }

            var alias = HubsAliases.Count == 0 ? 0 : HubsAliases.Values.Max();
            HubsAliases.Add(hub.Path, ++alias);
            hub.Id = alias.ToString();
            UsbHubs.Add(hub);
            UsbHubs = UsbHubs.OrderByDescending(usbHub => usbHub.Path.Length).ToList();
            var hubs = new List<UsbHubProperties>
            {
                hub
            };
            OnHubsCollectionChanged?.Invoke((hubs, DeviceStatus.Add));
        }

        internal void NeededDeviceSubscribe(Action<(List<Device>, DeviceStatus)> neededDeviceChangeAction) =>
            _neededDeviceCollectionEvents.Where(info => info.Item1.Any())
                .Subscribe(info =>
                {
                    var description = new
                    {
                        info.Item1.Count,
                        Devices = info.Item1,
                        Status = info.Item2
                    };
                    _logger.Debug($"Sending needed device message: {description.ToJson()}");
                    Task.Run(() => neededDeviceChangeAction?.Invoke(info)).ConfigureAwait(false);
                });

        internal void OtherDeviceSubscribe(Action<(List<Device>, DeviceStatus)> otherDeviceChangeAction) =>
            _otherDeviceCollectionEvents.Where(info => info.Item1.Any())
                .Subscribe(info =>
                {
                    var description = new
                    {
                        info.Item1.Count,
                        Devices = info.Item1,
                        Status = info.Item2
                    };
                    _logger.Debug($"Sending other device message: {description.ToJson()}");
                    Task.Run(() => otherDeviceChangeAction?.Invoke(info)).ConfigureAwait(false);
                });

        internal void HubsSubscribe(Action<(List<UsbHubProperties>, DeviceStatus)> hubsChangeAction) =>
            _hubsCollectionEvents.Where(info => info.Item1.Any())
                .Subscribe(info =>
                {
                    var description = new
                    {
                        info.Item1.Count,
                        Devices = info.Item1,
                        Status = info.Item2
                    };
                    _logger.Debug($"Sending hub message: {description.ToJson()}");
                    Task.Run(() => hubsChangeAction?.Invoke(info)).ConfigureAwait(false);
                });

        internal void AddDevices(List<Device> devices)
        {
            if (devices == null || devices.Any() == false)
            {
                return;
            }

            var neededDevices = devices.Where(IsNeededDevice).ToList();
            var newNeededDevices = neededDevices.Except(NeededDevices).ToList();
            AddNeededDevices(newNeededDevices);
            var otherDevices = devices.Where(IsOtherDevice).ToList();
            var newOtherDevices = otherDevices.Except(OtherDevices).ToList();
            AddOtherDevices(newOtherDevices);
        }

        internal Device GetDevice(Func<Device, bool> filter) => NeededDevices.FirstOrDefault(filter) ?? OtherDevices.FirstOrDefault(filter);

        internal void RemoveHub(UsbHubProperties hub)
        {
            RemoveDevicesByHub(hub);
            UsbHubs.Remove(hub);
            HubsAliases.Remove(hub.Path);
            var hubs = new List<UsbHubProperties>
            {
                hub
            };
            OnHubsCollectionChanged?.Invoke((hubs, DeviceStatus.Remove));
        }

        internal void RemoveDevices(List<Device> devices)
        {
            var neededDevices = devices.Where(IsNeededDevice).ToList();
            RemoveNeededDevices(neededDevices);
            var otherDevices = devices.Where(IsOtherDevice).ToList();
            RemoveOtherDevices(otherDevices);
        }

        internal void UpdateDevice(List<Device> devices)
        {
            var neededDevices = devices.Where(IsNeededDevice).ToList();
            UpdateNeededDevices(neededDevices);
            var otherDevices = devices.Where(IsOtherDevice).ToList();
            UpdateOtherDevices(otherDevices);
        }

        private void RemoveDevicesByHub(UsbHubProperties hub)
        {
            var shortPathPattern = $@"UsbHub\({hub.Id}\)#USB\(\d+\)";
            var neededDevices = NeededDevices.Where(device => Regex.IsMatch(device.ShortPath, shortPathPattern, RegexOptions.IgnoreCase)).ToList();
            if (neededDevices.Count > 0)
            {
                RemoveNeededDevices(neededDevices);
            }

            var otherDevices = OtherDevices.Where(device => Regex.IsMatch(device.ShortPath, shortPathPattern, RegexOptions.IgnoreCase)).ToList();
            if (otherDevices.Count > 0)
            {
                RemoveOtherDevices(otherDevices);
            }
        }

        private void RemoveOtherDevices(List<Device> otherDevices)
        {
            OtherDevices.Remove(OtherDevices.FindAll(device => otherDevices.FirstOrDefault(d => d.ShortPath == device.ShortPath) != null));
            OnOtherDeviceCollectionChanged?.Invoke((otherDevices, DeviceStatus.Remove));
        }

        private void RemoveNeededDevices(List<Device> neededDevices)
        {
            NeededDevices.Remove(NeededDevices.FindAll(device => neededDevices.FirstOrDefault(d => d.ShortPath == device.ShortPath) != null));
            OnNeededDeviceCollectionChanged?.Invoke((neededDevices, DeviceStatus.Remove));
        }

        private void AddOtherDevices(List<Device> otherDevices)
        {
            OtherDevices.Add(otherDevices);
            OnOtherDeviceCollectionChanged?.Invoke((otherDevices, DeviceStatus.Add));
        }

        private void AddNeededDevices(List<Device> neededDevices)
        {
            NeededDevices.Add(neededDevices);
            OnNeededDeviceCollectionChanged?.Invoke((neededDevices, DeviceStatus.Add));
        }

        private void UpdateOtherDevices(List<Device> otherDevices)
        {
            OtherDevices.Remove(OtherDevices.FindAll(device => otherDevices.FirstOrDefault(d => d.ShortPath == device.ShortPath) != null));
            OtherDevices.Add(otherDevices);
            OnOtherDeviceCollectionChanged?.Invoke((otherDevices, DeviceStatus.Update));
        }

        private void UpdateNeededDevices(List<Device> neededDevices)
        {
            NeededDevices.Remove(NeededDevices.FindAll(device => neededDevices.FirstOrDefault(d => d.ShortPath == device.ShortPath) != null));
            NeededDevices.Add(neededDevices);
            OnNeededDeviceCollectionChanged?.Invoke((neededDevices, DeviceStatus.Update));
        }

        private event Action<(List<Device>, DeviceStatus)> OnNeededDeviceCollectionChanged;
        private event Action<(List<Device>, DeviceStatus)> OnOtherDeviceCollectionChanged;
        private event Action<(List<UsbHubProperties>, DeviceStatus)> OnHubsCollectionChanged;

        private bool IsNeededDevice(Device device) =>
            string.IsNullOrEmpty(device.Id) == false &&
            device.Properties.Any(property => property.PnpClassesTypes is PnPDeviceClassType.MEDIA or PnPDeviceClassType.HIDClass or PnPDeviceClassType.None) == false;

        private bool IsOtherDevice(Device device) =>
            string.IsNullOrEmpty(device.Id) ||
            device.Properties.Any(property => property.PnpClassesTypes is PnPDeviceClassType.MEDIA or PnPDeviceClassType.HIDClass or PnPDeviceClassType.None);

        internal void ClearAll()
        {
            OnOtherDeviceCollectionChanged?.Invoke((OtherDevices, DeviceStatus.Remove));
            OnNeededDeviceCollectionChanged?.Invoke((NeededDevices, DeviceStatus.Remove));
            OnHubsCollectionChanged?.Invoke((UsbHubs, DeviceStatus.Remove));
            OtherDevices.Clear();
            NeededDevices.Clear();
            UsbHubs.Clear();
            HubsAliases.Clear();
        }

        internal void ClearAllSilent()
        {
            OnOtherDeviceCollectionChanged?.Invoke((OtherDevices, DeviceStatus.SilentRemove));
            OnNeededDeviceCollectionChanged?.Invoke((NeededDevices, DeviceStatus.SilentRemove));
            OnHubsCollectionChanged?.Invoke((UsbHubs, DeviceStatus.SilentRemove));
            OtherDevices.Clear();
            NeededDevices.Clear();
            UsbHubs.Clear();
            HubsAliases.Clear();
        }
    }
}