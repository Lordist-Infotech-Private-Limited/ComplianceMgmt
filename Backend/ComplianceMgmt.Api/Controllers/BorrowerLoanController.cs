using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BorrowerLoanController(IBorrowerLoanRepository repository) : ControllerBase
    {
        [HttpGet("{date}")]
        public async Task<IActionResult> GetByPrimaryKey(DateTime date)
        {
            var record = await repository.GetByPrimaryKeyAsync(date);
            return record != null ? Ok(record) : NotFound();
        }

        [HttpGet("byDate/{date}")]
        public async Task<IActionResult> GetAllByDate(DateTime date)
        {
            var records = await repository.GetAllByDateAsync(date);
            return Ok(records);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] BorrowerLoan borrowerLoan)
        {
            await repository.UpdateAsync(borrowerLoan);
            return NoContent();
        }
    }
}
