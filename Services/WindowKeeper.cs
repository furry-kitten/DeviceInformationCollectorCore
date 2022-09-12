using System;
using System.Windows;
using System.Windows.Interop;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class WindowKeeper
    {
        public WindowKeeper(Window window)
        {
            Window = window;
            Handle = new WindowInteropHelper(Window).EnsureHandle();
        }

        public IntPtr Handle { get; }

        public Window Window { get; }
    }
}