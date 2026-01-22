using Data.Interfaces;
using DTO.Request;
using DTO.Response;
using Microsoft.AspNetCore.Http;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Services
{
    public class DeckServices : IDeckServices
    {
        private readonly IDeckRepository _deckRepo;

        public DeckServices(IDeckRepository deckRepo)
        {
            _deckRepo = deckRepo;
        }

        public async Task<ResultHandler<string>> CreateDeckAsync(AddNewDeckRequest request)
        {
            try
            {
                var result = await _deckRepo.AddNewDeckAsync(request.Name, request.Description);

                if (!result)
                {
                    return ResultHandler<string>.Failure(
                        "Failed to create deck.",
                        StatusCodes.Status500InternalServerError,
                        new List<string> { "CannotCreateDeck" });
                }

                return ResultHandler<string>.Success(
                    "Deck created successfully.",
                    StatusCodes.Status201Created,
                    null);
            }
            catch (Exception ex)
            {
                return ResultHandler<string>.Failure(
                    "An error occurred while creating the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResultHandler<List<GetDeckResponse>>> GetAllDecksAsync()
        {
            try
            {
                var decks = await _deckRepo.GetAllDecksAsync();

                if (decks == null || !decks.Any())
                {
                    return ResultHandler<List<GetDeckResponse>>.Failure(
                        "No decks found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "NoDecksAvailable" });
                }

                return ResultHandler<List<GetDeckResponse>>.Success(
                    "Decks retrieved successfully.",
                    StatusCodes.Status200OK,
                    decks);
            }
            catch (Exception ex)
            {
                return ResultHandler<List<GetDeckResponse>>.Failure(
                    "An error occurred while retrieving decks.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }
    }
}
