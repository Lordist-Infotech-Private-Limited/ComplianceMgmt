using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthRepository authService) : ControllerBase
    {
        private readonly IAuthRepository _authService = authService;

        // POST: auth/login
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginUser user)
        {
            var loggedInUser = await _authService.Login(new LoginUser
            {
                MailId = user.MailId,
                Password = user.Password,
            });

            if (loggedInUser != null)
            {
                return Ok(loggedInUser);
            }

            return BadRequest(new { message = "User login unsuccessful" });
        }

        // POST: auth/register
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser user)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(user.Name))
            {
                return BadRequest(new { message = "Name is required." });
            }
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                return BadRequest(new { message = "Username is required." });
            }
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest(new { message = "Password is required." });
            }
            if (string.IsNullOrWhiteSpace(user.MailId) || !user.MailId.Contains("@"))
            {
                return BadRequest(new { message = "A valid email is required." });
            }

            try
            {
                // Call the repository or service to register the user
                var registeredUser = await _authService.Register(new User
                {
                    UserName = user.UserName,
                    Password = user.Password,
                    MailId = user.MailId,
                    MobileNo = user.PhoneNumber,
                    LoginId = user.LoginId,
                    CreateDate = DateTime.UtcNow, // Ensure consistent timestamps
                    IsActive = true // Default active status for new users
                });

                // If registration is successful, return the registered user's details (excluding sensitive data)
                if (registeredUser != null)
                {
                    return Ok(new
                    {
                        registeredUser.UserID,
                        registeredUser.UserName,
                        registeredUser.MailId,
                        registeredUser.MobileNo,
                        registeredUser.IsActive
                    });
                }

                return BadRequest(new { message = "User registration failed. Please try again." });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                //_logger.LogError(ex, "Error occurred during user registration.");

                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during registration. Please contact support." });
            }
        }

        // GET: auth/test
        [Authorize()]
        [HttpGet]
        public IActionResult Test()
        {
            // Get token from header

            string token = Request.Headers["Authorization"];

            if (token.StartsWith("Bearer"))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }
            var handler = new JwtSecurityTokenHandler();

            // Returns all claims present in the token

            JwtSecurityToken jwt = handler.ReadJwtToken(token);

            var claims = "List of Claims: \n\n";

            foreach (var claim in jwt.Claims)
            {
                claims += $"{claim.Type}: {claim.Value}\n";
            }

            return Ok(claims);
        }
    }
    public class TokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

}
