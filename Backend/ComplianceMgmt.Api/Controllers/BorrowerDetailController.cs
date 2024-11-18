using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BorrowerDetailController(IBorrowerDetailRepository borrowerDetailRepository) : ControllerBase
    {
        // Get all records by a specific date (or any other filter)
        [HttpGet("GetBorrowerDetail")]
        public async Task<IActionResult> GetBorrowerDetail([FromQuery] DateTime date)
        {
            var records = await borrowerDetailRepository.GetBorrowerDetailAsync(date);

            if (records == null || !records.Any())
            {
                return NotFound("No records found for the specified date.");
            }

            return Ok(records);
        }

        // Update a record (for all fields or individual fields)
        [HttpPost("UpdateBorrowerDetail")]
        public async Task<IActionResult> UpdateBorrowerDetail([FromBody] StgBorrowerDetail updatedDetail)
        {
            var isUpdated = await borrowerDetailRepository.UpdateBorrowerDetailAsync(updatedDetail);

            if (!isUpdated)
            {
                return NotFound("Record not found or update failed.");
            }

            return Ok(updatedDetail);
        }
    }
}
