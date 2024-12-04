using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IRecordCountRepository
    {
        Task<IEnumerable<TableSummary>> GetRecordCountAsync(DateOnly date);
        //Task BulkInsertAsync(string connectionString, string tableName, IEnumerable<dynamic> data);
        Task<bool> FetchAndInsertAllTablesAsync();
    }
}
