namespace UsbDeviceInformationCollectorCore.Models
{
    public class Device : BaseWindowsDeviceModel
    {
        public DeviceProperties[] Properties { get; set; }
        public string ComPort { get; set; }
        public string Id { get; set; }
        public string Manufacture { get; set; }
        public string ModelName { get; set; }
        public string ModelNumber { get; set; }
        public string ShortPath { get; set; }
    }
}