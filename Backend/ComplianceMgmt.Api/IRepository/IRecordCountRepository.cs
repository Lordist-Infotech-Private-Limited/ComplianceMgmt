namespace ComplianceMgmt.Api.IRepository
{
    public interface IRecordCountRepository
    {
        Task<long> GetRecordCountAsync(DateTime date, string tableName);
    }
}
