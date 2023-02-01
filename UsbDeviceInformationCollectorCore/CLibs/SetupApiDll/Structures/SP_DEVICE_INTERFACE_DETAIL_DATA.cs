using System.Runtime.InteropServices;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SpDeviceInterfaceDetailData
    {
        public uint CbSize;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string DevicePath;
    }
}