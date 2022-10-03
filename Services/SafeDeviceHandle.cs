using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using UsbDeviceInformationCollectorCore.CLibs.User32Dll;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class SafeDeviceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeDeviceHandle() : base(true) { }

        public SafeDeviceHandle(IntPtr pHandle) : base(true)
        {
            SetHandle(pHandle);
        }

        public static implicit operator HandleRef(SafeDeviceHandle generalSafeHandle) =>
            new(generalSafeHandle, generalSafeHandle.handle);

        public static implicit operator IntPtr(SafeDeviceHandle generalSafeHandle) => generalSafeHandle.handle;

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            var bSuccess = User32Dll.UnregisterDeviceNotification(handle);
            handle = IntPtr.Zero;
            return bSuccess;
        }
    }
}