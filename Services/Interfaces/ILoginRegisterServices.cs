using DTO.Request;
using Services.Helpers;

namespace Services.Interfaces
{
    public interface ILoginRegisterServices
    {
        Task<ResultHandler<bool>> HandleLoginAsync(LoginRequest request);
    }
}
