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

        [HttpPost("import")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid Excel file.");
            }

            try
            {
                // Read the file stream
                using var stream = file.OpenReadStream();
                // Pass the stream to the service for processing
                var result = await repository.ImportBorrowerDetailsFromExcelAsync(stream);

                if (result)
                {
                    return Ok("Excel file imported successfully.");
                }
                else
                {
                    return StatusCode(500, "Error importing data from Excel.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
