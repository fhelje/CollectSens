using System;
using System.Threading;
using Serilog;

namespace MeasurementApi.Services
{
    public class StatsDWriter
    {

        private IMonitoringConfiguration _config;
        private Thread _senderThread;
        private bool _stopSenderThread;
	    private MetricCollector _metricCollectorStatsD;

	    public void Start(IMonitoringConfiguration config)
        {
            _config = config;
			_metricCollectorStatsD = new MetricCollector(MetricCollection.StatsD);
            _senderThread = new Thread(WriterThread) { Name = "carbon-sender", IsBackground = true };

            _senderThread.Start();
        }

        private void WriterThread()
        {
            Log.Information("Started StatsD sender thread");

            while (!_stopSenderThread)
            {

                var epoch = DateTime.UtcNow.ToUnixTime();
				var countersStatsD = _metricCollectorStatsD.GetCountersDeltas();

                try
                {

                    foreach (var counter in countersStatsD)
                    {
                        if (counter.Value == 0)
                            continue;

                        StatsDClient.Send(counter.Key + ".count", counter.Value, "c");
                    }

                }
                catch
                {
                    // ignored
                }

                Thread.Sleep(_config.SleepIntervalMs);
            }

            Log.Information("StatsD sender shutdown");

        }
        public static string SanitizeKey(string key)
        {
            return key.Replace('/', '.').Replace(':', '_').Replace(' ', '_').ToLower();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);

            try
            {
                _stopSenderThread = true;

                if (_senderThread != null && _senderThread.IsAlive)
                    _senderThread.Join(2000);
            }
            catch
            {
                // ignored
            }
        }

        ~StatsDWriter()
        {
            Dispose(false);
        }

        public void Stop()
        {
            _stopSenderThread = true;
        }
    }

}