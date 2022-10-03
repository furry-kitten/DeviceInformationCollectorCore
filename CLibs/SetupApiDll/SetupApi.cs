using System;
using System.Runtime.InteropServices;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures;
using static UsbDeviceInformationCollectorCore.CLibs.LibrariesConstants;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll
{
    internal class SetupApi
    {
        private const string DevEnum = "USB";

        private IntPtr _deviceInfo;
        private IntPtr _intPtrBuffer;
        private SpDevinfoData _devInfoData;
        private uint _regType;
        private uint _requiredSize;
        private bool _success = true;

        internal SetupApi()
        {
            SetGeneralData();
        }

        internal void SetGeneralData()
        {
            _intPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            _deviceInfo = SetupDiGetClassDevs(IntPtr.Zero, DevEnum, IntPtr.Zero, (int)(Digcf.Present | Digcf.AllClasses));
            _devInfoData = new SpDevinfoData();
            _devInfoData.cbSize = (uint)Marshal.SizeOf(_devInfoData);
        }

        internal bool IsDeviceInfoValidValue => _deviceInfo.ToInt64() != InvalidHandleValue;
        internal bool IsRequiredBufferSizeValid => _requiredSize < BufferSize;
        internal bool IsSuccess => _success;

        internal bool CanFindDevice(uint memberIndex)
        {
            _success = SetupDiEnumDeviceInfo(_deviceInfo, memberIndex, ref _devInfoData);
            var isInsufficientBuffer = IsInsufficientBuffer();
            return _success && isInsufficientBuffer && IsRequiredBufferSizeValid;
        }

        internal bool IsInsufficientBuffer()
        {
            SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.HardwareId, ref _regType, IntPtr.Zero, 0, ref _requiredSize);
            return GetLastError() == Error.InsufficientBuffer;
        }

        internal void SetRequiredBufferSize()
        {
            SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.HardwareId, ref _regType, IntPtr.Zero, 0, ref _requiredSize);
        }

        internal Error GetLastError() => (Error)Marshal.GetLastWin32Error();

        internal bool GetDeviceInfoByIndex(uint memberIndex)
        {
            _success = SetupDiEnumDeviceInfo(_deviceInfo, memberIndex, ref _devInfoData);
            return _success;
        }

        internal IntPtr GetRegistryKeyForGlobalChanges() =>
            SetupDiOpenDevRegKey(_deviceInfo, ref _devInfoData, (uint)DicsFlag.FlagGlobal, 0,
                (uint)DiReg.Dev, (uint)RegKeySecurity.KeyRead);

        internal string GetDeviceHardwareId()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.HardwareId, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var hardwareId = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(hardwareId) == false ? hardwareId : null;
        }

        internal string GetDeviceLocalPaths()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.LocationPaths, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var localPath = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(localPath) == false ? localPath : null;
        }

        internal string GetDeviceAddress()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Address, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var address = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(address) == false ? address : null;
        }

        internal string GetDeviceUpperFilters()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.UpperFilters, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var upperFilters = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(upperFilters) == false ? upperFilters : null;
        }

        internal string GetDeviceNumberDescriptionFormat()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.UiNumberDescFormat, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var numberDescriptionFormat = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(numberDescriptionFormat) == false ? numberDescriptionFormat : null;
        }

        internal string GetDeviceNumberFormat()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.UiNumber, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var numberFormat = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(numberFormat) == false ? numberFormat : null;
        }

        internal string GetDeviceService()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Service, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var service = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(service) == false ? service : null;
        }

        internal string GetDeviceSecuritySdsForm()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.SecuritySds, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var securitySdsForm = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(securitySdsForm) == false ? securitySdsForm : null;
        }

        internal string GetDeviceSecurityBinaryForm()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Security, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var securityBinaryForm = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(securityBinaryForm) == false ? securityBinaryForm : null;
        }

        internal string GetDeviceRemovalPolicyHwDefault()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.RemovalPolicyHwDefault, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var removalPolicyHwDefault = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(removalPolicyHwDefault) == false ? removalPolicyHwDefault : null;
        }

        internal string GetDeviceRemovalPolicy()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.RemovalPolicy, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var removalPolicy = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(removalPolicy) == false ? removalPolicy : null;
        }

        internal string GetDevicePhysicalDeviceObjectName()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.PhysicalDeviceObjectName, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var physicalDeviceObjectName = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(physicalDeviceObjectName) == false ? physicalDeviceObjectName : null;
        }

        internal string GetDeviceManufacturer()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Mfg, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var mfg = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(mfg) == false ? mfg : null;
        }

        internal string GetDeviceLowerFilters()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.LowerFilters, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var lowerFilters = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(lowerFilters) == false ? lowerFilters : null;
        }

        internal string GetDeviceUnused0()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Unused0, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceUnused1()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Unused1, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceUnused2()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Unused2, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceClass()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Class, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceClassGuid()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.ClassGuid, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceBusNumber()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.BusNumber, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceBusNumberGuid()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.BusTypeGuid, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceCapabilities()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Capabilities, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceCharacteristics()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Characteristics, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceCompatibleIds()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Compatibleids, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceConfigFlags()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.ConfigFlags, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDevicePowerData()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.DevicePowerData, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceType()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.DevType, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceDriver()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Driver, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceLegacyBusType()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.LegacyBusType, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceExclusive()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Exclusive, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceInstallState()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.InstallState, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceEnumeratorName()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.EnumeratorName, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceRemovalPolicyOverride()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.RemovalPolicyOverride, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceFriendlyName()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.FriendlyName, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceLocationInformation()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.LocationInformation, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceDescription()
        {
            if (SetupDiGetDeviceRegistryProperty(_deviceInfo, ref _devInfoData, (uint)Spdrp.Devicedesc, ref _regType,
                    _intPtrBuffer, BufferSize, ref _requiredSize) ==
                false)
            {
                return null;
            }

            var value = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(value) == false ? value : null;
        }

        internal string GetDeviceId()
        {
            if (CM_Get_Device_ID(_devInfoData.DevInst, _intPtrBuffer, BufferSize, 0) != (int)CrErrorCodes.Success)
            {
                return null;
            }

            var deviceId = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(deviceId) == false ? deviceId : null;

        }

        internal string GetBusReportedDeviceDescription()
        {
            var busReportedDeviceDescKey = new Devpropkey
            {
                Fmtid = new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8,
                    0xa2, 0x6a, 0x0b, 0x89,
                    0x4c, 0xbd, 0xa2),
                Pid = 4
            };
            if (SetupDiGetDevicePropertyW(_deviceInfo, ref _devInfoData, ref busReportedDeviceDescKey, out _, _intPtrBuffer, BufferSize,
                    out _, 0) == false)
            {
                return null;
            }

            var deviceId = Marshal.PtrToStringAuto(_intPtrBuffer);
            return string.IsNullOrEmpty(deviceId) == false ? deviceId : null;

        }

        /// <summary>
        ///     The SetupDiEnumDeviceInfo function retrieves a context structure for a device information element of the specified
        ///     device information set. Each call returns information about one device. The function can be called repeatedly
        ///     to get information about several devices.
        /// </summary>
        /// <param name="deviceInfoSet">
        ///     A handle to the device information set for which to return an SP_DEVINFO_DATA structure
        ///     that represents a device information element.
        /// </param>
        /// <param name="memberIndex">A zero-based index of the device information element to retrieve.</param>
        /// <param name="deviceInfoData">
        ///     A pointer to an SP_DEVINFO_DATA structure to receive information about an enumerated
        ///     device information element. The caller must set DeviceInfoData.cbSize to sizeof(SP_DEVINFO_DATA).
        /// </param>
        /// <returns></returns>
        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, uint memberIndex, ref SpDevinfoData deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern int CM_Get_Device_ID(IntPtr pdnDevInst, IntPtr buffer, uint bufferLen, uint flags);

        /// <summary>
        ///     A call to SetupDiEnumDeviceInterfaces retrieves a pointer to a structure that identifies a specific device
        ///     interface
        ///     in the previously retrieved DeviceInfoSet array. The call specifies a device interface by passing an array index.
        ///     To retrieve information about all of the device interfaces, an application can loop through the array,
        ///     incrementing the array index until the function returns zero, indicating that there are no more interfaces.
        ///     The GetLastError API function then returns No more data is available.
        /// </summary>
        /// <param name="hDevInfo">Input: Give it the HDEVINFO we got from SetupDiGetClassDevs()</param>
        /// <param name="devInfo">Input (optional)</param>
        /// <param name="interfaceClassGuid">Input</param>
        /// <param name="memberIndex">Input: "Index" of the device you are interested in getting the path for.</param>
        /// <param name="deviceInterfaceData">Output: This function fills in an "SP_DEVICE_INTERFACE_DATA" structure.</param>
        /// <returns></returns>
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo,
            IntPtr devInfo,
            ref Guid interfaceClassGuid,
            uint memberIndex,
            ref SpDeviceInterfaceData deviceInterfaceData);

        /// <summary>
        ///     Gives us a device path, which is needed before CreateFile() can be used.
        /// </summary>
        /// <param name="hDevInfo">Input: Wants HDEVINFO which can be obtained from SetupDiGetClassDevs()</param>
        /// <param name="deviceInterfaceData">Input: Pointer to a structure which defines the device interface.</param>
        /// <param name="deviceInterfaceDetailData">Output: Pointer to a structure, which will contain the device path.</param>
        /// <param name="deviceInterfaceDetailDataSize">Input: Number of bytes to retrieve.</param>
        /// <param name="requiredSize">Output (optional): The number of bytes needed to hold the entire struct</param>
        /// <param name="deviceInfoData">Output</param>
        /// <returns></returns>
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo,
            ref SpDeviceInterfaceData deviceInterfaceData,
            ref SpDeviceInterfaceDetailData deviceInterfaceDetailData,
            uint deviceInterfaceDetailDataSize,
            out uint requiredSize,
            ref SpDevinfoData deviceInfoData);

        /// <summary>
        ///     Frees up memory by destroying a DeviceInfoList
        /// </summary>
        /// <param name="hDevInfo"></param>
        /// <returns></returns>
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);
        //Input: Give it a handle to a device info list to deallocate from RAM.

        /// <summary>
        ///     Returns a HDEVINFO type for a device information set.
        ///     We will need the HDEVINFO as in input parameter for calling many of the other SetupDixxx() functions.
        /// </summary>
        /// <param name="classGuid"></param>
        /// <param name="enumerator"></param>
        /// <param name="hwndParent"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)] // 1st form using a ClassGUID
        private static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid,
            IntPtr enumerator,
            IntPtr hwndParent,
            uint flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)] // 2nd form uses an Enumerator
        private static extern IntPtr SetupDiGetClassDevs(IntPtr classGuid,
            string enumerator,
            IntPtr hwndParent,
            int flags);

        /// <summary>
        ///     The SetupDiGetDeviceRegistryProperty function retrieves the specified device property.
        ///     This handle is typically returned by the SetupDiGetClassDevs or SetupDiGetClassDevsEx function.
        /// </summary>
        /// <param Name="deviceInfoSet">Handle to the device information set that contains the interface and its underlying device.</param>
        /// <param Name="deviceInfoData">Pointer to an SP_DEVINFO_DATA structure that defines the device instance.</param>
        /// <param Name="property">Device property to be retrieved. SEE MSDN</param>
        /// <param Name="propertyRegDataType">
        ///     Pointer to a variable that receives the registry data Type. This parameter can be
        ///     NULL.
        /// </param>
        /// <param Name="propertyBuffer">Pointer to a buffer that receives the requested device property.</param>
        /// <param Name="propertyBufferSize">Size of the buffer, in bytes.</param>
        /// <param Name="requiredSize">
        ///     Pointer to a variable that receives the required buffer size, in bytes. This parameter can
        ///     be NULL.
        /// </param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet,
            ref SpDevinfoData deviceInfoData,
            uint property,
            ref uint propertyRegDataType,
            IntPtr propertyBuffer,
            uint propertyBufferSize,
            ref uint requiredSize);

        /// <summary>
        ///     The CM_Get_Parent function obtains a device instance handle to the parent node of a specified device node, in the
        ///     local machine's device tree.
        /// </summary>
        /// <param name="pdnDevInst">
        ///     Caller-supplied pointer to the device instance handle to the parent node that this function
        ///     retrieves. The retrieved handle is bound to the local machine.
        /// </param>
        /// <param name="dnDevInst">Caller-supplied device instance handle that is bound to the local machine.</param>
        /// <param name="ulFlags">Not used, must be zero.</param>
        /// <returns>
        ///     If the operation succeeds, the function returns CR_SUCCESS. Otherwise, it returns one of the CR_-prefixed
        ///     error codes defined in cfgmgr32.h.
        /// </returns>
        [DllImport("setupapi.dll")]
        private static extern int CM_Get_Parent(out uint pdnDevInst,
            uint dnDevInst,
            int ulFlags);

        /// <summary>
        ///     The CM_Get_Device_ID function retrieves the device instance ID for a specified device instance, on the local
        ///     machine.
        /// </summary>
        /// <param name="dnDevInst">Caller-supplied device instance handle that is bound to the local machine.</param>
        /// <param name="buffer">
        ///     Address of a buffer to receive a device instance ID string. The required buffer size can be
        ///     obtained by calling CM_Get_Device_ID_Size, then incrementing the received value to allow room for the string's
        ///     terminating NULL.
        /// </param>
        /// <param name="bufferLen">Caller-supplied length, in characters, of the buffer specified by Buffer.</param>
        /// <param name="ulFlags">Not used, must be zero.</param>
        /// <returns>
        ///     If the operation succeeds, the function returns CR_SUCCESS. Otherwise, it returns one of the CR_-prefixed
        ///     error codes defined in cfgmgr32.h.
        /// </returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        private static extern int CM_Get_Device_ID(uint dnDevInst,
            IntPtr buffer,
            int bufferLen,
            int ulFlags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiGetDevicePropertyW(IntPtr deviceInfoSet,
            ref SpDevinfoData deviceInfoData,
            ref Devpropkey propertyKey,
            out ulong propertyType,
            IntPtr propertyBuffer, 
            int propertyBufferSize,
            out int requiredSize,
            uint flags);

        /// <summary>
        ///     The SetupDiOpenDevRegKey function opens a registry key for device-specific configuration information.
        /// </summary>
        /// <param name="hDeviceInfoSet">
        ///     A handle to the device information set that contains a device information element that
        ///     represents the device for which to open a registry key.
        /// </param>
        /// <param name="deviceInfoData">
        ///     A pointer to an SP_DEVINFO_DATA structure that specifies the device information element in
        ///     DeviceInfoSet.
        /// </param>
        /// <param name="scope">
        ///     The scope of the registry key to open. The scope determines where the information is stored. The scope can be
        ///     global or specific to a hardware profile. The scope is specified by one of the following values:
        ///     DICS_FLAG_GLOBAL Open a key to store global configuration information. This information is not specific to a
        ///     particular hardware profile. For NT-based operating systems this opens a key that is rooted at HKEY_LOCAL_MACHINE.
        ///     The exact key opened depends on the value of the KeyType parameter.
        ///     DICS_FLAG_CONFIGSPECIFIC Open a key to store hardware profile-specific configuration information. This key is
        ///     rooted at one of the hardware-profile specific branches, instead of HKEY_LOCAL_MACHINE. The exact key opened
        ///     depends on the value of the KeyType parameter.
        /// </param>
        /// <param name="hwProfile">
        ///     A hardware profile value, which is set as follows:
        ///     If Scope is set to DICS_FLAG_CONFIGSPECIFIC, HwProfile specifies the hardware profile of the key that is to be
        ///     opened.
        ///     If HwProfile is 0, the key for the current hardware profile is opened.
        ///     If Scope is DICS_FLAG_GLOBAL, HwProfile is ignored.
        /// </param>
        /// <param name="keyType">
        ///     The type of registry storage key to open, which can be one of the following values:
        ///     DIREG_DEV Open a hardware key for the device.
        ///     DIREG_DRV Open a software key for the device.
        ///     For more information about a device's hardware and software keys, see Driver Information in the Registry.
        /// </param>
        /// <param name="samDesired">
        ///     The registry security access that is required for the requested key. For information about
        ///     registry security access values of type REGSAM, see the Microsoft Windows SDK documentation.
        /// </param>
        /// <returns>
        ///     If the function is successful, it returns a handle to an opened registry key where private configuration data
        ///     pertaining to this device instance can be stored/retrieved.
        ///     If the function fails, it returns INVALID_HANDLE_VALUE. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("Setupapi", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetupDiOpenDevRegKey(IntPtr hDeviceInfoSet,
            ref SpDevinfoData deviceInfoData,
            uint scope,
            uint hwProfile,
            uint keyType,
            uint samDesired);
    }
}