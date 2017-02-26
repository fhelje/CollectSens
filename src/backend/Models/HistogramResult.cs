using System.Collections.Generic;

namespace backend.Models {
    public class HistogramResult {
        public int DeviceId { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public IEnumerable<decimal?> Values { get; set; }
        public decimal CurrentTemperature { get; set; }
        public string Timestamp { get; set; }
    }
}