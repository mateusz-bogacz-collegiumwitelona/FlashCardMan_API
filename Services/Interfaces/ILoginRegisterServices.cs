using DTO.Request;
using DTO.Response;
using Microsoft.AspNetCore.Identity;
using Services.Helpers;

namespace Services.Interfaces
{
    public interface ILoginRegisterServices
    {
        Task<ResultHandler<LoginResponse>> HandleLoginAsync(LoginRequest request);
        Task<ResultHandler<IdentityResult>> HandleLogoutAsync();
        Task<ResultHandler<IdentityResult>> RegisterUserAsync(RegisterRequest request);
        Task<ResultHandler<IdentityResult>> ConfirmEmailAsync(ConfirmEmailRequest request);
        Task<ResultHandler<IdentityResult>> ForgotPasswordAsync(string email);
        Task<ResultHandler<IdentityResult>> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
