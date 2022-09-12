using System;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures
{
    public struct SpDeviceInterfaceData
    {
        public UInt32 CbSize;
        public Guid InterfaceClassGuid;
        public UInt32 Flags;
        private IntPtr _reserved;
    }
}
