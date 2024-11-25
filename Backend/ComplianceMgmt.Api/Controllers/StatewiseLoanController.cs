using ComplianceMgmt.Api.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatewiseLoanController(IStatewiseLoanRepository repository) : ControllerBase
    {
        [HttpGet]
        [Route("GetStatewiseLoanData")]
        public async Task<IActionResult> GetStatewiseLoanData()
        {
            var data = await repository.GetStatewiseLoanDataAsync();
            return Ok(data);
        }
    }
}
