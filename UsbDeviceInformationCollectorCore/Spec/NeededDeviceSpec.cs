using System.Linq;
using UsbDeviceInformationCollectorCore.Enums;
using UsbDeviceInformationCollectorCore.Models;

namespace UsbDeviceInformationCollectorCore.Spec
{
    internal class NeededDeviceSpec
    {
        internal bool IsMatch(Device device)
        {
            return string.IsNullOrEmpty(device.Id) == false &&
                   device.Properties.All(property =>
                       property.PnpClassesTypes is not (PnPDeviceClassType.MEDIA or
                           PnPDeviceClassType.HIDClass or
                           PnPDeviceClassType.None));
        }
    }
}