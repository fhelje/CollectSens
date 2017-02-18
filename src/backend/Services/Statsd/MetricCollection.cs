using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace MeasurementApi.Services
{    
    public class MetricCollection
    {
        private readonly ConcurrentDictionary<string, AtomicLong> _counters = new ConcurrentDictionary<string, AtomicLong>();
        private readonly ConcurrentDictionary<string, TimerMetric> _metrics = new ConcurrentDictionary<string, TimerMetric>();
        private readonly ConcurrentDictionary<string, Gauge> _gauges = new ConcurrentDictionary<string, Gauge>();

        public static MetricCollection Default = new MetricCollection();
		public static MetricCollection StatsD = new MetricCollection();

        public ConcurrentDictionary<string, AtomicLong> Counters { get { return _counters; } }
        public ConcurrentDictionary<string, TimerMetric> Metrics { get { return _metrics; } }
        public ConcurrentDictionary<string, Gauge> Gauges { get { return _gauges; } }


        public AtomicLong Incr(string name, int count)
        {
            var lowerName = name.ToLower();
            var value = _counters.AddOrUpdate(lowerName, new AtomicLong(count), (k, v) => v.Increment(count));
            return value;
        }

        public void Gauge(string name, Func<double> func)
        {
            name = name.ToLower();
            var gauge = new Gauge(func);
            _gauges.AddOrUpdate(name, gauge, (n, g) => gauge);
        }

        public void Time(string name, long millis)
        {
            var lowerName = name.ToLower();

            _metrics.AddOrUpdate(lowerName, k => new TimerMetric(millis), (key, stat) =>
            {
                stat.Add(millis);
                return stat;
            });
        }

        public void Time(string name, Action action)
        {
            var watch = new Stopwatch();
            watch.Start();
            action();
            watch.Stop();
            
            Time(name, watch.ElapsedMilliseconds);
        }

        public T Time<T>(string name, Func<T> func)
        {
            var watch = new Stopwatch();
            watch.Start();
            var answer = func();
            watch.Stop();
            
            Time(name, watch.ElapsedMilliseconds);

            return answer;
        }
       
        public AtomicLong GetCounter(string name)
        {
            return _counters.GetOrAdd(name, new AtomicLong(0));
        }

        public TimerMetric GetMetric(string name)
        {
            TimerMetric metric;
            _metrics.TryGetValue(name, out metric);
            return metric ?? new TimerMetric(0);
        }

        public double GetGuage(string name)
        {
            Gauge gauge;
            _gauges.TryGetValue(name, out gauge);
            return gauge.Value;
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new SortedDictionary<string, object>
            {
              { "counters", BuildDict(_counters, c => c.Value) },
              { "metrics", BuildDict(_metrics, m => m.ToDictionary()) },
              { "gauges", BuildDict(_gauges, g => g.Value) }
            };
        }

        private static SortedDictionary<string, object> BuildDict<TRender, TValue>(IEnumerable<KeyValuePair<string, TValue>> origin, Func<TValue, TRender> display)
        {
            var destination = new SortedDictionary<string, object>();
            foreach (var kv in origin)
            {
                destination[kv.Key] = display(kv.Value);
            }
            return destination;
        }
       
        public void Clear()
        {
            _counters.Clear();
            _metrics.Clear();
            _gauges.Clear();
        }
    }
}