using DTO.Response;

namespace Data.Interfaces
{
    public interface IDeckRepository
    {
        Task<bool> AddNewDeckAsync(string name, string description);
        Task<List<GetDeckResponse>> GetAllDecksAsync();
    }
}
