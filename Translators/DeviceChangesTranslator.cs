using System;
using System.Runtime.InteropServices;
using NLog;
using UsbDeviceInformationCollectorCore.CLibs.Enums;
using UsbDeviceInformationCollectorCore.Models;

namespace UsbDeviceInformationCollectorCore.Translators
{
    internal class DeviceChangesTranslator
    {
        private readonly Func<DevBroadcastDeviceInterface, string> _devInterfaceToString =
            x => $"size:{x.Size}, type: {x.DeviceType}, reserved: {x.Reserved}, name:{new string(x.Name)}";

        private readonly Func<DevBroadcastHdr, string> _hdrToString =
            x => $"size:{x.Size}, type: {x.DeviceType}, reserved: {x.Reserved}";

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private Lazy<string> _toString;

        public override string ToString() => _toString.Value;

        internal string GetDevicePath(IntPtr deviceInfoHandle)
        {
            var devType = (DbtDevTyp)Marshal.ReadIntPtr(deviceInfoHandle, 4);
            var devicePath = string.Empty;
            switch (devType)
            {
                case DbtDevTyp.DevTypDeviceInterface:
                    var devInterfaceInfo =
                        Marshal.PtrToStructure(deviceInfoHandle, typeof(DevBroadcastDeviceInterface)) as
                            DevBroadcastDeviceInterface;

                    devicePath = new string(devInterfaceInfo?.Name ?? Array.Empty<char>());
                    _toString = new Lazy<string>(() => _devInterfaceToString(devInterfaceInfo));
                    break;

                default:
                    var devHdrInfo = new DevBroadcastHdr();
                    _toString = new Lazy<string>(() => _hdrToString(devHdrInfo));
                    _logger.Debug($"Unhandled DbtDevTyp: {devHdrInfo}");
                    break;
            }

            _logger.Info(_toString.Value);
            return devicePath;
        }
    }
}