using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MJA.CloudNative.IIoTStarter.ApiService.Models;
using Npgsql;

namespace MJA.CloudNative.IIoTStarter.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly NpgsqlConnection _iotdb;
        public HistoryController(NpgsqlConnection iotdb)
        {
            _iotdb = iotdb;
        }

        [HttpGet("{smartMeterName}")]
        public async Task<ActionResult<IEnumerable<SmartMeterMeasurement>>> GetHistoryValues(string smartMeterName)
        {
            if (string.IsNullOrWhiteSpace(smartMeterName))
            {
                return NotFound();
            }

            var result = new List<SmartMeterMeasurement>();

            await _iotdb.OpenAsync();
            using (var cmd = new NpgsqlCommand("SELECT id, smartmetername, time, measurement FROM smart_meter_measurement WHERE smartmetername = @smartMeterName", _iotdb))
            {
                cmd.Parameters.AddWithValue("smartMeterName", smartMeterName);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new SmartMeterMeasurement
                        {
                            Id = reader.GetGuid(0),
                            SmartMeterName = reader.GetString(1),
                            Timestamp = reader.GetDateTime(2),
                            Value = reader.GetDouble(3)
                        });
                    }
                }
            }
            await _iotdb.CloseAsync();

            return result;
        }
    }
}
