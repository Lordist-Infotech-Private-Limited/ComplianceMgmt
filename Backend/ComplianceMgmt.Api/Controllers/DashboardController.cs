using ComplianceMgmt.Api.IRepository;
using Microsoft.AspNetCore.Mvc;

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
                var result = await recordCountRepository.GetRecordCountAsync(date);
                var result1 = await recordCountRepository.FetchAndInsertAllTablesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
