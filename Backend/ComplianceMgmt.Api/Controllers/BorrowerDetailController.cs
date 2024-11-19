using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BorrowerDetailController(IBorrowerDetailRepository repository) : ControllerBase
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
        public async Task<IActionResult> Update([FromBody] BorrowerDetail borrowerDetail)
        {
            await repository.UpdateAsync(borrowerDetail);
            return NoContent();
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportToExcel(DateTime date)
        {
            var excelData = await repository.ExportBorrowerDetailsToExcelAsync(date);
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BorrowerDetails.xlsx");
        }
    }
}
