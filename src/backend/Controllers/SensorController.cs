using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MeasurementApi.Models;
using MeasurementApi.Services.Sensors;
using System.Threading.Tasks;
using MeasurementApi.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace MeasurementApi.Controllers
{
    [Route("api/[controller]")]
    public class SensorController : Controller
    {
        private readonly ISensorService _sensorService;
        private readonly IOptions<MeasurementOptions> _config;
        private readonly ILogger<SensorController> _logger;

        public SensorController(ISensorService sensorService, IOptions<MeasurementOptions> config, ILogger<SensorController> logger) {
            _sensorService = sensorService;
            _config = config;
            _logger = logger;
        }

        [HttpGet("{deviceId}", Name = "GetSensor")]
        public async Task<IActionResult> Get(int deviceId)
        {
            var sensor = await _sensorService.Get(deviceId);
            if (sensor == null)
            {
                return NotFound();
            }
            return new ObjectResult(sensor);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]DeviceMetadata deviceMetadata)
        {
            if (deviceMetadata == null)
            {
                return BadRequest();
            }

            var result = await _sensorService.Add(deviceMetadata);
            if (result) {
                return CreatedAtRoute("GetSensor", new { deviceId = deviceMetadata.DeviceId }, deviceMetadata);
            }
            return BadRequest();
        }

        [HttpPut("{deviceId}")]
        public IActionResult Update(int deviceId, [FromBody] DeviceMetadata item)
        {
            if (item == null || item.DeviceId != deviceId)
            {
                return BadRequest();
            }

            var sensor = _sensorService.Get(deviceId);
            if (sensor == null)
            {
                return NotFound();
            }

            _sensorService.Update(item);
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var todo = _sensorService.Get(id);
            if (todo == null)
            {
                return NotFound();
            }

            _sensorService.Remove(id);
            return new NoContentResult();
        }

        [HttpGet("{id}/histogram", Name = "GetSensorHistogram")]
        public IActionResult GetHistogram(int id) {
            _logger.LogDebug("Elasitc url: {url}", _config.Value.MetricsDatabase);
            var connSetting = new ConnectionSettings(new Uri(_config.Value.MetricsDatabase));
            connSetting.DisableDirectStreaming(true);
            var client = new ElasticClient(connSetting);
            var result = client.Search<Measument>(s =>
                 s.Index("measurement-*")
                     .Size(1)
                     .Sort(z=>z.Descending(f=>f.Timestamp))
                     .Query(q =>
                        q.DateRange(r =>
                            r.Field(f => f.Timestamp)
                             .GreaterThan(DateMath.Anchored(DateTime.Now).Subtract("7d"))
                             .LessThan(DateMath.Anchored(DateTime.Now))
                        ) && q.Term(t=>t.Field(f=>f.DeviceId).Value(id))

                    )
                    .Aggregations(a =>
                        a.DateHistogram("Temperature",
                            t => t.Field(f=>f.Timestamp)
                                  .Interval(DateInterval.Day)
                                  .Aggregations(x => x.Average("average", av=>av.Field(fi=>fi.Value)))))
            );
            if (!result.IsValid) {
                _logger.LogError(result.DebugInformation);
                _logger.LogError(result.ServerError.Error.Reason);
            }
            _logger.LogDebug("Success: {success}", result.IsValid);
            var histogram = result.Aggs
                .DateHistogram("Temperature")
                .Buckets.Select(x =>
                                    new MetricsBucket {Key = x.Date.Date.ToString("ddd MM-dd1"), Value = GetAverage(x)}
                               );
            return new ObjectResult(
                            new HistogramResult {
                                DeviceId = id,
                                CurrentTemperature = result.Documents.First().Value,
                                Timestamp = result.Documents.First().Timestamp.ToString("yyyy-MM-dd hh:mm:ss"),
                                Labels = histogram.Select(x=>x.Key),
                                Values = histogram.Select(x=>x.Value)
                            });
        }

        private decimal? GetAverage(DateHistogramBucket dateHistogramBucket) {
            if (dateHistogramBucket?.Average("average")?.Value != null)
                return Math.Round((decimal)dateHistogramBucket.Average("average").Value, 1);
            else {
                return null;
            }
        }
    }

    public class HistogramResult {
        public int DeviceId { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public IEnumerable<decimal?> Values { get; set; }
        public decimal CurrentTemperature { get; set; }
        public string Timestamp { get; set; }
    }

    public class MetricsBucket
    {
        public string Key { get; set; }
        public decimal? Value { get; set;  }
    }
}