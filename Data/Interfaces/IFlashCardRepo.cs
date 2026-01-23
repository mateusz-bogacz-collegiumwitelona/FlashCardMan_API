using DTO.Request;

namespace Data.Interfaces
{
    public interface IFlashCardRepo
    {
        Task<string?> AddCardsToDeckAsync(Guid deckId, List<AddCardsRequest> requests);
    }
}
