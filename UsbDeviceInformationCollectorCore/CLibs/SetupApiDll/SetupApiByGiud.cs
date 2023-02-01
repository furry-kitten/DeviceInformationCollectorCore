using System;
using System.Runtime.InteropServices;
using System.Text;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures;
using static UsbDeviceInformationCollectorCore.CLibs.LibrariesConstants;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll
{
    internal class SetupApiByGiud : SetupApiDll
    {
        private Guid _classGuid;
        private SpDeviceInterfaceData _dia;
        private SpDeviceInterfaceDetailData _deviceInterfaceDetailData;

        internal override bool IsDeviceInfoValidValue => DeviceInfo == new IntPtr(InvalidHandleValue);

        internal override bool IsSuccess { get; set; } = true;

        internal override bool CanFindDevice(uint memberIndex) =>
            SetupDiEnumDeviceInterfaces(DeviceInfo, IntPtr.Zero, ref _classGuid, memberIndex, ref _dia);

        internal override bool GetDeviceInfoByIndex(uint memberIndex)
        {
            _dia.CbSize = (uint)Marshal.SizeOf(_dia);
            return IsSuccess = CanFindDevice(memberIndex);
        }

        internal virtual string GetDeviceDriver()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Driver, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var value = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal override string GetDeviceId()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            // current InstanceID is at the "USBSTOR" level, so we
            // need up "move up" one level to get to the "USB" level
            //CM_Get_Parent(out var ptrPrevious, DevInfoData.DevInst, 0);

            // Now we get the InstanceID of the USB level device
            //CM_Get_Device_ID(ptrPrevious, ptrInstanceBuf, BufferSize, 0);
            CM_Get_Device_ID(DevInfoData.DevInst, IntPtrBuffer, BufferSize, 0);
            var instanceId = Marshal.PtrToStringAuto(IntPtrBuffer);

            Marshal.FreeHGlobal(IntPtrBuffer);
            return instanceId;
        }

        internal override string GetDevicePath()
        {
            DevInfoData.cbSize = (uint)Marshal.SizeOf(DevInfoData);
            _deviceInterfaceDetailData.CbSize =
                (uint)(IntPtr.Size == 8 ? 8 : 5); // I do not trust you (pragma pack(8) for x64)

            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (!SetupDiGetDeviceInterfaceDetail(DeviceInfo, ref _dia, ref _deviceInterfaceDetailData, BufferSize,
                    out NRequiredSize, ref DevInfoData))
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return _deviceInterfaceDetailData.DevicePath;
            }

            Marshal.FreeHGlobal(IntPtrBuffer);
            return _deviceInterfaceDetailData.DevicePath;
        }

        internal override string GetParentId()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            CM_Get_Parent(out var ptrPrevious, DevInfoData.DevInst, 0);
            CM_Get_Device_ID(ptrPrevious, IntPtrBuffer, BufferSize, 0);
            var instanceId = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return instanceId;
        }

        internal override void SetGeneralData(Guid? classGuid = null)
        {
            if (classGuid == null)
            {
                throw new Exception($"{nameof(classGuid)} is null");
            }

            _classGuid = classGuid.Value;
            DeviceInfo = SetupDiGetClassDevs(ref _classGuid, IntPtr.Zero, IntPtr.Zero,
                (uint)(Digcf.Present | Digcf.DeviceInterface));
            
            DevInfoData = new SpDevInfoData();
            DevInfoData.cbSize = (uint)Marshal.SizeOf(DevInfoData);
        }
    }
}