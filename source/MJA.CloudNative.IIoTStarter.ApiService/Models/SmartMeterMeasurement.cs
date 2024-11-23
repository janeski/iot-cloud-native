namespace MJA.CloudNative.IIoTStarter.ApiService.Models
{
    public class SmartMeterMeasurement
    {
        public Guid Id { get; set; }
        public string SmartMeterName { get; set; }
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }
}
