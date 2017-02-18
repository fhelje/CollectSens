using System;

namespace MeasurementApi.Services
{
    public class Gauge
    {
        private readonly Func<double> func;

        public Gauge(Func<double> func)
        {
            this.func = func;
        }

        public double Value
        {
            get
            {
                try
                {
                    return func();
                }
                catch (Exception) { }

                return 0;
            }
        }
    }
}