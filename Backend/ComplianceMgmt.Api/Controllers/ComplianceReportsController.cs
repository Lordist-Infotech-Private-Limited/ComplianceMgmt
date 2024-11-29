using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceMgmt.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ComplianceReportsController(IComplianceReportRepository repository) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await repository.GetAllReportsAsync();
            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(int id)
        {
            var report = await repository.GetReportByIdAsync(id);
            if (report == null)
                return NotFound();

            return Ok(report);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport(ComplianceReport report)
        {
            var newId = await repository.CreateReportAsync(report);
            report.Id = newId;
            return CreatedAtAction(nameof(GetReportById), new { id = newId }, report);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(int id, ComplianceReport report)
        {
            if (id != report.Id)
                return BadRequest();

            var success = await repository.UpdateReportAsync(report);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var success = await repository.DeleteReportAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
