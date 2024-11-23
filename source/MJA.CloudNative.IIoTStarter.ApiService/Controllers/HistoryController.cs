using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MJA.CloudNative.IIoTStarter.ApiService.Models;

namespace MJA.CloudNative.IIoTStarter.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        [HttpGet("{smartMeterName}")]
        public async Task<ActionResult<IEnumerable<SmartMeterMeasurement>>> GetHistoryValues(string smartMeterName)
        {
            var result = new List<SmartMeterMeasurement>();

            await Task.Run(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    result.Add(new SmartMeterMeasurement
                    {
                        Id = Guid.NewGuid(),
                        SmartMeterName = smartMeterName,
                        Timestamp = DateTime.UtcNow.AddMinutes(-i * 10),
                        Value = new Random().NextDouble() * 100
                    });
                }
            });

            if (string.IsNullOrWhiteSpace(smartMeterName))
            {
                return NotFound();
            }

            return result;
        }
    }
}
