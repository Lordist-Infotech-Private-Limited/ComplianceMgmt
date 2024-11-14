namespace ComplianceMgmt.Api.IRepository
{
    public interface IRecordCountRepository
    {
        Task<long> GetRecordCountAsync(DateOnly date, string tableName);
    }
}
