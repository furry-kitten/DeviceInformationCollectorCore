namespace UsbDeviceInformationCollectorCore.CLibs.Enums
{
    /// <summary>
    /// Access rights for registry key objects.
    /// </summary>
    public enum RegKeySecurity : uint
    {
        /// <summary>
        /// Combines the STANDARD_RIGHTS_REQUIRED, KEY_QUERY_VALUE, KEY_SET_VALUE, KEY_CREATE_SUB_KEY, KEY_ENUMERATE_SUB_KEYS, KEY_NOTIFY, and KEY_CREATE_LINK access rights.
        /// </summary>
        KeyAllAccess = 0xF003F,

        /// <summary>
        /// Reserved for system use.
        /// </summary>
        KeyCreateLink = 0x0020,

        /// <summary>
        /// Required to create a subkey of a registry key.
        /// </summary>
        KeyCreateSubKey = 0x0004,

        /// <summary>
        /// Required to enumerate the subkeys of a registry key.
        /// </summary>
        KeyEnumerateSubKeys = 0x0008,

        /// <summary>
        /// Equivalent to KEY_READ.
        /// </summary>
        KeyExecute = 0x20019,

        /// <summary>
        /// Required to request change notifications for a registry key or for subkeys of a registry key.
        /// </summary>
        KeyNotify = 0x0010,

        /// <summary>
        /// Required to query the values of a registry key.
        /// </summary>
        KeyQueryValue = 0x0001,

        /// <summary>
        /// Combines the STANDARD_RIGHTS_READ, KEY_QUERY_VALUE, KEY_ENUMERATE_SUB_KEYS, and KEY_NOTIFY values.
        /// </summary>
        KeyRead = 0x20019,

        /// <summary>
        /// Required to create, delete, or set a registry value.
        /// </summary>
        KeySetValue = 0x0002,

        /// <summary>
        /// Indicates that an application on 64-bit Windows should operate on the 32-bit registry view. For more information, see Accessing an Alternate Registry View. This flag must be combined using the OR operator with the other flags in this table that either query or access registry values. Windows 2000:  This flag is not supported.
        /// </summary>
        KeyWow6432Key = 0x0200,

        /// <summary>
        /// Indicates that an application on 64-bit Windows should operate on the 64-bit registry view. For more information, see Accessing an Alternate Registry View. This flag must be combined using the OR operator with the other flags in this table that either query or access registry values. Windows 2000:  This flag is not supported.
        /// </summary>
        KeyWow6464Key = 0x0100,

        /// <summary>
        /// Combines the STANDARD_RIGHTS_WRITE, KEY_SET_VALUE, and KEY_CREATE_SUB_KEY access rights.
        /// </summary>
        KeyWrite = 0x20006
    }
}
