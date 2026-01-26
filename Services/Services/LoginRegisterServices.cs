using Data.Models;
using DTO.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Services
{
    public class LoginRegisterServices : ILoginRegisterServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public LoginRegisterServices (
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<ResultHandler<bool>> HandleLoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Login);

                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(request.Login);

                    if (user == null)
                    {
                        return ResultHandler<bool>.Failure(
                            $"Can't find user with this email or login: {request.Login}",
                            StatusCodes.Status404NotFound);
                    }
                }


                if (!user.EmailConfirmed)
                {
                    return ResultHandler<bool>.Failure(
                        "Please confirm your email address before logging in. Check your inbox for the confirmation link.",
                        StatusCodes.Status403Forbidden
                        );
                }

                var roles = await _userManager.GetRolesAsync(user);

                if (roles == null || !roles.Any())
                {
                    return ResultHandler<bool>.Failure(
                        "User has no role assigned",
                        StatusCodes.Status403Forbidden
                        );
                }

                var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password,
                    false
                    );

                if (!result.Succeeded)
                {
                    return ResultHandler<bool>.Failure(
                        "Invalid login attempt",
                        StatusCodes.Status401Unauthorized
                        );
                }

                return ResultHandler<bool>.Success(
                    "Login successful",
                    StatusCodes.Status200OK,
                    true
                    );
            }
            catch (Exception ex)
            {
                return ResultHandler<bool>.Failure(
                    "An error occurred while adding cards to the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }
    }
}
