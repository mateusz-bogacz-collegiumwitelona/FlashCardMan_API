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
        Task<bool> DeleteDeckAsync(Deck deck);
        Task<List<GetCardsForDeckResponse>> GetDeckCardsAsync(string token);
        Task<bool> IsDeckExist(string token);
    }
}
