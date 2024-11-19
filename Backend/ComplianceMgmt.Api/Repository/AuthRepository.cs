using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using ComplianceMgmt.Api.Services;
using Dapper;
using System.Data;

namespace ComplianceMgmt.Api.Repository
{
    public class AuthRepository(IConfiguration configuration, ComplianceMgmtDbContext context, TokenService tokenService) : IAuthRepository
    {
        public async Task<User> Login(LoginUser loginUser)
        {
            User user = null;
            string sql = "SELECT UserID, LoginId, UserName, MailId, MobileNo, Designation, CreatedBy, CreateDate, UpdatedBy, UpdatedDate, Password, IsActive, LastLogin FROM usermaster WHERE MailId = @MailId AND IsActive = 1";

            using (var connection = context.CreateConnection())
            {
                try
                {
                    user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { loginUser.MailId });
                    if (user == null) return null;

                    // Verify password
                    if (!BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password))
                    {
                        return null; // Password mismatch
                    }

                    // Generate JWT token
                    var token = tokenService.GenerateToken(user.UserID);

                    // Store token in sessioninfo table
                    string insertSessionSql = @"
                        INSERT INTO sessioninfo (UserID, Token, ResetTime) 
                        VALUES (@UserID, @Token, @ResetTime)";

                    await connection.ExecuteAsync(insertSessionSql, new
                    {
                        UserID = user.UserID,
                        Token = token,
                        ResetTime = DateTime.Now
                    });

                    // Return user with token
                    user.Token = token;
                    return user;
                }
                catch (Exception ex)
                {
                    // Handle or log exception
                    return null;
                }
            }
        }

        public async Task<User> Register(User registerUser)
        {
            // Hash the password using BCrypt
            registerUser.Password = BCrypt.Net.BCrypt.HashPassword(registerUser.Password);

            // SQL query for inserting a new user
            string sql = @"
                INSERT INTO usermaster 
                (LoginId, UserName, MailId, MobileNo, Designation, CreatedBy, Password, IsActive) 
                VALUES 
                (@LoginId, @UserName, @MailId, @MobileNo, @Designation, @CreatedBy, @Password, @IsActive);
                SELECT LAST_INSERT_ID();"; // MySQL function to get the last auto-incremented ID

            using (var connection = context.CreateConnection())
            {
                try
                {
                    // Insert the user and get the generated UserID
                    var userId = await connection.QuerySingleAsync<int>(sql, new
                    {
                        registerUser.LoginId,
                        registerUser.UserName,
                        registerUser.MailId,
                        registerUser.MobileNo,
                        registerUser.Designation,
                        registerUser.CreatedBy,
                        registerUser.Password,
                        IsActive = true // Set the user as active by default
                    });

                    // Assign the generated UserID to the registered user
                    registerUser.UserID = userId;

                    return registerUser; // Return the newly registered user
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    throw new Exception("An error occurred while registering the user.", ex);
                }
            }
        }

        public async Task<bool> ValidateToken(string token)
        {
            using (var connection = context.CreateConnection())
            {
                var query = "SELECT COUNT(*) FROM sessioninfo WHERE Token = @Token AND ResetTime > @Now";
                var count = await connection.ExecuteScalarAsync<int>(query, new { Token = token, Now = DateTime.Now });

                return count > 0; // Valid token if found
            }
        }

        public async Task Logout(int userId)
        {
            using (var connection = context.CreateConnection())
            {
                var query = "DELETE FROM sessioninfo WHERE UserID = @UserID";
                await connection.ExecuteAsync(query, new { UserID = userId });
            }
        }

        private async Task<Role> GetUserRole(IDbConnection connection, int roleId)
        {
            string roleSql = "SELECT * FROM Roles WHERE RoleID = @RoleID";
            return await connection.QueryFirstOrDefaultAsync<Role>(roleSql, new { RoleID = roleId });
        }
    }
}
