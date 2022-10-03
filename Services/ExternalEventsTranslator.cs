using System;
using System.Linq;
using System.Reactive.Linq;
using NLog;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using UsbDeviceInformationCollectorCore.CLibs.User32Dll;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Extensions;
using UsbDeviceInformationCollectorCore.Translators;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class ExternalEventsTranslator
    {
        internal static readonly ExternalEventsTranslator Instance = new();
        private readonly DeviceChangesTranslator _translator = new();
        private readonly DeviceManager _dataPoolManager = DeviceManager.Instance;
        private readonly DevicePool _devicePool = DevicePool.Instance;
        private readonly DevicePropertiesAnalyzer _devicePropertiesAnalyzer = DevicePropertiesAnalyzer.Instance;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly User32Dll _user32 = new();

        private ExternalEventsTranslator() { }

        internal void ConfigEventHandlers(IntPtr externalEventHandle, Action<WndProcDelegate> act)
        {
            act.Invoke(WndProc);
            _user32.RegisterForDeviceChange(externalEventHandle);
        }

        internal void InitEvents()
        {
            Observable.FromEvent<(string DevicePath, DeviceStatus Status)>(
                    handler => OnDeviceChanged += handler,
                    handler => OnDeviceChanged -= handler)
                .Where(tuple =>
                    tuple.Status is DeviceStatus.Add or DeviceStatus.Remove)
                .Buffer(TimeSpan.FromSeconds(1))
                .Select(list => list
                    .GroupBy(tuple => _devicePropertiesAnalyzer.GetHardwareIdFromDevicePath(tuple.DevicePath))
                    .ToList())
                .Where(list => list.Any(tuples => string.IsNullOrEmpty(tuples.Key) == false))
                .Subscribe(events =>
                {
                    _logger.Debug($"Devices arrives: {events.ToJson()}");
                    try
                    {
                        foreach (var @event in events)
                        {
                            var lastDeviceEvent = @event.Last();
                            switch (lastDeviceEvent.Status)
                            {
                                case DeviceStatus.Add:
                                    if (_devicePool.DeviceIds.Contains(
                                            _devicePropertiesAnalyzer.ExtractDeviceId(@event.Key)))
                                    {
                                        continue;
                                    }

                                    _logger.Debug($"Serching a device {lastDeviceEvent.DevicePath}");
                                    _dataPoolManager.AddDeviceToList(
                                        _devicePropertiesAnalyzer.GetVidPid(lastDeviceEvent.DevicePath), true);

                                    break;

                                case DeviceStatus.Remove:
                                    _dataPoolManager.RemoveDevice(@event.Key);
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Fatal(e,
                            $"Unhandled exception!!!{Environment.NewLine}Base device info: {events.ToJson()}");
                    }
                });
        }

        private event Action<(string DevicePath, DeviceStatus Status)> OnDeviceChanged;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != (int)WindowsMessage.DeviceChange)
            {
                return (IntPtr)0;
            }

            var device = (DbtDevice)wParam.ToInt32();
            string devicePath;
            switch (device)
            {
                case DbtDevice.DeviceArrival:
                    devicePath = _translator.GetDevicePath(lParam);
                    OnDeviceChanged?.Invoke((devicePath, DeviceStatus.Add));
                    break;

                case DbtDevice.DeviceRemoveComplete:
                    devicePath = _translator.GetDevicePath(lParam);
                    OnDeviceChanged?.Invoke((devicePath, DeviceStatus.Remove));
                    break;
            }

            return (IntPtr)0;
        }
    }
}