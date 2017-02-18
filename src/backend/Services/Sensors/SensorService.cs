using MeasurementApi.Configuration;
using Dapper;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using MeasurementApi.Models;
using Microsoft.Extensions.Options;

namespace MeasurementApi.Services.Sensors
{
    public class SensorService : ISensorService
    {
        private readonly string _cs;

        public SensorService(IOptions<MeasurementOptions> configuration)
        {
            _cs = configuration.Value.SensorDatabase;
        }

        private SensorService(string cs) {
            _cs = cs;
        }

        public async Task<IEnumerable<DeviceMetadata>> GetAll()
        {
            using(var conn = new System.Data.SqlClient.SqlConnection(_cs))
            {
                conn.Open();
                var result = await conn.QueryAsync<DeviceMetadata>(@"
                SELECT deviceId as DeviceId
                      ,name as name
                      ,location as location
                      ,description as Description 
                    FROM dbo.SensorConfiguration");
                return result;
            }
        }

        public async Task<DeviceMetadata> Get(int deviceId)
        {
            using(var conn = new System.Data.SqlClient.SqlConnection(_cs))
            {
                conn.Open();
                var result = await conn.QueryAsync<DeviceMetadata>(@"
                SELECT deviceId as DeviceId
                      ,name as name
                      ,location as location
                      ,description as Description 
                    FROM dbo.SensorConfiguration 
                    WHERE deviceId = @deviceId", new { deviceId});
                return result.SingleOrDefault();
            }                 
        }

        public async Task<bool> Add(DeviceMetadata deviceMetadata)
        {
            using(var conn = new System.Data.SqlClient.SqlConnection(_cs))
            {
                conn.Open();
                await conn.ExecuteAsync(@"
                INSERT INTO dbo.SensorConfiguration
                    (DeviceId, Name, Location, Description)
                    VALUES (@DeviceId, @Name, @Location, @Description)", deviceMetadata);
                return true;
            }
        }

        public async Task<bool> Update(DeviceMetadata deviceMetadata)
        {
            using(var conn = new System.Data.SqlClient.SqlConnection(_cs))
            {
                conn.Open();
                await conn.ExecuteAsync(@"
                UPDATE dbo.SensorConfiguration
                    SET Name = @Name,
                        Location = @Location,
                        Description = @Description
                    WHERE 
                        DeviceId = @DeviceId", deviceMetadata);
                return true;
            }            
        }

        public async Task<bool> Remove(int id) {
            using(var conn = new System.Data.SqlClient.SqlConnection(_cs))
            {
                conn.Open();
                await conn.ExecuteAsync(@"
                DELETE FROM dbo.SensorConfiguration
                    WHERE DeviceId = @id", new { id = id });
                return true;
            }

        }

        public static ISensorService Create(string databaseCs) {
            return new SensorService(databaseCs);
        }
    }
}