using ComplianceMgmt.Api.Models;

namespace ComplianceMgmt.Api.IRepository
{
    public interface IAuthRepository
    {
        public Task<User> Login(User loginUser);
        public Task<User> Register(User registerUser);
        public Task<User> RefreshToken(string token, string refreshToken);
    }
}
