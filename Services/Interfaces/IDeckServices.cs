using DTO.Request;
using DTO.Response;
using Services.Helpers;

namespace Services.Interfaces
{
    public interface IDeckServices
    {
        Task<ResultHandler<string>> CreateDeckAsync(AddNewDeckRequest request, string userEmail);
        Task<ResultHandler<List<GetDeckResponse>>> GetAllDecksAsync(string userEmail);
        Task<ResultHandler<bool>> EditDeckAsync(EditDeckRequest request, string userEmail);
        Task<ResultHandler<bool>> DeleteDeckAsync(string token, string userEmail);
        Task<ResultHandler<List<GetCardsForDeckResponse>>> GetDeckCardsAsync(string token, string userEmail);
    }
}
