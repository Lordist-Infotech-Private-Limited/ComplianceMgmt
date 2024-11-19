using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IBorrowerMortgageOtherRepository
    {
        Task<BorrowerMortgageOther> GetByPrimaryKeyAsync(DateTime date);
        Task<IEnumerable<BorrowerMortgageOther>> GetAllByDateAsync(DateTime date);
        Task UpdateAsync(BorrowerMortgageOther borrowerMortgageOther);
    }
}
