using ComplianceMgmt.Api.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DashboardController(IRecordCountRepository recordCountRepository) : ControllerBase
    {
        [HttpGet("GetRecordCount")]
        public async Task<IActionResult> GetRecordCount([FromQuery] DateTime date, [FromQuery] string tableName)
        {
            try
            {
                var recordCount = await recordCountRepository.GetRecordCountAsync(date, tableName);
                return Ok(new { Date = date, TableName = tableName, RecordCount = recordCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
