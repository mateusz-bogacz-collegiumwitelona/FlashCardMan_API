using DTO.Request;
using DTO.Response;
using Services.Helpers;

namespace Services.Interfaces
{
    public interface IDeckServices
    {
        Task<ResultHandler<string>> CreateDeckAsync(AddNewDeckRequest request, string userEmail);
        Task<ResultHandler<List<GetDeckResponse>>> GetAllDecksAsync(string userEmail);
        Task<ResultHandler<string>> EditDeckAsync(EditDeckRequest request);
        Task<ResultHandler<string>> DeleteDeckAsync(string token);
        Task<ResultHandler<List<GetCardsForDeckResponse>>> GetDeckCardsAsync(string token);
    }
}
