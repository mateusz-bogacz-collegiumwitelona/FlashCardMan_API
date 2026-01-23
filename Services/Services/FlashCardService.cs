using Data.Interfaces;
using DTO.Request;
using Microsoft.AspNetCore.Http;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Services
{
    public class FlashCardService : IFlashCardService
    {
        private readonly IFlashCardRepo _flashCardRepo;
        private readonly IDeckRepository _deckRepo;

        public FlashCardService(IFlashCardRepo flashCardRepo, IDeckRepository deckRepo)
        {
            _flashCardRepo = flashCardRepo;
            _deckRepo = deckRepo;
        }

        public async Task<ResultHandler<bool>> AddCardsToDeckAsync(string deckToken, List<AddCardsRequest> requests)
        {
            try
            {
                var idExist = await _deckRepo.FindDeckAsync(deckToken);

                if (idExist == null)
                {
                    return ResultHandler<bool>.Failure(
                        "Deck not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "DeckNotFound" });
                }

                if (requests == null || !requests.Any())
                {
                    return ResultHandler<bool>.Failure(
                        "No cards to add.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "NoCardsProvided" });
                }

                var result = await _flashCardRepo.AddCardsToDeckAsync(idExist.Id, requests);

                if (string.IsNullOrEmpty(result))
                {
                    return ResultHandler<bool>.Failure(
                        "Failed to add cards to the deck.",
                        StatusCodes.Status500InternalServerError,
                        new List<string> { "AddCardsFailed" });
                }

                return ResultHandler<bool>.Success(
                    "Cards added to the deck successfully.",
                    StatusCodes.Status200OK,
                    true);
            }
            catch (Exception ex)
            {
                return ResultHandler<bool>.Failure(
                    "An error occurred while adding cards to the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }
    }
}
