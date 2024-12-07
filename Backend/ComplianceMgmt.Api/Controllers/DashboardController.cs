using ComplianceMgmt.Api.IRepository;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DashboardController(IRecordCountRepository recordCountRepository) : ControllerBase
    {
        [HttpGet("GetRecordCount")]
        public async Task<IActionResult> GetRecordCount([FromQuery] DateOnly date)
        {
            try
            {
                var result = await recordCountRepository.FetchAndInsertAllTablesAsync();
                if (result)
                {
                    return Ok(await recordCountRepository.GetRecordCountAsync(date));
                }
                else
                {
                    Log.Error("Failed to fetch and insert records into all tables. Please check the logs for more details.");
                    return StatusCode(500, "Failed to fetch and insert records into all tables. Please check the logs for more details.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
