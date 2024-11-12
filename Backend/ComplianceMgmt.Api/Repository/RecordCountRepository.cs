using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using Dapper;
using System.Data;

namespace ComplianceMgmt.Api.Repository
{
    public class RecordCountRepository(IConfiguration configuration, ComplianceMgmtDbContext context) : IRecordCountRepository
    {
        public async Task<long> GetRecordCountAsync(DateTime date, string tableName)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("V_DDATE", date, DbType.Date);
                parameters.Add("V_TABLENAME", tableName, DbType.String);
                parameters.Add("V_RECORDCOUNT", dbType: DbType.Int64, direction: ParameterDirection.Output);
                using (var connection = context.CreateConnection())
                {
                    // Call the stored procedure
                    await connection.ExecuteAsync("spGetRecordCount", parameters, commandType: CommandType.StoredProcedure);
                }
                // Retrieve the output parameter value
                return parameters.Get<long>("V_RECORDCOUNT");
            }
            catch (Exception ex)
            {
                // Log or handle the error
                throw new Exception("An error occurred while calling the stored procedure.", ex);
            }
        }
    }
}
