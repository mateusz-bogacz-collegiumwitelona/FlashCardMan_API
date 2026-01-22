using DTO.Request;
using DTO.Response;
using Services.Helpers;

namespace Services.Interfaces
{
    public interface IDeckServices
    {
        Task<ResultHandler<string>> CreateDeckAsync(AddNewDeckRequest request);
        Task<ResultHandler<List<GetDeckResponse>>> GetAllDecksAsync();
    }
}
