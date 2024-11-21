using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IBorrowerLoanRepository
    {
        Task<BorrowerLoan> GetByPrimaryKeyAsync(DateTime date);
        Task<IEnumerable<BorrowerLoan>> GetAllByDateAsync(DateTime date);
        Task UpdateAsync(BorrowerLoan borrowerLoan);
        Task<bool> ImportBorrowerDetailsFromExcelAsync(Stream excelStream);
    }
}
