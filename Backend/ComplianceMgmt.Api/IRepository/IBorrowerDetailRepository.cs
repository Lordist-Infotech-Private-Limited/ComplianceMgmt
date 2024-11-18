using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IBorrowerDetailRepository
    {
        Task<IEnumerable<StgBorrowerDetail>> GetBorrowerDetailAsync(DateTime date);
        Task<bool> UpdateBorrowerDetailAsync(StgBorrowerDetail updatedDetail);
    }
}
