using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;

namespace ComplianceMgmt.Api.Repository
{
    public class BorrowerLoanRepository(IConfiguration configuration, ComplianceMgmtDbContext context) : IBorrowerLoanRepository
    {
        public async Task<BorrowerLoan> GetByPrimaryKeyAsync(DateTime date)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"SELECT * FROM stgborrowerloan 
                      WHERE `Date` = @Date";
                return await connection.QuerySingleOrDefaultAsync<BorrowerLoan>(query, new { Date = date});
            }
        }

        public async Task<IEnumerable<BorrowerLoan>> GetAllByDateAsync(DateTime date)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"SELECT * FROM stgborrowerloan WHERE `Date` = @Date";
                return await connection.QueryAsync<BorrowerLoan>(query, new { Date = date });
            }
        }

        public async Task UpdateAsync(BorrowerLoan borrowerLoan)
        {
            using (var connection = context.CreateConnection())
            {

                var query = @"UPDATE stgborrowerloan 
                      SET SanctAmount = @SanctAmount, LoanStatus = @LoanStatus, IsValidated = @IsValidated 
                      WHERE `Date` = @Date AND BankId = @BankId AND Cin = @Cin AND BLoanNo = @BLoanNo";
                await connection.ExecuteAsync(query, borrowerLoan);
            }
        }
    }
}
