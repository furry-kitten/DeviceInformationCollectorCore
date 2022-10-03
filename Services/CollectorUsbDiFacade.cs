using System;
using System.Collections.Generic;
using System.Reflection;
using NLog;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;
using UsbDeviceInformationCollectorCore.Utils;

namespace UsbDeviceInformationCollectorCore.Services
{
    public class CollectorUsbDiFacade
    {
        private readonly DeviceManager _dataPoolFacade = DeviceManager.Instance;
        private readonly DevicePool _devicePool = DevicePool.Instance;
        private readonly ExternalEventsTranslator _eventsHolder = ExternalEventsTranslator.Instance;

        public CollectorUsbDiFacade()
        {
            LogWorker.ConfigNLog();

            _eventsHolder.InitEvents();

            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("Logger configured successfully, start observing. App version {0}",
                Assembly.GetExecutingAssembly()
                    .GetName()
                    .Version);

            logger.Info("CollectorUsbDiFacade is initializing");
        }

        /// <summary>
        ///     Sample for WPF:
        ///     <code>
        /// private void Button_Click(object sender, RoutedEventArgs e)
        /// {
        ///    var wih = new System.Windows.Interop.WindowInteropHelper(this);
        ///    var hWnd = wih.Handle;
        /// }
        /// </code>
        ///     Another samples (WinForms, C++, etc) at link
        ///     https://docs.microsoft.com/ru-ru/windows/apps/develop/ui-input/retrieve-hwnd
        /// </summary>
        /// <param name="config"></param>
        public void Init(UsbDeviceInfoCollectorConfiguration config)
        {
            _eventsHolder.ConfigEventHandlers(config.WindowHandle, config.AddHook);

            SubscribeOnChangingDeviceCollection(config.DeviceChangeAction);

            if (config.OtherDeviceChangeAction != null)
            {
                SubscribeOnChangingOtherDeviceCollection(config.OtherDeviceChangeAction);
            }

            if (config.HubChangeAction != null)
            {
                SubscribeOnChangingHubsCollection(config.HubChangeAction);
            }

            _dataPoolFacade.FullResetUsbDevices();
        }

        public void SubscribeOnChangingDeviceCollection(
            Action<(List<Device> Devices, DeviceStatus Status)> deviceChangeAction) =>
            _devicePool.NeededDeviceSubscribe(deviceChangeAction);

        public void SubscribeOnChangingHubsCollection(
            Action<(List<UsbHubProperties> Devices, DeviceStatus Status)> hubChangeAction) =>
            _devicePool.HubsSubscribe(hubChangeAction);

        public void SubscribeOnChangingOtherDeviceCollection(
            Action<(List<Device> Devices, DeviceStatus Status)> otherDeviceChangeAction) =>
            _devicePool.OtherDeviceSubscribe(otherDeviceChangeAction);
    }
}