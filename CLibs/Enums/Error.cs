namespace UsbDeviceInformationCollectorCore.CLibs.Enums
{
    public enum Error : long
    {
        Success = 0,
        InvalidFunction = 1,
        FileNotFound = 2,
        PathNotFound = 3,
        TooManyOpenFiles = 4,
        AccessDenied = 5,
        InvalidHandle = 6,
        ArenaTrashed = 7,
        NotEnoughMemory = 8,
        InvalidBlock = 9,
        BadEnvironment = 10,
        BadFormat = 11,
        InvalidAccess = 12,
        InvalidData = 13,
        OutOfMemory = 14,
        InsufficientBuffer = 122,
        MoreData = 234,
        NoMoreItems = 259,
        ServiceSpecificError = 1066,
        InvalidUserBuffer = 1784
    }
}
