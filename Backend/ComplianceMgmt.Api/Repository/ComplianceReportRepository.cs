using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;

namespace ComplianceMgmt.Api.Repository
{
    public class ComplianceReportRepository(ComplianceMgmtDbContext context) : IComplianceReportRepository
    {
        public async Task<IEnumerable<BorrowerDetail>> GetAllReportsAsync()
        {
            using var connection = await context.CreateDefaultConnectionAsync();

            var query = "SELECT * FROM stgborrowerdetail";
            return await connection.QueryAsync<BorrowerDetail>(query);
        }

        public async Task<ComplianceReport> GetReportByIdAsync(int id)
        {
            using var connection = await context.CreateDefaultConnectionAsync();
            var query = "SELECT * FROM ComplianceReports WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<ComplianceReport>(query, new { Id = id });
        }

        public async Task<int> CreateReportAsync(ComplianceReport report)
        {
            using var connection = await context.CreateDefaultConnectionAsync();
            var query = @"
            INSERT INTO ComplianceReports (ReportName, GeneratedDate, Status)
            VALUES (@ReportName, @GeneratedDate, @Status);
            SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.ExecuteScalarAsync<int>(query, report);
        }

        public async Task<bool> UpdateReportAsync(ComplianceReport report)
        {
            using var connection = await context.CreateDefaultConnectionAsync();
            var query = @"
            UPDATE ComplianceReports
            SET ReportName = @ReportName, GeneratedDate = @GeneratedDate, Status = @Status
            WHERE Id = @Id";

            var result = await connection.ExecuteAsync(query, report);
            return result > 0;
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            using var connection = await context.CreateDefaultConnectionAsync();
            var query = "DELETE FROM ComplianceReports WHERE Id = @Id";
            var result = await connection.ExecuteAsync(query, new { Id = id });
            return result > 0;
        }
    }
}
