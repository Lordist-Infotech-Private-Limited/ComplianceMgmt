using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IAuthRepository
    {
        public Task<User> Login(LoginUser loginUser);
        public Task<User> Register(User registerUser);
    }
}
