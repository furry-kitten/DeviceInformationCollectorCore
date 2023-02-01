using System;
using System.Collections.Generic;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;

namespace UsbDeviceInformationCollectorCore
{
    public interface ICollectorUsbDiFacade
    {
        List<Device> GetAllConnectedDevices();
        void FullReset();

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
        void Init(UsbDeviceInfoCollectorConfiguration config);

        void SubscribeOnChangingDeviceCollection(
            Action<(List<Device> Devices, DeviceStatus Status)> deviceChangeAction);

        void SubscribeOnChangingHubsCollection(
            Action<(List<UsbHubProperties> Devices, DeviceStatus Status)> hubChangeAction);

        void SubscribeOnChangingOtherDeviceCollection(
            Action<(List<Device> Devices, DeviceStatus Status)> otherDeviceChangeAction);
    }
}