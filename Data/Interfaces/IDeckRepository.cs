using Data.Models;
using DTO.Response;

namespace Data.Interfaces
{
    public interface IDeckRepository
    {
        Task<bool> AddNewDeckAsync(string name, string description);
        Task<List<GetDeckResponse>> GetAllDecksAsync();
        Task<Deck> FindDeckAsync(string token);
        Task<bool> EditDeckAsync(string token, string? name, string? description);
    }
}
