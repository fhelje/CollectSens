namespace MeasurementApi.Services
{
    public class MonitoringConfiguration : IMonitoringConfiguration
    {
        public string GraphiteKeyPrefixForStatsD { get; set; }

        public int SleepIntervalMs { get; set; }

        public string StatsDHost { get; set; }

        public int StatsDPort { get; set; }
    }
}