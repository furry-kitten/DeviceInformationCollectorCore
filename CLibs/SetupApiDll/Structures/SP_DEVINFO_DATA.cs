using System;
using System.Runtime.InteropServices;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures
{
    //pack=8 for 64 bit.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpDevinfoData
    {
        public UInt32 cbSize;
        public Guid ClassGuid;
        public UInt32 DevInst;
        public IntPtr Reserved;
    }
}
