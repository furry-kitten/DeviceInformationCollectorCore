using UsbDeviceInformationCollectorCore.Enums;

namespace UsbDeviceInformationCollectorCore.Models
{
    public class DeviceProperties : UsbHubProperties
    {
        public PnPDeviceClassType PnpClassesTypes { get; set; }
        public string Address { get; set; }
        public string ComPort { get; set; }
        public string Description { get; set; }
        public string FriendlyName { get; set; }
        public string Location { get; set; }
        public string Manufacturer { get; set; }
        public string ShortPath { get; set; }
        public string Type { get; set; }
    }
}