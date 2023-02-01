namespace UsbDeviceInformationCollectorCore.Models
{
    public class UsbHubProperties : BaseWindowsDeviceModel
    {
        public string BusReportedDeviceDesc { get; set; }
        public string Class { get; set; }
        public string HardwareId { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
        public string PhysicalObjectName { get; set; }
        public string ParentId { get; set; }
    }
}