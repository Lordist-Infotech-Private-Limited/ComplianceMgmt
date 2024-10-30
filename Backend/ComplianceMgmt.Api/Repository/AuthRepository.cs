using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ComplianceMgmt.Api.Repository
{
    public class AuthRepository(IConfiguration configuration, ComplianceMgmtDbContext context) : IAuthRepository
    {
        public async Task<User> Login(User loginUser)
        {
            User user = null;
            string sql = "SELECT Email, PasswordHash, Name, UserID, RoleID, ClientID, BranchID, IsActive, EmployeeID FROM Users WHERE Email = @Email AND IsActive = 1";

            using (var connection = context.CreateConnection())
            {
                user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { loginUser.Email });

                if (user == null)
                {
                    return null; // User does not exist
                }

                // Fetch the user's Role
                user.Role = await GetUserRole(connection, user.RoleId);

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(loginUser.PasswordHash, user.PasswordHash))
                {
                    return null; // Password mismatch
                }

                // Generate tokens and update user
                user.AccessToken = GenerateAccessToken(user);
                user.RefreshToken = GenerateRefreshToken();
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Set refresh token expiration

                // Update the user in the database with new tokens
                await UpdateUserTokens(connection, user);

                return user;
            }
        }

        public async Task<User> RefreshToken(string token, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var email = principal.Identity?.Name;

            string sql = "SELECT * FROM Users WHERE Email = @Email";
            User user = null;

            using (var connection = context.CreateConnection())
            {
                user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });

                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                {
                    return null; // Invalid refresh token
                }

                // Refresh tokens
                user.AccessToken = GenerateAccessToken(user);
                user.RefreshToken = GenerateRefreshToken();

                // Update the tokens in the database
                await UpdateUserTokens(connection, user);

                return user;
            }
        }

        public async Task<User> Register(User registerUser)
        {
            registerUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerUser.PasswordHash);

            string sql = @"INSERT INTO Users ( PasswordHash, RoleID, IsActive, Email) 
                       VALUES ( @PasswordHash, @RoleID, @IsActive, @Email);
                       SELECT CAST(SCOPE_IDENTITY() as int);";

            using (var connection = context.CreateConnection())
            {
                var userId = await connection.QuerySingleAsync<int>(sql, new
                {
                    registerUser.Email,
                    registerUser.PasswordHash,
                    registerUser.RoleId,
                    IsActive = true
                });

                registerUser.UserId = userId;

                return registerUser;
            }
        }

        private string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["JWT:SecretKey"]);

            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, user.Email),                      // Username claim
                new (ClaimTypes.GivenName, user.Name),                    // User's given name
                new (ClaimTypes.Role, user.Role?.RoleName ?? string.Empty), // User's role name
                new (ClaimTypes.NameIdentifier, user.UserId.ToString()),   // User ID
                new ("RoleID", user.Role?.RoleId.ToString() ?? string.Empty) // Role ID as a claim
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                IssuedAt = DateTime.UtcNow,
                Issuer = configuration["JWT:Issuer"],
                Audience = configuration["JWT:Audience"],
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JWT:SecretKey"])),
                ValidateLifetime = false // We check for expired tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        private async Task UpdateUserTokens(IDbConnection connection, User user)
        {
            string updateSql = @"UPDATE Users 
                             SET AccessToken = @AccessToken, 
                                 RefreshToken = @RefreshToken, 
                                 RefreshTokenExpiry = @RefreshTokenExpiry,
                                 IsActive = @IsActive 
                             WHERE UserID = @UserId";

            await connection.ExecuteAsync(updateSql, new
            {
                user.AccessToken,
                user.RefreshToken,
                user.RefreshTokenExpiry,
                user.IsActive,
                user.UserId
            });
        }

        private async Task<Role> GetUserRole(IDbConnection connection, int roleId)
        {
            string roleSql = "SELECT * FROM Roles WHERE RoleID = @RoleID";
            return await connection.QueryFirstOrDefaultAsync<Role>(roleSql, new { RoleID = roleId });
        }
    }
}
