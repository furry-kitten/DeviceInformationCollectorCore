using System;
using System.Runtime.InteropServices;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures
{
    internal struct SpDrvInfoDataV1
    {
        internal int cbSize;
        internal int DriverType;
        internal IntPtr Reserved;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        internal string Description;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        internal string MfgName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        internal string ProviderName;
    }
}