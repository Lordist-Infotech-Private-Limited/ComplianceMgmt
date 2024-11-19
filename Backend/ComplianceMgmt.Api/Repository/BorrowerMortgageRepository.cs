using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;

namespace ComplianceMgmt.Api.Repository
{
    public class BorrowerMortgageRepository(IConfiguration configuration, ComplianceMgmtDbContext context) : IBorrowerMortgageRepository
    {
        public async Task<BorrowerMortgage> GetByPrimaryKeyAsync(DateTime date)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"SELECT * FROM stgborrowermortgage 
                      WHERE `Date` = @Date";
                return await connection.QuerySingleOrDefaultAsync<BorrowerMortgage>(query, new { Date = date});
            }
        }

        public async Task<IEnumerable<BorrowerMortgage>> GetAllByDateAsync(DateTime date)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"SELECT * FROM stgborrowermortgage WHERE `Date` = @Date";
                return await connection.QueryAsync<BorrowerMortgage>(query, new { Date = date });
            }
        }

        public async Task UpdateAsync(BorrowerMortgage borrowerMortgage)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"UPDATE stgborrowermortgage 
                      SET PresentValue = @PresentValue, IsValidated = @IsValidated, RejectedReason = @RejectedReason 
                      WHERE `Date` = @Date AND BankId = @BankId AND Cin = @Cin AND BLoanNo = @BLoanNo AND PropType = @PropType";
                await connection.ExecuteAsync(query, borrowerMortgage);
            } 
        }
    }
}
