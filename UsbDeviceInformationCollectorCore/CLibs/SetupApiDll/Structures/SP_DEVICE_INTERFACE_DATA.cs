using System;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures
{
    public struct SpDeviceInterfaceData
    {
        public uint CbSize;
        public Guid InterfaceClassGuid;
        public uint Flags;
        private IntPtr _reserved;
    }
}