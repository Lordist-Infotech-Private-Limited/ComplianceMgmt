using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;

namespace ComplianceMgmt.Api.Repository
{
    public class CoBorrowerDetailsRepository(IConfiguration configuration, ComplianceMgmtDbContext context) : ICoBorrowerDetailsRepository
    {
        public async Task<CoBorrowerDetails> GetByPrimaryKeyAsync(DateTime date)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"SELECT * FROM stgcoborrowerdetails 
                      WHERE `Date` = @Date";
                return await connection.QuerySingleOrDefaultAsync<CoBorrowerDetails>(query, new { Date = date});
            }
        }

        public async Task<IEnumerable<CoBorrowerDetails>> GetAllByDateAsync(DateTime date)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"SELECT * FROM stgcoborrowerdetails WHERE `Date` = @Date";
                return await connection.QueryAsync<CoBorrowerDetails>(query, new { Date = date });
            }
        }

        public async Task UpdateAsync(CoBorrowerDetails coBorrowerDetails)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"UPDATE stgcoborrowerdetails 
                      SET CbName = @CbName, CbDob = @CbDob, CbCitizenship = @CbCitizenship, CbPanNo = @CbPanNo,
                          CbAadhaar = @CbAadhaar, IdType = @IdType, IdNumber = @IdNumber, CbMonthlyIncome = @CbMonthlyIncome,
                          CbReligion = @CbReligion, CbCast = @CbCast, CbGender = @CbGender, CbOccupation = @CbOccupation,
                          IsValidated = @IsValidated, RejectedReason = @RejectedReason, ValidatedDate = @ValidatedDate
                      WHERE `Date` = @Date AND BankId = @BankId AND Cin = @Cin AND CbCin = @CbCin";
                await connection.ExecuteAsync(query, coBorrowerDetails);
            }
        }
    }
}
