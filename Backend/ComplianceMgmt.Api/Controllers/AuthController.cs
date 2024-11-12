using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ComplianceMgmt.Api.Controllers
{
    [Route("api/[controller]")]
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
            // Error checks

            if (String.IsNullOrEmpty(user.Name))
            {
                return BadRequest(new { message = "Name needs to entered" });
            }
            else if (String.IsNullOrEmpty(user.UserName))
            {
                return BadRequest(new { message = "User name needs to entered" });
            }
            else if (String.IsNullOrEmpty(user.Password))
            {
                return BadRequest(new { message = "Password needs to entered" });
            }

            // Try registration

            var registeredUser = await _authService.Register(new User
            {
                UserName = user.Name,
                Password = user.Password,
                MailId = user.MailId,
                LastLogin = DateTime.Now,
                MobileNo = user.PhoneNumber,
            });

            // Return responses

            if (registeredUser != null)
            {
                return Ok(registeredUser);
            }

            return BadRequest(new { message = "User registration unsuccessful" });
        }

        // POST: auth/refresh-token
        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            var user = await _authService.RefreshToken(tokenRequest.AccessToken, tokenRequest.RefreshToken);

            if (user != null)
            {
                return Ok(user);
            }

            return Unauthorized(new { message = "Invalid token or refresh token" });
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
