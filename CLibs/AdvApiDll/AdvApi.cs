using System;
using System.Runtime.InteropServices;
using System.Text;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using static UsbDeviceInformationCollectorCore.CLibs.LibrariesConstants;

namespace UsbDeviceInformationCollectorCore.CLibs.AdvApiDll
{
    internal class AdvApi
    {
        private const string PortName = "PortName";
        uint _type = 0;
        StringBuilder _data = new(BufferSize);
        uint _size;

        internal AdvApi()
        {
            _size = (uint)_data.Capacity;
        }

        internal string GetPortName(IntPtr deviceRegistryKey)
        {
            var value = RegQueryValueEx(deviceRegistryKey, PortName, 0, out _type,
                _data, ref _size);
            return value == (int)Error.Success ? _data.ToString() : null;
        }
        
        internal int CloseKey(IntPtr deviceRegistryKey)
        {
            return RegCloseKey(deviceRegistryKey);
        }

        /// <summary>
        /// Retrieves the type and data for the specified value name associated with an open registry key.
        /// </summary>
        /// <param name="hKey">A handle to an open registry key. The key must have been opened with the KEY_QUERY_VALUE access right.</param>
        /// <param name="lpValueName">The name of the registry value.
        /// If lpValueName is NULL or an empty string, "", the function retrieves the type and data for the key's unnamed or default value, if any.
        /// If lpValueName specifies a key that is not in the registry, the function returns ERROR_FILE_NOT_FOUND.</param>
        /// <param name="lpReserved">This parameter is reserved and must be NULL.</param>
        /// <param name="lpType">A pointer to a variable that receives a code indicating the type of data stored in the specified value. The lpType parameter can be NULL if the type code is not required.</param>
        /// <param name="lpData">A pointer to a buffer that receives the value's data. This parameter can be NULL if the data is not required.</param>
        /// <param name="lpcbData">A pointer to a variable that specifies the size of the buffer pointed to by the lpData parameter, in bytes. When the function returns, this variable contains the size of the data copied to lpData.
        /// The lpcbData parameter can be NULL only if lpData is NULL.
        /// If the data has the REG_SZ, REG_MULTI_SZ or REG_EXPAND_SZ type, this size includes any terminating null character or characters unless the data was stored without them. For more information, see Remarks.
        /// If the buffer specified by lpData parameter is not large enough to hold the data, the function returns ERROR_MORE_DATA and stores the required buffer size in the variable pointed to by lpcbData. In this case, the contents of the lpData buffer are undefined.
        /// If lpData is NULL, and lpcbData is non-NULL, the function returns ERROR_SUCCESS and stores the size of the data, in bytes, in the variable pointed to by lpcbData. This enables an application to determine the best way to allocate a buffer for the value's data.If hKey specifies HKEY_PERFORMANCE_DATA and the lpData buffer is not large enough to contain all of the returned data, RegQueryValueEx returns ERROR_MORE_DATA and the value returned through the lpcbData parameter is undefined. This is because the size of the performance data can change from one call to the next. In this case, you must increase the buffer size and call RegQueryValueEx again passing the updated buffer size in the lpcbData parameter. Repeat this until the function succeeds. You need to maintain a separate variable to keep track of the buffer size, because the value returned by lpcbData is unpredictable.
        /// If the lpValueName registry value does not exist, RegQueryValueEx returns ERROR_FILE_NOT_FOUND and the value returned through the lpcbData parameter is undefined.</param>
        /// <returns>If the function succeeds, the return value is ERROR_SUCCESS.
        /// If the function fails, the return value is a system error code.
        /// If the lpData buffer is too small to receive the data, the function returns ERROR_MORE_DATA.
        /// If the lpValueName registry value does not exist, the function returns ERROR_FILE_NOT_FOUND.</returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        private static extern int RegQueryValueEx(
            IntPtr hKey,
            string lpValueName,
            UInt32 lpReserved,
            out UInt32 lpType,
            System.Text.StringBuilder lpData,
            ref UInt32 lpcbData);

        /// <summary>
        /// Closes a handle to the specified registry key.
        /// </summary>
        /// <param name="hKey">A handle to the open key to be closed.</param>
        /// <returns>If the function succeeds, the return value is ERROR_SUCCESS.
        /// If the function fails, the return value is a nonzero error code defined in Winerror.h.</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(
            IntPtr hKey);
    }
}
