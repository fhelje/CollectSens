using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;

namespace MeasurementApi.Services.Sensors {
    public interface ISensorService
    {
        Task<IEnumerable<DeviceMetadata>> GetAll();
        Task<DeviceMetadata> Get(int deviceId);
        Task<bool> Add(DeviceMetadata deviceMetadata);
        Task<bool> Update(DeviceMetadata deviceMetadata);
        Task<bool> Remove(int id);
    }
}
