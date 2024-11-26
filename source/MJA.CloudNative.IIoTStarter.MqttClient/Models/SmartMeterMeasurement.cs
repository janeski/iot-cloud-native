namespace MJA.CloudNative.IIoTStarter.MqttClient.Models
{
    public class SmartMeterMeasurement
    {
        public string SmartMeterName { get; set; }
        public DateTime Time { get; set; }
        public double Measurement { get; set; }
    }

}
