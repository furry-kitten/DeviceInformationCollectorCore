using System;

namespace UsbDeviceInformationCollectorCore
{
    public delegate IntPtr WndProcDelegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
}