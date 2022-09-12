using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using UsbDeviceInformationCollectorCore.CLibs.User32;

namespace UsbDeviceInformationCollectorCore.Services
{
    internal class SafeDeviceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeDeviceHandle() : base(true) { }

        public SafeDeviceHandle(IntPtr pHandle) : base(true) => SetHandle(pHandle);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            var bSuccess = User32.UnregisterDeviceNotification(handle);
            handle = IntPtr.Zero;
            return bSuccess;
        }

        public static implicit operator IntPtr(SafeDeviceHandle generalSafeHandle) => generalSafeHandle.handle;

        //public static explicit operator IntPtr(GeneralSafeHandle generalSafeHandle)
        //{
        //    return generalSafeHandle.handle;
        //}

        //public static explicit operator HandleRef(GeneralSafeHandle generalSafeHandle)
        //{
        //    return new HandleRef(generalSafeHandle, generalSafeHandle.handle);
        //}

        public static implicit operator HandleRef(SafeDeviceHandle generalSafeHandle) => new HandleRef(generalSafeHandle, generalSafeHandle.handle);
    }
}