using Data.Interfaces;
using Data.Models;
using DTO.Request;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Helpers;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace Services.Services
{
    public class LoginRegisterServices : TokenFactory, ILoginRegisterServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LoginRegisterServices(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IHttpContextAccessor httpContext,
            IConfiguration config,
            IRefreshTokenRepository refreshTokenRepository
            ) : base(config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpContext = httpContext;
            _refreshTokenRepository = refreshTokenRepository;
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

                var jwtToken = CreateJwtToken(user, roles);
                string tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                var context = _httpContext.HttpContext;
                var refreshToken = CreateRefreshToken(user.Id,
                    context?.Connection.RemoteIpAddress?.ToString(),
                    context?.Request.Headers["User-Agent"].ToString()
                    );

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                SetAuthCookie(context, tokenString, jwtToken.ValidTo, refreshToken);

                context.Response.Headers.Append("X-Token-Expiry", jwtToken.ValidTo.ToString("o"));

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

        private void SetAuthCookie(HttpContext context, string token, DateTime expiry, RefreshToken refresh)
        {
            var isDev = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDev,
                SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.None,
                Path = "/"
            };

            context.Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = cookieOptions.HttpOnly,
                Secure = cookieOptions.Secure,
                SameSite = cookieOptions.SameSite,
                Expires = expiry,
                Path = cookieOptions.Path
            });

            context.Response.Cookies.Append("refresh_token", refresh.Token, new CookieOptions
            {
                HttpOnly = cookieOptions.HttpOnly,
                Secure = cookieOptions.Secure,
                SameSite = cookieOptions.SameSite,
                Expires = refresh.ExpiryDate,
                Path = cookieOptions.Path
            });
        }
    }
}
