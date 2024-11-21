using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface ICoBorrowerDetailsRepository
    {
        Task<CoBorrowerDetails> GetByPrimaryKeyAsync(DateTime date);
        Task<IEnumerable<CoBorrowerDetails>> GetAllByDateAsync(DateTime date);
        Task UpdateAsync(CoBorrowerDetails coBorrowerDetails);
        Task<bool> ImportBorrowerDetailsFromExcelAsync(Stream excelStream);

    }
}
