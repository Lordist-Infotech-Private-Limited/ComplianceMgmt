using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IBorrowerDetailRepository
    {
        Task<BorrowerDetail> GetByPrimaryKeyAsync(DateTime date);
        Task<IEnumerable<BorrowerDetail>> GetAllByDateAsync(DateTime date);
        Task UpdateAsync(BorrowerDetail borrowerDetail);
        Task ValidateAsync(BorrowerDetailValidationRequest request);
        Task<byte[]> ExportBorrowerDetailsToExcelAsync(DateTime date);
        Task<bool> ImportBorrowerDetailsFromExcelAsync(Stream excelStream);
    }
}
