using System;
using System.Runtime.InteropServices;
using System.Text;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using UsbDeviceInformationCollectorCore.CLibs.SetupApiDll.Structures;
using static UsbDeviceInformationCollectorCore.CLibs.LibrariesConstants;

namespace UsbDeviceInformationCollectorCore.CLibs.SetupApiDll
{
    internal abstract class SetupApiDll
    {
        protected const string DevEnum = "usb";
        protected IntPtr DeviceInfo;
        protected IntPtr IntPtrBuffer;
        protected SpDevInfoData DevInfoData;
        protected uint NRequiredSize;
        protected uint RegType;
        protected uint RequiredSize;

        internal abstract bool IsDeviceInfoValidValue { get; }
        internal abstract bool IsSuccess { get; set; }

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr hDevInfo,
            ref SpDevInfoData devInfo,
            ref Guid interfaceClassGuid,
            uint memberIndex,
            ref SpDeviceInterfaceData deviceInterfaceData
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet,
            ref SpDevInfoData deviceInfoData,
            uint property,
            out uint propertyRegDataType,
            byte[] propertyBuffer,
            uint propertyBufferSize,
            out uint requiredSize
        );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiOpenDeviceInfo(
            IntPtr intPtr,
            IntPtr ptrInstanceBuf,
            IntPtr zero, int i,
            ref SpDevInfoData devInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int CM_Get_Device_ID(uint dnDevInst, ref IntPtr buffer, uint bufferLen, int ulFlags = 0);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int CM_Get_Device_ID(uint dnDevInst, StringBuilder buffer, int bufferLen, int ulFlags = 0);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int CM_Get_Device_ID_List(string filter, byte[] bffr, uint bffrLen, uint ulFlags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int CM_Get_Device_ID_List_Size(ref uint idListlen, int dnDevInst, uint ulFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid,
            [MarshalAs(UnmanagedType.LPTStr)] string enumerator, IntPtr hwndParent, uint flags);

        [DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode)]
        public static extern uint CM_Get_Device_Interface_List(ref Guid interfaceClassGuid, string deviceId,
            char[] buffer, uint bufferLength, uint flags);

        [DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode)]
        public static extern uint CM_Get_Device_Interface_List_Size(out uint size, ref Guid interfaceClassGuid,
            string deviceId, uint flags);

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
        protected static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, uint memberIndex,
            ref SpDevInfoData deviceInfoData);

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
        protected static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo,
            IntPtr devInfo,
            ref Guid interfaceClassGuid,
            uint memberIndex,
            ref SpDeviceInterfaceData deviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr lpDeviceInfoSet,
            ref SpDeviceInterfaceData oInterfaceData, ref SpDeviceInterfaceDetailData lpDeviceInterfaceDetailData,
            uint nDeviceInterfaceDetailDataSize, ref uint nRequiredSize, IntPtr lpDeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr lpDeviceInfoSet,
            ref SpDeviceInterfaceData oInterfaceData, IntPtr lpDeviceInterfaceDetailData,
            uint nDeviceInterfaceDetailDataSize, ref uint nRequiredSize, IntPtr lpDeviceInfoData);

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
        protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo,
            ref SpDeviceInterfaceData deviceInterfaceData,
            ref SpDeviceInterfaceDetailData deviceInterfaceDetailData,
            uint deviceInterfaceDetailDataSize,
            out uint requiredSize,
            ref SpDevInfoData deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern bool SetupDiGetDevicePropertyW(IntPtr deviceInfoSet,
            ref SpDevInfoData deviceInfoData,
            ref Devpropkey propertyKey,
            out ulong propertyType,
            IntPtr propertyBuffer,
            int propertyBufferSize,
            out int requiredSize,
            uint flags);

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
        protected static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet,
            ref SpDevInfoData deviceInfoData,
            uint property,
            ref uint propertyRegDataType,
            IntPtr propertyBuffer,
            uint propertyBufferSize,
            ref uint requiredSize);

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
        protected static extern int CM_Get_Device_ID(uint dnDevInst,
            IntPtr buffer,
            int bufferLen,
            int ulFlags);

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
        protected static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid,
            IntPtr enumerator,
            IntPtr hwndParent,
            uint flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)] // 2nd form uses an Enumerator
        protected static extern IntPtr SetupDiGetClassDevs(IntPtr classGuid,
            string enumerator,
            IntPtr hwndParent,
            int flags);

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
        protected static extern IntPtr SetupDiOpenDevRegKey(IntPtr hDeviceInfoSet,
            ref SpDevInfoData deviceInfoData,
            uint scope,
            uint hwProfile,
            uint keyType,
            uint samDesired);

        internal abstract bool CanFindDevice(uint memberIndex);
        internal abstract bool GetDeviceInfoByIndex(uint memberIndex);

        internal abstract string GetDeviceId();

        internal abstract string GetDevicePath();
        internal abstract string GetParentId();

        internal abstract void SetGeneralData(Guid? classGuid = null);

        internal virtual string GetBusReportedDeviceDescription()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            var busReportedDeviceDescKey = new Devpropkey
            {
                Fmtid = new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8,
                    0xa2, 0x6a, 0x0b, 0x89,
                    0x4c, 0xbd, 0xa2),
                Pid = 4
            };

            if (SetupDiGetDevicePropertyW(DeviceInfo, ref DevInfoData, ref busReportedDeviceDescKey, out _,
                    IntPtrBuffer, BufferSize,
                    out _, 0) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var deviceId = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(deviceId) == false ? deviceId : null;
        }

        internal virtual string GetDeviceAddress()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Address, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var address = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(address) == false ? address : null;
        }

        internal virtual string GetDeviceBusNumber()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.BusNumber, ref RegType,
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

        internal virtual string GetDeviceBusNumberGuid()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.BusTypeGuid, ref RegType,
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

        internal virtual string GetDeviceCapabilities()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Capabilities, ref RegType,
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

        internal virtual string GetDeviceCharacteristics()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Characteristics,
                    ref RegType,
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

        internal virtual string GetDeviceClass()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Class, ref RegType,
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

        internal virtual string GetDeviceClassGuid()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.ClassGuid, ref RegType,
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

        internal virtual string GetDeviceCompatibleIds()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Compatibleids, ref RegType,
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

        internal virtual string GetDeviceConfigFlags()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.ConfigFlags, ref RegType,
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

        internal virtual string GetDeviceDescription()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Devicedesc, ref RegType,
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

        internal virtual string GetDeviceEnumeratorName()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.EnumeratorName,
                    ref RegType,
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

        internal virtual string GetDeviceExclusive()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Exclusive, ref RegType,
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

        internal virtual string GetDeviceFriendlyName()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.FriendlyName, ref RegType,
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

        internal virtual string GetDeviceHardwareId()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.HardwareId, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var hardwareId = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(hardwareId) == false ? hardwareId : null;
        }

        internal virtual string GetDeviceInstallState()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.InstallState, ref RegType,
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

        internal virtual string GetDeviceLegacyBusType()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.LegacyBusType, ref RegType,
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

        internal virtual string GetDeviceLocalPaths()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.LocationPaths, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var localPath = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(localPath) == false ? localPath : null;
        }

        internal virtual string GetDeviceLocationInformation()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.LocationInformation,
                    ref RegType,
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

        internal virtual string GetDeviceLowerFilters()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.LowerFilters, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var lowerFilters = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(lowerFilters) == false ? lowerFilters : null;
        }

        internal virtual string GetDeviceManufacturer()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Mfg, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var mfg = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(mfg) == false ? mfg : null;
        }

        internal virtual string GetDeviceNumberDescriptionFormat()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.UiNumberDescFormat,
                    ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var numberDescriptionFormat = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(numberDescriptionFormat) == false ? numberDescriptionFormat : null;
        }

        internal virtual string GetDeviceNumberFormat()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.UiNumber, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var numberFormat = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(numberFormat) == false ? numberFormat : null;
        }

        internal virtual string GetDevicePhysicalDeviceObjectName()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.PhysicalDeviceObjectName,
                    ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var physicalDeviceObjectName = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(physicalDeviceObjectName) == false ? physicalDeviceObjectName : null;
        }

        internal virtual string GetDevicePowerData()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.DevicePowerData,
                    ref RegType,
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

        internal virtual string GetDeviceRemovalPolicy()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.RemovalPolicy, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var removalPolicy = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(removalPolicy) == false ? removalPolicy : null;
        }

        internal virtual string GetDeviceRemovalPolicyHwDefault()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.RemovalPolicyHwDefault,
                    ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var removalPolicyHwDefault = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(removalPolicyHwDefault) == false ? removalPolicyHwDefault : null;
        }

        internal virtual string GetDeviceRemovalPolicyOverride()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.RemovalPolicyOverride,
                    ref RegType,
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

        internal virtual string GetDeviceSecurityBinaryForm()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Security, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var securityBinaryForm = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(securityBinaryForm) == false ? securityBinaryForm : null;
        }

        internal virtual string GetDeviceSecuritySdsForm()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.SecuritySds, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var securitySdsForm = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(securitySdsForm) == false ? securitySdsForm : null;
        }

        internal virtual string GetDeviceService()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Service, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var service = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(service) == false ? service : null;
        }

        internal virtual string GetDeviceType()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.DevType, ref RegType,
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

        internal virtual string GetDeviceUnused0()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Unused0, ref RegType,
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

        internal virtual string GetDeviceUnused1()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Unused1, ref RegType,
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

        internal virtual string GetDeviceUnused2()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.Unused2, ref RegType,
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

        internal virtual string GetDeviceUpperFilters()
        {
            IntPtrBuffer = Marshal.AllocHGlobal(BufferSize);
            if (SetupDiGetDeviceRegistryProperty(DeviceInfo, ref DevInfoData, (uint)Spdrp.UpperFilters, ref RegType,
                    IntPtrBuffer, BufferSize, ref RequiredSize) ==
                false)
            {
                Marshal.FreeHGlobal(IntPtrBuffer);
                return null;
            }

            var upperFilters = Marshal.PtrToStringAuto(IntPtrBuffer);
            Marshal.FreeHGlobal(IntPtrBuffer);
            return string.IsNullOrEmpty(upperFilters) == false ? upperFilters : null;
        }

        /// <summary>
        ///     Frees up memory by destroying a DeviceInfoList
        /// </summary>
        /// <param name="hDevInfo"></param>
        /// <returns></returns>
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern int CM_Get_Child(out uint pdnDevInst, uint dnDevInst, int ulFlags);


        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern int CM_Get_Device_ID_Size(out uint pulLen, uint dnDevInst, int flags = 0);

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
        internal static extern int CM_Get_Parent(out uint pdnDevInst,
            uint dnDevInst,
            int ulFlags);

        [DllImport("setupapi.dll")]
        internal static extern bool SetupDiEnumDriverInfo(IntPtr deviceInfoSet, SpDevInfoData devInfoData,
            uint driverType, uint memberIndex, out SpDrvInfoDataV1 driverInfoDataV1);
        
        [DllImport("setupapi.dll")]
        internal static extern bool SetupDiEnumDriverInfo(IntPtr deviceInfoSet, SpDevInfoData devInfoData,
            uint driverType, uint memberIndex, out SpDrvInfoDataV2 driverInfoDataV1);

        internal Error GetLastError() => (Error)Marshal.GetLastWin32Error();

        internal IntPtr GetRegistryKeyForGlobalChanges() =>
            SetupDiOpenDevRegKey(DeviceInfo, ref DevInfoData, (uint)DicsFlag.FlagGlobal, 0,
                (uint)DiReg.Dev, (uint)RegKeySecurity.KeyRead);

        internal void FlushBuffer() => Marshal.FreeHGlobal(IntPtrBuffer);

        internal void SetupDiDestroyDeviceInfoList() => SetupDiDestroyDeviceInfoList(DeviceInfo);
    }
}