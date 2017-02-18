namespace MeasurementApi.Services
{
    public interface IMonitoringConfiguration
    {
        int     StatsDPort { get; set; }
        string  StatsDHost { get; set; }
        int     SleepIntervalMs { get; set; }
		string  GraphiteKeyPrefixForStatsD { get; set; }
    }
}