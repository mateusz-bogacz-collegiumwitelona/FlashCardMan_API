using DTO.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class LoginRegisterController : ControllerBase
    {
        private readonly ILoginRegisterServices _loginRegisterServices;
        public LoginRegisterController(ILoginRegisterServices loginRegisterServices)
        {
            _loginRegisterServices = loginRegisterServices;
        }

        [HttpPost("login")]
        public async Task<IActionResult> HandleLoginAsync(LoginRequest request)
        {
            var result = await _loginRegisterServices.HandleLoginAsync(request);

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
    }
}
