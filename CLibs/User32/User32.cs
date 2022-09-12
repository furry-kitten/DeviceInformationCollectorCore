using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using UsbDeviceInformationCollectorCore.Models;
using UsbDeviceInformationCollectorCore.Services;

namespace UsbDeviceInformationCollectorCore.CLibs.User32
{
    internal class User32
    {
        //private static readonly ILog Logger = LogManager.GetLogger(nameof(User32));
        private readonly bool _isWindowConfigured;
        private readonly WindowKeeper _windowKeeper;
        private SafeDeviceHandle _interfaceNotificationHandle;
        private IntPtr _buffer;

        internal User32(Window window)
        {
            _isWindowConfigured = true;
            _windowKeeper = new WindowKeeper(window);
            RegisterForDeviceChange();
        }

        /// <summary>
        ///     Registers to be notified when devices are added or removed.
        /// </summary>
        /// <returns>True if successfull, False otherwise</returns>
        public bool RegisterForDeviceChange()
        {
            if (!_isWindowConfigured)
            {
                throw new Exception(@"Please call ""ConfigTargetWindow"" before use!!");
            }

            var status = false;
            try
            {
                _interfaceNotificationHandle = new SafeDeviceHandle(RegisterDeviceNotification(_windowKeeper.Handle));
                if (_interfaceNotificationHandle != null && !_interfaceNotificationHandle.IsInvalid)
                {
                    status = true;
                }
            }
            catch (Win32Exception ex)
            {
                //Logger.DebugFormat("Error Code[{0}] : [{1}]", ex.ErrorCode, ex.Message);
            }
            finally
            {
                if (_buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_buffer);
                }
            }

            return status;
        }

        public IntPtr RegisterDeviceNotification(IntPtr hRecipient)
        {
            _buffer = IntPtr.Zero;
            var deviceInterface = new DevBroadcastDeviceInterface();
            var size = Marshal.SizeOf(deviceInterface);
            _buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(deviceInterface, _buffer, true);
            return RegisterDeviceNotification(hRecipient, _buffer, (int)(DeviceNotify.WindowHandle | DeviceNotify.AllInterfaceClasses));
        }

        /// <summary>
        /// we can also use the declaration in comment, it provides a smart way to use win32 api in DotNet.
        /// </summary>
        //[DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterDeviceNotificationA", CharSet = CharSet.Ansi)]
        //private static extern SafeNotifyHandle RegisterDeviceNotification(IntPtr hRecipient, [MarshalAs(UnmanagedType.AsAny), In] object NotificationFilter, int Flags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr notificationFilter, Int32 flags);
        
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnregisterDeviceNotification(IntPtr hHandle);
    }
}
