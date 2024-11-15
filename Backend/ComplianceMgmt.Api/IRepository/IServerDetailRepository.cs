using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IServerDetailRepository
    {
        Task<IEnumerable<ServerDetail>> GetServerDetailsAsync();
    }
}
