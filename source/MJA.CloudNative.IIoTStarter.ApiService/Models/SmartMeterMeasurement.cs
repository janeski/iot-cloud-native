﻿namespace MJA.CloudNative.IIoTStarter.ApiService.Models
{
    public class SmartMeterMeasurement
    {
        public Guid Id { get; set; }
        public string SmartMeterName { get; set; }
        public DateTime Time { get; set; }
        public double Measurement { get; set; }
    }
}
