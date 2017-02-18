using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MeasurementApi.Services
{    
    public class MetricCollector
    {
	    private readonly MetricCollection _collection;
	    private readonly ConcurrentDictionary<string, long> lastCounters = new ConcurrentDictionary<string, long>();

	    public MetricCollector(MetricCollection collection)
	    {
		    _collection = collection;
	    }

	    public IDictionary<string, long> GetCountersDeltas()
        {
            var deltas = new Dictionary<string, long>();
			foreach (var kv in _collection.Counters)
            {
	            var value = kv.Value.Value;
				deltas[kv.Key] = Delta(lastCounters.GetOrAdd(kv.Key, 0), value);
				lastCounters[kv.Key] = value;
            }
            return deltas;
        }
        
        public IDictionary<string, TimerMetric> GetMetrics()
        {
            var deltas = new Dictionary<string, TimerMetric>();
			foreach (var kv in _collection.Metrics)
            {
                deltas[kv.Key] = kv.Value.SnapshotAndClear();
			}
            return deltas;
        }
        
        public IDictionary<string, Gauge> GetGauges()
        {
			return _collection.Gauges;
        }

        private long Delta(long oldValue, long newValue)
        {
            if (oldValue <= newValue)
                return newValue - oldValue;

            var count = (AtomicLong.MaxValue.Value - oldValue) + (newValue - AtomicLong.MinValue.Value);
            return count + 1;
        }
    }
}