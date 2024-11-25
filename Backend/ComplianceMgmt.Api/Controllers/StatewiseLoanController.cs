using ComplianceMgmt.Api.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatewiseLoanController(IStatewiseLoanRepository repository) : ControllerBase
    {
        [HttpGet("statewise-loan-data")]
        public async Task<IActionResult> GetStatewiseData([FromQuery] string state)
        {
            if (string.IsNullOrWhiteSpace(state))
                return BadRequest("State parameter is required.");

            var data = await repository.GetStatewiseLoanDataAsync(state);
            return Ok(data);
        }
    }
}
