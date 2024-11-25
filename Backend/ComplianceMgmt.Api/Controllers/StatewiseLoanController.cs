using ComplianceMgmt.Api.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatewiseLoanController(IStatewiseLoanRepository repository) : ControllerBase
    {
        [HttpGet("filtered-loan-data")]
        public async Task<IActionResult> GetFilteredLoanData([FromQuery] string filterType)
        {
            if (string.IsNullOrWhiteSpace(filterType))
                return BadRequest("FilterType parameter is required.");

            // Validate filterType
            var validFilters = new[] { "Disbursement", "Sanction", "Outstanding", "NPA" };
            if (!validFilters.Contains(filterType, StringComparer.OrdinalIgnoreCase))
                return BadRequest("Invalid FilterType. Valid values are: Disbursement, Sanction, Outstanding, NPA.");

            var data = await repository.GetFilteredLoanDataAsync(filterType);
            return Ok(data);
        }
    }
}
