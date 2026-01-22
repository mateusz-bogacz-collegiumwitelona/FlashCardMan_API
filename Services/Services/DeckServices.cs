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

        public async Task<ResultHandler<string>> EditDeckAsync(EditDeckRequest request)
        {
            try
            {
                var isExist = await _deckRepo.FindDeckAsync(request.Token);

                if (isExist == null)
                {
                    return ResultHandler<string>.Failure(
                        "Deck not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "DeckNotFound" });
                }

                if (string.IsNullOrEmpty(request.Name) && string.IsNullOrEmpty(request.Description))
                {
                    return ResultHandler<string>.Failure(
                        "No fields to update.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "NoUpdateFields" });
                }

                if (request.Name == isExist.Name && request.Description == isExist.Description)
                {
                    return ResultHandler<string>.Failure(
                        "No changes detected.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "NoChangesDetected" });
                }

                var result = await _deckRepo.EditDeckAsync(request.Token, request.Name, request.Description);

                if (!result)
                {
                    return ResultHandler<string>.Failure(
                        "Failed to update deck.",
                        StatusCodes.Status500InternalServerError,
                        new List<string> { "CannotUpdateDeck" });
                }

                return ResultHandler<string>.Success(
                    "Deck updated successfully.",
                    StatusCodes.Status200OK,
                    null);
            }
            catch (Exception ex)
            {
                return ResultHandler<string>.Failure(
                    "An error occurred while updating the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResultHandler<string>> DeleteDeckAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return ResultHandler<string>.Failure(
                        "No token.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "NoToken" });
                }

                var isExist = await _deckRepo.FindDeckAsync(token);

                if (isExist == null)
                {
                    return ResultHandler<string>.Failure(
                        "Deck not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "DeckNotFound" });
                }

                var result = await _deckRepo.DeleteDeckAsync(isExist);

                if (!result)
                {
                    return ResultHandler<string>.Failure(
                        "Failed to delete deck.",
                        StatusCodes.Status500InternalServerError,
                        new List<string> { "CannotDeleteDeck" });
                }

                return ResultHandler<string>.Success(
                    "Deck deleted successfully.",
                    StatusCodes.Status200OK,
                    null);
            } 
            catch (Exception ex)
            {
                return ResultHandler<string>.Failure(
                    "An error occurred while updating the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }
    }
}
