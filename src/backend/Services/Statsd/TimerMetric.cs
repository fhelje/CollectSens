using System;
using System.Collections.Generic;

namespace MeasurementApi.Services
{    
    public class TimerMetric
    {
        public double Mean;
        public double Min;
        public double Max;
        public long Count;
        
        public TimerMetric() {}

        public TimerMetric(long millis)
        {
            Add(millis);
        }
        
        public void Add(double elapsedTime)
        {
            if (elapsedTime < 0) return;

            lock (this)
            {
                Count += 1;
                if (Count == 1)
                {
                    Mean = elapsedTime;
                    Min = elapsedTime;
                    Max = elapsedTime;
                }
                else
                {
                    var delta = elapsedTime - Mean;
                    Mean += (delta / Count);
                    Min = Math.Min(elapsedTime, Min);
                    Max = Math.Max(elapsedTime, Max);                    
                }
            }
        }

        public TimerMetric SnapshotAndClear()
        {
            lock (this)
            {
                var metric = new TimerMetric
                {
                    Mean = Mean, 
                    Max = Max, 
                    Count = Count, 
                    Min = Min
                };

				Mean = 0;
				Min = 0;
				Max = 0;
				Count = 0;

                return metric;
            }
        }

        public IDictionary<string, object> ToDictionary()
        {
            lock (this)
            {
                return new Dictionary<string, object>
                       {
                           { "average", Mean },
                           { "count", Count },
                           { "max", Max },
                           { "min", Min }
                       };
            }
        }
    }
}