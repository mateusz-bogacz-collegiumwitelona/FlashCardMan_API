using DTO.Request;
using Services.Helpers;

namespace Services.Interfaces
{
    public interface IFlashCardService
    {
        Task<ResultHandler<bool>> AddCardsToDeckAsync(string deckToken, List<AddCardsRequest> requests);
        Task<ResultHandler<bool>> EditCardAsync(string cardToken, string? question, string? answare);
    }
}
