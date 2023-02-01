using System;
using System.Runtime.InteropServices;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures
{
    //pack=8 for 64 bit.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpDevInfoData
    {
        public uint cbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public IntPtr Reserved;
    }
}