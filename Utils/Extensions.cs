using Newtonsoft.Json;

namespace UsbDeviceInformationCollectorCore.Utils
{
    internal static class Extensions
    {
        public static string ToJson<T>(this T obj) => JsonConvert.SerializeObject(obj);
    }
}