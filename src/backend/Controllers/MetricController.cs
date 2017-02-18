using System;
using Microsoft.AspNetCore.Mvc;
using MeasurementApi.Models;
using MeasurementApi.Services.Sensors;
using Microsoft.Extensions.Logging;

namespace MeasurementApi.Controllers
{

    [Route("api/[controller]")]
    public class MetricController : Controller
    {
        private readonly ICurrentSensorValues _currentSensorValues;
        private readonly ILogger<MetricController> _logger;

        public MetricController(ICurrentSensorValues currentSensorValues, ILogger<MetricController> logger) {
            _currentSensorValues = currentSensorValues;
            _logger = logger;
        }
        // GET api/values
        // GET api/values/5
        [HttpGet("{id}/{type}/{value}/{timestamp}")]
        public string Get(int id, MetricType type, decimal value, string timestamp)
        {
            _logger.LogDebug("Get sensor");
            _currentSensorValues.SetSensorData(id, type, value);
            _logger.LogDebug("Save to elastic");
            var client = new Nest.ElasticClient(new Uri("http://localhost:9200"));
            var date = DateTime.Parse(System.Net.WebUtility.HtmlDecode(timestamp));
            var measurement = new Measument
            {
                DeviceId = id.ToString(),
                Type = MetricType.Temperature,
                Position = "Köket",
                Timestamp = date,
                Value = value
            };
            client.Index<Measument>(measurement, s => s.Index("measurement-2017"));
            return $"Device: {id} reported {type.ToString().ToLower()} {value} at {date.ToString("yyyy-MM-dd hh:MM:ss")}";
        }
    }
}
