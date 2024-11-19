using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;

namespace ComplianceMgmt.Api.Repository
{
    public class BorrowerMortgageOtherRepository(IConfiguration configuration, ComplianceMgmtDbContext context) : IBorrowerMortgageOtherRepository
    {
        public async Task<BorrowerMortgageOther> GetByPrimaryKeyAsync(DateTime date)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"SELECT * FROM stgborrowermortgageother 
                      WHERE `Date` = @Date";
                return await connection.QuerySingleOrDefaultAsync<BorrowerMortgageOther>(query, new { Date = date });
            }
        }

        public async Task<IEnumerable<BorrowerMortgageOther>> GetAllByDateAsync(DateTime date)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"SELECT * FROM stgborrowermortgageother WHERE `Date` = @Date";
                return await connection.QueryAsync<BorrowerMortgageOther>(query, new { Date = date });
            }
        }

        public async Task UpdateAsync(BorrowerMortgageOther borrowerMortgageOther)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"UPDATE stgborrowermortgageother 
                      SET PresentValue = @PresentValue, IsValidated = @IsValidated, RejectedReason = @RejectedReason 
                      WHERE `Date` = @Date AND BankId = @BankId AND Cin = @Cin AND BLoanNo = @BLoanNo AND CollType = @CollType";
                await connection.ExecuteAsync(query, borrowerMortgageOther);
            }
        }
    }
}
