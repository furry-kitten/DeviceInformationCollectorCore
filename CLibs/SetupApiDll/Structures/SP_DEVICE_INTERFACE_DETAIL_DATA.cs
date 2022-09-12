using System;
using System.Runtime.InteropServices;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures
{
    public struct SpDeviceInterfaceDetailData
    {
        public UInt32 CbSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string DevicePath;
    }
}
