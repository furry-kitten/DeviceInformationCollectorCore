using System;
using System.Runtime.InteropServices;
using UsbDeviceInformationCollectorCore.CLibs.Enums;

namespace UsbDeviceInformationCollectorCore.Models
{
    //DEV_BROADCAST_VOLUME Structure
    /*typedef struct _DEV_BROADCAST_VOLUME {
      DWORD dbcv_size;
      DWORD dbcv_devicetype;
      DWORD dbcv_reserved;
      DWORD dbcv_unitmask;
      WORD  dbcv_flags;
    }DEV_BROADCAST_VOLUME, *PDEV_BROADCAST_VOLUME;*/
    [StructLayout(LayoutKind.Sequential)]
    internal class DevBroadcastVolume : DevBroadcastHdr
    {
        public DevBroadcastVolume() : base()
        {
            DeviceType = (int)DbtDevTyp.DevTypVolume;
        }
        public Int32 UnitMask;
        public Int16 Flags;
    }
}
