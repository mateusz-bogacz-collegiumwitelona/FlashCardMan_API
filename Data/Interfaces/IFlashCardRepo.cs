using DTO.Request;

namespace Data.Interfaces
{
    public interface IFlashCardRepo
    {
        Task<string?> AddCardsToDeckAsync(Guid deckId, List<AddCardsRequest> requests);
        Task<bool> IsCardExistAsync(string token);
        Task<bool> EditCardAsync(string cardToken, string? question, string? answare);
    }
}
