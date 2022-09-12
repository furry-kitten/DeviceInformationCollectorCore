namespace UsbDeviceInformationCollectorCore.Models
{
    public class UsbHubProperties
    {
        public string Path { get; set; }
        public string Class { get; set; }
        public string Id { get; set; }
        public string HardwareId { get; set; }
        public string PhysicalObjectName { get; set; }
        public string BusReportedDeviceDesc { get; set; }
    }
}