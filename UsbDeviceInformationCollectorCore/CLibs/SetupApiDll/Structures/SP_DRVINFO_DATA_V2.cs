using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures
{
    internal struct SpDrvInfoDataV2
    {
        internal FILETIME DriverDate;
        internal int cbSize;
        internal int DriverType;
        internal IntPtr Reserved;
        internal long DriverVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        internal string Description;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        internal string MfgName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        internal string ProviderName;
    }
}