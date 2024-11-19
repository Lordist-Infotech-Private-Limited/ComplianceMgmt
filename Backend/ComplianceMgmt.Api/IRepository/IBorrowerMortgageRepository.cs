using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IBorrowerMortgageRepository
    {
        Task<BorrowerMortgage> GetByPrimaryKeyAsync(DateTime date);
        Task<IEnumerable<BorrowerMortgage>> GetAllByDateAsync(DateTime date);
        Task UpdateAsync(BorrowerMortgage borrowerMortgage);
    }
}
