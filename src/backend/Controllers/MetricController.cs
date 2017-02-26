using System;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using MeasurementApi.Services.Sensors;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MeasurementApi.Configuration;

namespace MeasurementApi.Controllers
{

    [Route("api/[controller]")]
    public class MetricController : Controller
    {
        private readonly ICurrentSensorValues _currentSensorValues;
        private readonly ILogger<MetricController> _logger;
        private readonly IOptions<MeasurementOptions> _config;

        public MetricController(ICurrentSensorValues currentSensorValues, IOptions<MeasurementOptions> config, ILogger<MetricController> logger) {
            _currentSensorValues = currentSensorValues;
            _logger = logger;
            _config = config;
        }
        // GET api/values
        // GET api/values/5
        [HttpGet("{id}/{type}/{value}")]
        public async Task<decimal> Get(int id, MetricType type, decimal value)
        {
            _logger.LogDebug("Get sensor");
            var device = await _currentSensorValues.SetSensorData(id, type, value);
            if (device == null)
            {
                _logger.LogError("Faild to set measurement in cache");
            }

            _logger.LogDebug("Save to elastic");               
            var client = new Nest.ElasticClient(new Uri(_config.Value.MetricsDatabase));
            var now = DateTime.Now;
            var measurement = new Measument
            {
                DeviceId = id.ToString(),
                Type = MetricType.Temperature,
                Position = device.Metadata.Location,
                Timestamp = now,
                Value = value
            };
            var elasticResult = await client.IndexAsync<Measument>(measurement, s => s.Index("measurement-2017"));
            if (!elasticResult.IsValid)
            {
                _logger.LogError("Failed to save measurement");                
            }
            return device.Metadata.Value ?? 10M;
        }
    }
}
