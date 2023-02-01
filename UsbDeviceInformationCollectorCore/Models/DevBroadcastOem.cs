using System;
using System.Runtime.InteropServices;
using UsbDeviceInformationCollectorCore.CLibs.Enums;

namespace UsbDeviceInformationCollectorCore.Models
{
    //DEV_BROADCAST_OEM Structure
    /*typedef struct _DEV_BROADCAST_OEM {
      DWORD dbco_size;
      DWORD dbco_devicetype;
      DWORD dbco_reserved;
      DWORD dbco_identifier;
      DWORD dbco_suppfunc;
    }DEV_BROADCAST_OEM, *PDEV_BROADCAST_OEM;*/
    [StructLayout(LayoutKind.Sequential)]
    internal class DevBroadcastOem : DevBroadcastHdr
    {
        public DevBroadcastOem() : base()
        {
            DeviceType = (int)DbtDevTyp.DevTypOem;
        }
        public Int32 Identifier;
        public Int32 SuppFunc;
    }
}
