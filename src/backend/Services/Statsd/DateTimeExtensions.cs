using System;

namespace MeasurementApi.Services
{    
    public static class DateTimeExtensions
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        

        public static long ToUnixTime(this DateTime self)
        {
            return (long)((self - epoch).TotalMilliseconds) / 1000;
        }
    }
}