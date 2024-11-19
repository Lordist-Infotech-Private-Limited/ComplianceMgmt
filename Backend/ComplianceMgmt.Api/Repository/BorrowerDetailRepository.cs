using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;

namespace ComplianceMgmt.Api.Repository
{
    public class BorrowerDetailRepository(IConfiguration configuration, ComplianceMgmtDbContext context) : IBorrowerDetailRepository
    {
        public async Task<BorrowerDetail> GetByPrimaryKeyAsync(DateTime date)
        {
            using var connection = context.CreateConnection();

            var query = @"SELECT * FROM stgborrowerdetail 
                      WHERE `Date` = @Date";
            return await connection.QuerySingleOrDefaultAsync<BorrowerDetail>(query, new { Date = date});
        }

        public async Task<IEnumerable<BorrowerDetail>> GetAllByDateAsync(DateTime date)
        {
            using var connection = context.CreateConnection();

            var query = @"SELECT * FROM stgborrowerdetail WHERE `Date` = @Date";
            return await connection.QueryAsync<BorrowerDetail>(query, new { Date = date });
        }

        public async Task UpdateAsync(BorrowerDetail borrowerDetail)
        {
            using var connection = context.CreateConnection();

            var query = @"UPDATE stgborrowerdetail 
                      SET BName = @BName, BDob = @BDob, sbCitizenship = @sbCitizenship, BPanNo = @BPanNo,
                          Aadhaar = @Aadhaar, IdType = @IdType, IdNumber = @IdNumber, BMonthlyIncome = @BMonthlyIncome,
                          BReligion = @BReligion, BCast = @BCast, BGender = @BGender, BOccupation = @BOccupation,
                          IsValidated = @IsValidated, RejectedReason = @RejectedReason, ValidatedDate = @ValidatedDate
                      WHERE `Date` = @Date AND BankId = @BankId AND Cin = @Cin";
            await connection.ExecuteAsync(query, borrowerDetail);
        }
    }
}
