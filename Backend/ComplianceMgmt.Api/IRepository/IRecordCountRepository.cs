using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IRecordCountRepository
    {
        Task<IEnumerable<TableSummary>> GetRecordCountAsync(DateOnly date);
        Task FetchDataFromClientDatabasesAsync();
    }
}
