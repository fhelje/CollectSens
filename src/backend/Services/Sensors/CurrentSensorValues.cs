using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeasurementApi.Models;
using Microsoft.Extensions.Logging;
using Nest;

namespace MeasurementApi.Services.Sensors
{
    public interface ICurrentSensorValues {
        Task<bool> SetSensorData(int deviceId, MetricType metricType, decimal value);
        decimal GetSensorData(int deviceId, MetricType metricType);
        IEnumerable<Device> Devices { get; }
    }

    public class CurrentSensorValues : ICurrentSensorValues {
        private readonly ISensorService _sensorService;
        private readonly ILogger<CurrentSensorValues> _logger;
        private readonly Dictionary<int, Device> _deviceCache;

        public CurrentSensorValues(
            Dictionary<int, Device> deviceCache,
            ISensorService sensorService,
            ILogger<CurrentSensorValues> logger) {
            _deviceCache = deviceCache;
            _sensorService = sensorService;
            _logger = logger;
        }

        public IEnumerable<Device> Devices => _deviceCache.Values;

        public async Task<bool> SetSensorData(int deviceId, MetricType metricType, decimal value) {
            _logger.LogDebug("Looking for device id {deviceId}", deviceId);
            if (!_deviceCache.ContainsKey(deviceId)) {
                _logger.LogInformation("No device found");
                var device = await GetOrCreateSensor(deviceId);
                _deviceCache[device.DeviceId] = device;
            }
            _deviceCache[deviceId].AddTemperature(value);
            return true;
        }

        private async Task<Device> GetOrCreateSensor(int deviceId) {
            _logger.LogDebug("Find device with id {deviceId}", deviceId);
            var metadata = await _sensorService.Get(deviceId);
            if (metadata == null) {
                metadata = new DeviceMetadata {DeviceId = deviceId};
                _logger.LogInformation("Saving device {deviceId}", deviceId);
                var result = await _sensorService.Add(metadata);
                if(!result) {
                    _logger.LogError("Failed saving device {deviceId}", deviceId);
                    throw new Exception("Unable to add sensor");
                }
                _logger.LogInformation("Device {deviceId} saved", deviceId);

            }
            var device = new Device(metadata);
            return device;
        }

        public decimal GetSensorData(int deviceId, MetricType metricType) {
            return _deviceCache.ContainsKey(deviceId)
                ? _deviceCache[deviceId].Temperature.Value
                : -1;
        }
    }

    public class Device {
        private readonly Temperature _temperature = new Temperature();

        public Device(DeviceMetadata metadata) {
            Metadata = metadata;
        }
        public DeviceMetadata Metadata { get; set; }
        public DateTime LastMeasurement { get; set; }
        public string Timestamp => LastMeasurement.ToString("u");
        public DeviceStatus Status { get; set; }
        public int DeviceId => Metadata.DeviceId;

        public void AddTemperature(decimal value) {
            LastMeasurement = DateTime.Now;
            Status = DeviceStatus.Ok;
            _temperature.Value = value;
        }

        public Temperature Temperature => _temperature;

    }

    public enum DeviceStatus {
        Initializing,
        Ok,
        Late
    }

    public abstract class Sensor<T> {
        public abstract MetricType Type { get; }
        public T Value { get; set; }
    }

    public class Temperature : Sensor<decimal> {
        public override MetricType Type => MetricType.Temperature;
    }
}