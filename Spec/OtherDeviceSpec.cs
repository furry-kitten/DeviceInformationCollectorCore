using System.Linq;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;

namespace UsbDeviceInformationCollectorCore.Spec
{
    internal class OtherDeviceSpec
    {
        internal bool IsMatch(Device device)
        {
            return string.IsNullOrEmpty(device.Id) ||
                   device.Properties.Any(property =>
                       property.PnpClassesTypes is PnPDeviceClassType.MEDIA or
                           PnPDeviceClassType.HIDClass or
                           PnPDeviceClassType.None);
        }
    }
}