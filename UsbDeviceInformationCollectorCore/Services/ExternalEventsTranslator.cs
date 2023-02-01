using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using DynamicData;
using NLog;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using UsbDeviceInformationCollectorCore.CLibs.User32Dll;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Extensions;
using UsbDeviceInformationCollectorCore.Models;
using UsbDeviceInformationCollectorCore.Translators;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class ExternalEventsTranslator
    {
        internal static readonly ExternalEventsTranslator Instance = new();
        private readonly DeviceChangesTranslator _translator = new();
        private readonly DeviceManager _dataPoolManager = DeviceManager.Instance;
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
                    tuple.Status is DeviceStatus.Add or DeviceStatus.Remove && string.IsNullOrEmpty(tuple.DevicePath) == false)
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
                        List<DeviceProperties> pr = new();
                        foreach (var @event in events)
                        {
                            var lastDeviceEvent = @event.Last();
                            switch (lastDeviceEvent.Status)
                            {
                                case DeviceStatus.Add:
                                    if (_dataPoolManager.HasDeviceId(@event.Key))
                                    {
                                        continue;
                                    }

                                    var vidPid = _devicePropertiesAnalyzer.GetVidPid(lastDeviceEvent.DevicePath);
                                    var interfacesId = @event.Select(tuple => tuple.DevicePath).ToList();
                                    var deviceProperties = UsbPortsReader.Instance.ReadDeviceProperties(interfacesId);
                                    pr.Add(deviceProperties);
                                    pr.Add(UsbPortsReader.Instance.ReadDeviceProperties("884b96c3-56ef-11d1-bc8c-00a0c91405dd"));
                                    _logger.Debug($"Serching a device {lastDeviceEvent.DevicePath}");
                                    _dataPoolManager.AddDeviceToList(/*devicePath,*/ vidPid, true);

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