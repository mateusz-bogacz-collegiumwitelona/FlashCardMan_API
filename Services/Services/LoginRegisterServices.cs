using Data.Interfaces;
using Data.Models;
using DTO.Request;
using DTO.Response;
using Events.Event;
using Events.Interfaces;
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
        private readonly IEventDispatcher _eventDispatcher;

        public LoginRegisterServices(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IHttpContextAccessor httpContext,
            IConfiguration config,
            IRefreshTokenRepository refreshTokenRepository,
            IEventDispatcher eventDispatcher
            ) : base(config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpContext = httpContext;
            _refreshTokenRepository = refreshTokenRepository;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<ResultHandler<LoginResponse>> HandleLoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Login);

                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(request.Login);

                    if (user == null)
                    {
                        return ResultHandler<LoginResponse>.Failure(
                            $"Can't find user with this email or login: {request.Login}",
                            StatusCodes.Status404NotFound);
                    }
                }


                if (!user.EmailConfirmed)
                {
                    return ResultHandler<LoginResponse>.Failure(
                        "Please confirm your email address before logging in. Check your inbox for the confirmation link.",
                        StatusCodes.Status403Forbidden
                        );
                }

                var roles = await _userManager.GetRolesAsync(user);

                if (roles == null || !roles.Any())
                {
                    return ResultHandler<LoginResponse>.Failure(
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
                    return ResultHandler<LoginResponse>.Failure(
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

                var response = new LoginResponse
                {
                    Message = "Login successful",
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles.ToList()
                };

                return ResultHandler<LoginResponse>.Success(
                    "Login successful",
                    StatusCodes.Status200OK,
                    response
                    );
            }
            catch (Exception ex)
            {
                return ResultHandler<LoginResponse>.Failure(
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

        public async Task<ResultHandler<IdentityResult>> HandleLogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();

                var context = _httpContext.HttpContext;

                var isDev = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !isDev,
                    SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.None,
                    Path = "/"
                };

                context.Response.Cookies.Delete("jwt", cookieOptions);
                context.Response.Cookies.Delete("refresh_token", cookieOptions);

              

                return ResultHandler<IdentityResult>.Success("Logout successful.", StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                return ResultHandler<IdentityResult>.Failure(
                    "An error occurred during logout.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message }
                    );
            }
        }

        public async Task<ResultHandler<IdentityResult>> RegisterUserAsync(RegisterRequest request)
        {
            try
            {
                var isEmailTaken = await _userManager.FindByEmailAsync(request.Email);

                if (isEmailTaken != null)
                    return ResultHandler<IdentityResult>.Failure(
                        "Email is already taken.",
                        StatusCodes.Status400BadRequest
                        );

                var isUserNameTaken = await _userManager.FindByNameAsync(request.UserName);
                if (isUserNameTaken != null)
                    return ResultHandler<IdentityResult>.Failure(
                        "User name is already taken.",
                        StatusCodes.Status400BadRequest
                        );

                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = request.UserName,
                    NormalizedUserName = request.UserName.ToUpper(),
                    Email = request.Email,
                    NormalizedEmail = request.Email.ToUpper(),
                    EmailConfirmed = false,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createUser = await _userManager.CreateAsync(user, request.Password);

                if (!createUser.Succeeded)
                    return ResultHandler<IdentityResult>.Failure(
                        "User creation failed.",
                        StatusCodes.Status500InternalServerError,
                        createUser.Errors.Select(e => e.Description).ToList()
                        );

                string defaultRole = "User";

                if (!await _roleManager.RoleExistsAsync(defaultRole))
                {
                    return ResultHandler<IdentityResult>.Failure(
                        $"Default role '{defaultRole}' does not exist.",
                        StatusCodes.Status500InternalServerError
                    );
                }

                var addToRole = await _userManager.AddToRoleAsync(user, defaultRole);

                if (!addToRole.Succeeded)
                    return ResultHandler<IdentityResult>.Failure(
                        "Assigning role to user failed.",
                        StatusCodes.Status500InternalServerError,
                        addToRole.Errors.Select(e => e.Description).ToList()
                        );

                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                if (string.IsNullOrEmpty(confirmationToken))
                {
                    return ResultHandler<IdentityResult>.Failure(
                        "Failed to generate email confirmation token.",
                        StatusCodes.Status500InternalServerError
                    );
                }

                await _eventDispatcher.PublishAsync(new UserRegisteredEvent(user, confirmationToken));

                return ResultHandler<IdentityResult>.Success(
                    "User registered successfully. Please check your email to confirm your account.",
                    StatusCodes.Status201Created
                    );
            }
            catch (Exception ex)
            {
                return ResultHandler<IdentityResult>.Failure(
                    "An error occurred during user registration.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message }
                    );
            }
        }

        public async Task<ResultHandler<IdentityResult>> ConfirmEmailAsync(ConfirmEmailRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    return ResultHandler<IdentityResult>.Failure(
                        "User not found.",
                        StatusCodes.Status404NotFound
                        );
                }

                if (user.EmailConfirmed)
                {
                    return ResultHandler<IdentityResult>.Failure(
                        "Email is already confirmed.",
                        StatusCodes.Status400BadRequest
                        );
                }

                var result = await _userManager.ConfirmEmailAsync(user, request.Token);

                if (!result.Succeeded)
                {
                    return ResultHandler<IdentityResult>.Failure(
                        "Email confirmation failed.",
                        StatusCodes.Status500InternalServerError,
                        result.Errors.Select(e => e.Description).ToList()
                        );
                }

                return ResultHandler<IdentityResult>.Success(
                    "Email confirmed successfully.",
                    StatusCodes.Status200OK
                    );
            }
            catch (Exception ex)
            {
                return ResultHandler<IdentityResult>.Failure(
                    "An error occurred during email confirmation.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message }
                    );
            }
        }
    }
}
