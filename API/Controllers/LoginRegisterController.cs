using DTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    [EnableCors("AllowClient")]
    [Route("api/")]
    [ApiController]
    public class LoginRegisterController : ControllerBase
    {
        private readonly ILoginRegisterServices _login;
        public LoginRegisterController(ILoginRegisterServices login)
        {
            _login = login;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> HandleLoginAsync(LoginRequest request)
        {
            var result = await _login.HandleLoginAsync(request);

            return result.IsSuccess
                ? StatusCode(result.StatusCode, new
                {
                    success = true,
                    message = result.Message,
                    data = result.Data
                })
                : StatusCode(result.StatusCode, new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> HandleLogoutAsync()
        {
            var result = await _login.HandleLogoutAsync();
            return result.IsSuccess
                ? StatusCode(result.StatusCode, new
                {
                    success = true,
                    message = result.Message
                })
                : StatusCode(result.StatusCode, new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
        }
    }
}
