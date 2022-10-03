using UsbDeviceInformationCollectorCore.CLibs.AdvApiDll;
using UsbDeviceInformationCollectorCore.CLibs.SetupApiDll;

namespace UsbDeviceInformationCollectorCore.CLibs
{
    internal class LibrariesWorker
    {
        internal AdvApi AdvApi { get; } = new();
        internal SetupApi SetupApi { get; private set; } = new();

        internal string GetComPort()
        {
            var deviceRegistryKey = SetupApi.GetRegistryKeyForGlobalChanges();
            if (deviceRegistryKey.ToInt32() == LibrariesConstants.InvalidHandleValue)
            {
                return null;
            }

            var port = AdvApi.GetPortName(deviceRegistryKey);
            AdvApi.CloseKey(deviceRegistryKey);
            return port;
        }

        internal void ResetSetupApi() => SetupApi = new SetupApi();
    }
}