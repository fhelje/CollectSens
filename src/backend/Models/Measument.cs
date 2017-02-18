using System;

namespace MeasurementApi.Models
{
    public class Measument 
    {
        public decimal Value { get; set; }
        public MetricType Type { get; set; }
        public string Position { get; set; }
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
