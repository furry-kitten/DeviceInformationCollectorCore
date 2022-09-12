using NLog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using UsbDeviceInformationCollectorCore.CLibs.User32;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;
using UsbDeviceInformationCollectorCore.Translators;

namespace UsbDeviceInformationCollectorCore.Services
{
    public class Class1
    {
        private bool _isWindowConfigured;
        private User32 _user32;

        public Class1()
        {
            LogWorker.ConfigNLog();
            InitEvents();
            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("Logger configured successfully, start observing. App version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            logger.Info($"{nameof(Class1)} is initializing");
        }

        private void InitEvents()
        {
            Observable.FromEvent<DeviceChangesTranslator>(
                    handler => OnAddDevice += handler,
                    handler => OnAddDevice -= handler)
                .WhereNotNull()
                .Where(translator =>
                    DeviceCache.Instance.UsbHubs.Any(hub => string.Equals(hub.HardwareId, translator?.HardwareId?.Replace("#", "\\"), StringComparison.InvariantCultureIgnoreCase) == false))
                .Buffer(TimeSpan.FromSeconds(1))
                .DoWhile(() => DeviceChangesTranslator.IsFullSet == false)
                .Select(list => list.GroupBy(translator => translator.VidPid)
                    .ToList())
                .Where(list => list.Any())
                .Subscribe(list =>
                {
                    foreach (var grouping in list)
                    {
                        foreach (var translator in grouping)
                        {
                            if (DeviceCache.Instance.UsbHubs.Any(hub => string.Equals(hub?.HardwareId, translator?.HardwareId?.Replace("#", "\\"), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                break;
                            }

                            translator.AddDeviceToList(true);
                            break;
                        }
                    }
                });
            Observable.FromEvent<DeviceChangesTranslator>(
                    handler => OnRemoveDevice += handler,
                    handler => OnRemoveDevice -= handler)
                .Select(translator => (
                    DeviceCache.Instance.GetDevice(device =>
                        device.Properties.Any(properties => string.Equals(properties.Id, translator?.HardwareId?.Replace("#", "\\"), StringComparison.InvariantCultureIgnoreCase))), translator))
                .Buffer(TimeSpan.FromSeconds(2))
                .DoWhile(() => DeviceChangesTranslator.IsFullSet == false)
                .Select(list => list.GroupBy(device => device.Item1?.Id)
                    .ToList())
                .Where(list => list.Any())
                .Subscribe(list =>
                {
                    foreach (var grouping in list)
                    {
                        foreach (var pair in grouping)
                        {
                            if (pair.Item1 == null)
                            {
                                pair.translator.RemoveDevice();
                                continue;
                            }

                            pair.translator.RemoveDevice();
                            break;
                        }
                    }
                });
        }

        public void Init(Window window, Action<(List<Device> Devices, DeviceStatus Status)> deviceChangeAction)
        {
            ConfigTargetWindow(window);
            SubscribeOnChangingDeviceCollection(deviceChangeAction);
            PollUsbPorts();
        }

        public void Init(Window window, Action<(List<Device> Devices, DeviceStatus Status)> deviceChangeAction, Action<(List<Device> Devices, DeviceStatus Status)> otherDeviceChangeAction)
        {
            ConfigTargetWindow(window);
            SubscribeOnChangingDeviceCollection(deviceChangeAction);
            SubscribeOnChangingOtherDeviceCollection(otherDeviceChangeAction);
            PollUsbPorts();
        }

        public void Init(Window window,
            Action<(List<Device> Devices, DeviceStatus Status)> deviceChangeAction,
            Action<(List<Device> Devices, DeviceStatus Status)> otherDeviceChangeAction,
            Action<(List<UsbHubProperties> Devices, DeviceStatus Status)> hubChangeAction)
        {
            ConfigTargetWindow(window);
            SubscribeOnChangingDeviceCollection(deviceChangeAction);
            SubscribeOnChangingOtherDeviceCollection(otherDeviceChangeAction);
            SubscribeOnChangingHubsCollection(hubChangeAction);
            PollUsbPorts();
        }

        public void ConfigTargetWindow(Window window)
        {
            _isWindowConfigured = true;
            _user32 = new User32(window);
            HookWndProc(window);
            _user32.RegisterForDeviceChange();
        }

        public void SubscribeOnChangingDeviceCollection(Action<(List<Device> Devices, DeviceStatus Status)> deviceChangeAction) => DeviceCache.Instance.NeededDeviceSubscribe(deviceChangeAction);

        public void SubscribeOnChangingOtherDeviceCollection(Action<(List<Device> Devices, DeviceStatus Status)> otherDeviceChangeAction) =>
            DeviceCache.Instance.OtherDeviceSubscribe(otherDeviceChangeAction);

        public void SubscribeOnChangingHubsCollection(Action<(List<UsbHubProperties> Devices, DeviceStatus Status)> hubChangeAction) => DeviceCache.Instance.HubsSubscribe(hubChangeAction);

        public void PollUsbPorts() => DeviceChangesTranslator.FullSetUsbDevice();

        private event Action<DeviceChangesTranslator> OnAddDevice;
        private event Action<DeviceChangesTranslator> OnRemoveDevice;

        private IntPtr WndProc(IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (!_isWindowConfigured)
            {
                throw new Exception(@"Please call ""ConfigTargetWindow"" before use!!");
            }

            switch (msg)
            {
                case (int) WindowsMessage.DeviceChange:
                    OnDeviceChanged(wParam, lParam);
                    break;
            }

            return (IntPtr) 0;
        }

        private void HookWndProc(Window window)
        {
            if (PresentationSource.FromVisual(window) is HwndSource hwndSource)
            {
                hwndSource.AddHook(WndProc);
            }
        }

        private void OnDeviceChanged(IntPtr wParam, IntPtr lParam)
        {
            var device = (DbtDevice) wParam.ToInt32();
            DeviceChangesTranslator translator;
            switch (device)
            {
                case DbtDevice.DeviceArrival:
                    translator = new DeviceChangesTranslator(lParam);
                    OnAddDevice?.Invoke(translator);
                    break;
                case DbtDevice.DeviceRemoveComplete:
                    translator = new DeviceChangesTranslator(lParam);
                    OnRemoveDevice?.Invoke(translator);
                    break;
            }
        }
    }
}