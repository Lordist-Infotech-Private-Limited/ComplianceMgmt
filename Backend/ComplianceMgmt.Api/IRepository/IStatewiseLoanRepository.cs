using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IStatewiseLoanRepository
    {
        Task<IEnumerable<StatewiseLoanSummary>> GetFilteredLoanDataAsync(string filterType);
    }
}
