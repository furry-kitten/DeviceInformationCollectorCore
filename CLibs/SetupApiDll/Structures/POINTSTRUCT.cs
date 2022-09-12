using System.Runtime.InteropServices;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Pointstruct
    {
        public int x;
        public int y;
        public Pointstruct(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
