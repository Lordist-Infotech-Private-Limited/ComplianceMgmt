using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IComplianceReportRepository
    {
        Task<IEnumerable<BorrowerDetail>> GetAllReportsAsync();
        Task<ComplianceReport> GetReportByIdAsync(int id);
        Task<int> CreateReportAsync(ComplianceReport report);
        Task<bool> UpdateReportAsync(ComplianceReport report);
        Task<bool> DeleteReportAsync(int id);
    }
}
