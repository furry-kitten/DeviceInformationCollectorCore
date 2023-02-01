using System;
using System.Runtime.InteropServices;
using UsbDeviceInformationCollectorCore.CLibs.Enums;

namespace UsbDeviceInformationCollectorCore.Models
{
    //DEV_BROADCAST_HANDLE Structure
    /*typedef struct _DEV_BROADCAST_HANDLE {
      DWORD      dbch_size;
      DWORD      dbch_devicetype;
      DWORD      dbch_reserved;
      HANDLE     dbch_handle;
      HDEVNOTIFY dbch_hdevnotify;
      GUID       dbch_eventguid;
      LONG       dbch_nameoffset;
      BYTE       dbch_data[1];
    }DEV_BROADCAST_HANDLE *PDEV_BROADCAST_HANDLE;*/
    [StructLayout(LayoutKind.Sequential)]
    internal class DevBroadcastHandle : DevBroadcastHdr
    {
        public IntPtr Handle;
        public IntPtr HDevNotify;
        public Guid EventGuid;
        public long NameOffset;
        public byte Data;
        public byte Data1;
        public DevBroadcastHandle()
        {
            Size = (int)Marshal.SizeOf(this);
            DeviceType = (int)DbtDevTyp.DevTypHandle;
        }
    }
}
