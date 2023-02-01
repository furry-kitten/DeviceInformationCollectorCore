using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures;
using static UsbDeviceInformationCollectorCore.CLibs.LibrariesConstants;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll
{
    internal class SetupApi : SetupApiDll
    {
        private const ulong CmGetIdListFilterPresent = 0x00000100;

        private const uint CM_GET_DEVICE_INTERFACE_LIST_PRESENT = 0x1;
        private const int CR_SUCCESS = 0x0;
        private static Guid GUID_DEVINTERFACE_COMPORT = new("{6AC27878-A6FA-4155-BA85-F98F491D4F33}");

        internal override bool IsDeviceInfoValidValue => DeviceInfo.ToInt64() != InvalidHandleValue;
        internal bool IsRequiredBufferSizeValid => RequiredSize < BufferSize;
        internal override bool IsSuccess { get; set; } = true;

        public static string[] GetPortNames()
        {
            var cr = CM_Get_Device_Interface_List_Size(out var size, ref GUID_DEVINTERFACE_COMPORT, null,
                CM_GET_DEVICE_INTERFACE_LIST_PRESENT);

            if (cr == CR_SUCCESS && size != 0)
            {
                var data = new char[size];
                cr = CM_Get_Device_Interface_List(ref GUID_DEVINTERFACE_COMPORT, null, data, (uint)data.Length,
                    CM_GET_DEVICE_INTERFACE_LIST_PRESENT);

                if (cr == CR_SUCCESS)
                {
                    return new string(data).Split('\0')
                        .ToList()
                        .Where(m => !string.IsNullOrEmpty(m))
                        .ToArray();
                }
            }

            return null;
        }

        //public List<string> BytesToStrings(byte[] rawData)
        //{
        //    var strings = new List<string>();
        //    var limit = rawData.Length;
        //    var idx = 0;
        //    var processing = idx < limit;

        //    while (processing)
        //    {
        //        var x = idx;
        //        while (x < limit && 0 != rawData[x])
        //        {
        //            ++x;
        //        }

        //        if (x >= limit)
        //        {
        //            break;
        //        }

        //        var sz = x - idx;
        //        var bytes = new byte[sz];
        //        Array.Copy(rawData, idx, bytes, 0, sz);
        //        try
        //        {
        //            var str = Encoding.Default.GetString(bytes);
        //            strings.Add(str);
        //            idx = x + 1;
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //            idx = limit;
        //        }

        //        processing = idx < limit;
        //    }

        //    return strings;
        //}

        internal override bool CanFindDevice(uint memberIndex)
        {
            IsSuccess = SetupDiEnumDeviceInfo(DeviceInfo, memberIndex, ref DevInfoData);
            var isInsufficientBuffer = IsInsufficientBuffer();
            return IsSuccess && isInsufficientBuffer && IsRequiredBufferSizeValid;
        }

        internal SpDrvInfoDataV1 GetDrvInfoDataV1(uint driverType, uint memberIndex, out bool isSuccess)
        {
            isSuccess = SetupDiEnumDriverInfo(DeviceInfo, DevInfoData, driverType, memberIndex,
                out SpDrvInfoDataV1 driverData);

            return isSuccess ?
                driverData :
                default;
        }
        
        internal SpDrvInfoDataV2 GetDrvInfoDataV2(uint driverType, uint memberIndex, out bool isSuccess)
        {
            isSuccess = SetupDiEnumDriverInfo(DeviceInfo, DevInfoData, driverType, memberIndex,
                out SpDrvInfoDataV2 driverData);

            return isSuccess ? driverData : default;
        }

        internal override bool GetDeviceInfoByIndex(uint memberIndex)
        {
            IsSuccess = SetupDiEnumDeviceInfo(DeviceInfo, memberIndex, ref DevInfoData);
            return IsSuccess;
        }

        internal override string GetDeviceId()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (CM_Get_Device_ID(DevInfoData.DevInst, IntPtrBuffer, BufferSize, 0) != (int)CrErrorCodes.Success)
            {
                return null;
            }

            var deviceId = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(deviceId) == false ? deviceId : null;
        }

        internal override string GetDevicePath()
        {
            uint nRequiredSize = 0;
            //basically, the first device detailed will return the false... it the really thing.
            var hDeviceInfo = DeviceInfo;
            SpDeviceInterfaceData devInterfaceData = new();
            if (SetupDiGetDeviceInterfaceDetail(hDeviceInfo, ref devInterfaceData, IntPtr.Zero, 0, ref nRequiredSize,
                    IntPtr.Zero))
            {
                return null;
            }

            //Declare Sample 3:
            SpDeviceInterfaceDetailData oDevInterfaceDetailedData = new()
            {
                //Create Buffer to save the path detailed.
                CbSize = 5 //=5
            };

            //With Sample 3:
            if (SetupDiGetDeviceInterfaceDetail(hDeviceInfo, ref devInterfaceData, ref oDevInterfaceDetailedData,
                    nRequiredSize, ref nRequiredSize, IntPtr.Zero))
            {
                //FIXME: Problem 1: The CreateFile will return 123. its means The filename, directory name, or volume label syntax is incorrect.
                //https://msdn.microsoft.com/en-us/library/windows/desktop/ms681382(v=vs.85).aspx

                //With Sample 3:
                //Log.Text += ("Acutal Path = " + oDevInterfaceDetailedData.DevicePath + "\n");
                return oDevInterfaceDetailedData.DevicePath;
            }

            return null;
        }

        internal override string GetParentId()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            CM_Get_Parent(out var ptrPrevious, DevInfoData.DevInst, 0);
            CM_Get_Device_ID(ptrPrevious, IntPtrBuffer, BufferSize, 0);
            var parentId = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return parentId;
        }

        internal override void SetGeneralData(Guid? classGuid = null)
        {
            DeviceInfo =
                SetupDiGetClassDevs(IntPtr.Zero, DevEnum, IntPtr.Zero, (int)(Digcf.Present | Digcf.AllClasses));

            DevInfoData = new SpDevInfoData();
            DevInfoData.cbSize = (uint)Marshal.SizeOf(DevInfoData);
        }

        internal bool IsInsufficientBuffer()
        {
            SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.HardwareId, ref RegType,
                IntPtr.Zero, 0, ref RequiredSize);

            return GetLastError() == Error.InsufficientBuffer;
        }

        internal (string Models, string SerialNumber, string Imei) Foo(string PortName)
        {
            if (string.IsNullOrEmpty(PortName))
            {
                return default;
            }

            string key = "";
            //SerialPort serialPort = new SerialPort(PortName);
            //if (!serialPort.IsOpen)
            //    serialPort.Open();
            //serialPort.Write("AT+DEVCONINFO\r\n");
            //Task.Delay(1000).GetAwaiter().GetResult();
            //key = serialPort.ReadExisting();
            //serialPort.Close();
            var model = Regex.Match(key, @"mn\(\w*-\w*\)", RegexOptions.IgnoreCase).Value;
            var serialNumber = Regex.Match(key, @"sn\(\w*\)", RegexOptions.IgnoreCase).Value;
            var imei = Regex.Match(key, @"imei\(\d*\)", RegexOptions.IgnoreCase).Value;
            model = GetValue(model);
            serialNumber = GetValue(serialNumber);
            imei = GetValue(imei);
            return (Models: model, SerialNumber: serialNumber, Imei: imei);
            //var deviceHandle = ti.hSource; // a handle obtained from WM_TOUCH message.
            //if (devBufer == null)
            //    devBufer = new StringBuilder(4096 * 2);
            //devBufer.Clear();

            //uint returnedDataSize = (uint)devBufer.Capacity;
            //var firstCall = GetRawInputDeviceInfo(deviceHandle, DeviceInfoTypes.RIDI_DEVICENAME, devBufer, ref returnedDataSize);
            //var firstError = Marshal.GetLastWin32Error();
            //var firtsDataSize = returnedDataSize;
            //var devName = devBufer.ToString();
            //if (string.IsNullOrEmpty(devName))
            //    devName = "No name retrieved";

            //var devInfo = new RID_DEVICE_INFO();
            //var structureSize = Convert.ToUInt32(Marshal.SizeOf<RID_DEVICE_INFO>());
            //devInfo.cbSize = structureSize;
            //returnedDataSize = structureSize;
            //var secondCall = GetRawInputDeviceInfo(deviceHandle, DeviceInfoTypes.RIDI_DEVICEINFO, ref devInfo, ref returnedDataSize);
            //var secondError = Marshal.GetLastWin32Error();
            //string hidData = "ERROR: hid data retrieval failed";
            //if (devInfo.dwType == 2)
            //{
            //    hidData = devInfo.hid.ToString();
            //}

            //Debug.LogWarning($"New touch device detected with name '{devName}' and handle '{ti.hSource}'. " +
            //                 $"\nWinapi calls returned '{(int)firstCall}' per name and '{(int)secondCall}' per data " +
            //                 $"\nError codes were '{firstError}' per name and '{secondError}' per data" +
            //                 $"\nData sizes were '{firtsDataSize}' per name and '{returnedDataSize}' per data" +
            //                 $"\nHid data structure size: '{structureSize}'; Structure type is '{devInfo.dwType}'" +
            //                 $"\nHid data: {hidData}");
        }

        private static string GetValue(string data)
        {
            return data.Remove(data.Length - 1)[(data.LastIndexOf('(') + 1)..];
        }

        [DllImport("hid.dll", SetLastError = true)]
        private static extern void HidD_GetHidGuid(ref Guid hidGuid);

        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
        //[DllImport("user32.dll", SetLastError = true)]
        //static extern uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);
        //[DllImport("user32.dll", SetLastError = true, EntryPoint = "GetRawInputDeviceInfoA")]
        //public static extern uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, ref RID_DEVICE_INFO pData, ref uint pcbSize);
        //[DllImport("user32.dll", SetLastError = true, EntryPoint = "GetRawInputDeviceInfoA", CharSet = CharSet.Ansi)]
        //public static extern uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, StringBuilder pData, ref uint pcbSize);
        //Input: Give it a handle to a device info list to deallocate from RAM.
    }

    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    //public struct DISPLAY_DEVICE
    //{
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int cb;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    //    public string DeviceName;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    //    public string DeviceString;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public DisplayDeviceStateFlags StateFlags;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    //    public string DeviceID;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    //    public string DeviceKey;
    //}

    //[Flags()]
    //public enum DisplayDeviceStateFlags : int
    //{
    //    /// <summary>The device is part of the desktop.</summary>
    //    AttachedToDesktop = 0x1,
    //    MultiDriver = 0x2,
    //    /// <summary>The device is part of the desktop.</summary>
    //    PrimaryDevice = 0x4,
    //    /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
    //    MirroringDriver = 0x8,
    //    /// <summary>The device is VGA compatible.</summary>
    //    VGACompatible = 0x10,
    //    /// <summary>The device is removable; it cannot be the primary display.</summary>
    //    Removable = 0x20,
    //    /// <summary>The device has more display modes than its output devices support.</summary>
    //    ModesPruned = 0x8000000,
    //    Remote = 0x4000000,
    //    Disconnect = 0x2000000
    //}
    //[StructLayout(LayoutKind.Sequential)]
    //internal struct RID_DEVICE_INFO_HID
    //{
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwVendorId;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwProductId;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwVersionNumber;
    //    [MarshalAs(UnmanagedType.U2)]
    //    public ushort usUsagePage;
    //    [MarshalAs(UnmanagedType.U2)]
    //    public ushort usUsage;
    //}

    //[StructLayout(LayoutKind.Sequential)]
    //internal struct RID_DEVICE_INFO_KEYBOARD
    //{
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwType;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwSubType;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwKeyboardMode;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwNumberOfFunctionKeys;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwNumberOfIndicators;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwNumberOfKeysTotal;
    //}

    //[StructLayout(LayoutKind.Sequential)]
    //internal struct RID_DEVICE_INFO_MOUSE
    //{
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwId;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwNumberOfButtons;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int dwSampleRate;
    //    [MarshalAs(UnmanagedType.U4)]
    //    public int fHasHorizontalWheel;
    //}
    //[StructLayout(LayoutKind.Explicit)]
    //internal struct RID_DEVICE_INFO
    //{
    //    [FieldOffset(0)]
    //    public int cbSize;
    //    [FieldOffset(4)]
    //    public int dwType;
    //    [FieldOffset(8)]
    //    public RID_DEVICE_INFO_MOUSE mouse;
    //    [FieldOffset(8)]
    //    public RID_DEVICE_INFO_KEYBOARD keyboard;
    //    [FieldOffset(8)]
    //    public RID_DEVICE_INFO_HID hid;
    //}
    //internal enum DeviceInfoTypes
    //{
    //    RIDI_PREPARSEDDATA = 0x20000005,
    //    RIDI_DEVICENAME = 0x20000007,
    //    RIDI_DEVICEINFO = 0x2000000B
    //}

}