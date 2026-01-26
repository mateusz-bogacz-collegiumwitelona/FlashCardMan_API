using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Helpers;
using System.Security.Claims;

namespace API.Controllers
{
    public abstract class AuthControllerBase : ControllerBase
    {
        protected string? userEmail => User.FindFirst(ClaimTypes.Email)?.Value;

        protected (string email, IActionResult? error) GetAuthenticatedUser()
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                var errorResult = ResultHandler<IdentityResult>.Failure(
                    "User is not authenticated.",
                    StatusCodes.Status401Unauthorized,
                    new List<string> { "UnAuthenticated" });

                return (string.Empty, StatusCode(errorResult.StatusCode, new
                {
                    success = false,
                    message = errorResult.Message,
                    errors = errorResult.Errors
                }));
            }

            return (userEmail!, null);
        }
    }
}
