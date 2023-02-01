using System.Runtime.InteropServices;

namespace UsbDeviceInformationCollectorCore.Models
{
    //Structure with information for RegisterDeviceNotification.
    //DEV_BROADCAST_HDR Structure
    /*typedef struct _DEV_BROADCAST_HDR {
      DWORD dbch_size;
      DWORD dbch_devicetype;
      DWORD dbch_reserved;
    }DEV_BROADCAST_HDR, *PDEV_BROADCAST_HDR;*/
    [StructLayout(LayoutKind.Sequential)]
    internal class DevBroadcastHdr
    {
        public int Size;
        public int DeviceType;
        public int Reserved;
        public DevBroadcastHdr()
        {
            Size = (int)Marshal.SizeOf(this);
        }
    }
}
