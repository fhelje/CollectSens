using MeasurementApi.Services.Sensors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace MeasurementApi.Controllers
{
    [Route("api/[controller]")]
    public class DevicesController : Controller
    {
        private readonly ISensorService _sensorService;
        private readonly ICurrentSensorValues _currentSensorValues;

        public DevicesController(ICurrentSensorValues currentSensorValues) {
            _currentSensorValues = currentSensorValues;
        }


        [HttpGet(Name = "GetSensors")]
        public IActionResult GetAll()
        {
            return new ObjectResult(_currentSensorValues.Devices);
        }
    }
}