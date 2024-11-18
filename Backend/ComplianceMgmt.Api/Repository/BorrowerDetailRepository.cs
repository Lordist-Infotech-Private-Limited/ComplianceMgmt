using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;

namespace ComplianceMgmt.Api.Repository
{
    public class BorrowerDetailRepository(IConfiguration configuration, ComplianceMgmtDbContext context) : IBorrowerDetailRepository
    {
        public async Task<IEnumerable<StgBorrowerDetail>> GetBorrowerDetailAsync(DateTime date)
        {
            using var connection = context.CreateConnection();
            string query = @"
                SELECT * 
                FROM stgborrowerdetail 
                WHERE Date = @Date";
            return await connection.QueryAsync<StgBorrowerDetail>(query, new { Date = date });
        }

        public async Task<bool> UpdateBorrowerDetailAsync(StgBorrowerDetail updatedDetail)
        {
            using var connection = context.CreateConnection();
            string query = @"
                UPDATE stgborrowerdetail
                SET BName = @BName, 
                    BDob = @BDob,
                    sbcitizenship = @sbcitizenship,
                    BPanNo = @BPanNo,
                    Aadhaar = @Aadhaar,
                    IdType = @IdType,
                    IdNumber = @IdNumber,
                    BMonthlyIncome = @BMonthlyIncome,
                    BReligion = @BReligion,
                    BCast = @BCast,
                    BGender = @BGender,
                    BOccupation = @BOccupation,
                    IsValidated = @IsValidated,
                    RejectedReason = @RejectedReason,
                    ValidatedDate = @ValidatedDate
                WHERE Date = @Date AND BankId = @BankId AND Cin = @Cin";

            var result = await connection.ExecuteAsync(query, updatedDetail);

            return result > 0;  // Returns true if any rows were affected
        }
    }
}
