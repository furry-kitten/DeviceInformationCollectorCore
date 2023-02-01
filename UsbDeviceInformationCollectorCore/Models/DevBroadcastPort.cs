using System.Runtime.InteropServices;
using UsbDeviceInformationCollectorCore.CLibs.Enums;

namespace UsbDeviceInformationCollectorCore.Models
{
    //DEV_BROADCAST_PORT Structure
    /*typedef struct _DEV_BROADCAST_PORT {
      DWORD dbcp_size;
      DWORD dbcp_devicetype;
      DWORD dbcp_reserved;
      TCHAR dbcp_name[1];
    }DEV_BROADCAST_PORT *PDEV_BROADCAST_PORT;*/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class DevBroadcastPort : DevBroadcastHdr
    {
        public DevBroadcastPort() : base()
        {
            DeviceType = (int)DbtDevTyp.DevTypPort;
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] Name;
    }
}
