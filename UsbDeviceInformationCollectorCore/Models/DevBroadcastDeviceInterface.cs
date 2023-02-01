using System.Runtime.InteropServices;
using UsbDeviceInformationCollectorCore.CLibs.Enums;

namespace UsbDeviceInformationCollectorCore.Models
{
    //DEV_BROADCAST_DEVICEINTERFACE Structure
    /*typedef struct _DEV_BROADCAST_DEVICEINTERFACE {
      DWORD dbcc_size;
      DWORD dbcc_devicetype;
      DWORD dbcc_reserved;
      GUID  dbcc_classguid;
      TCHAR dbcc_name[1];
    }DEV_BROADCAST_DEVICEINTERFACE *PDEV_BROADCAST_DEVICEINTERFACE;*/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class DevBroadcastDeviceInterface : DevBroadcastHdr
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        public byte[] ClassGuid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] Name;
        public DevBroadcastDeviceInterface() : base()
        {
            DeviceType = (int)DbtDevTyp.DevTypDeviceInterface;
        }
    }
}
