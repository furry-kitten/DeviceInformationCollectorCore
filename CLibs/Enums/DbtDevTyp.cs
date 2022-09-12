using System;

namespace UsbDeviceInformationCollectorCore.CLibs.Enums
{
    [Flags]
    public enum DbtDevTyp : uint
    {
        DevTypOem = 0x00000000,                //OEM-defined device type
        DevTypDevNode = 0x00000001,            //Devnode number
        DevTypVolume = 0x00000002,             //Logical volume
        DevTypPort = 0x00000003,               //Serial, parallel
        DevTypNet = 0x00000004,                //Network resource
        DevTypDeviceInterface = 0x00000005,    //Device interface class
        DevTypHandle = 0x00000006              //File system handle
    }
}
