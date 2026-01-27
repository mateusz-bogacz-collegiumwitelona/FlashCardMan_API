using Data.Interfaces;
using Data.Models;
using DTO.Request;
using DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NpgsqlTypes;
using Services.Helpers;
using Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Services.Services
{
    public class DeckServices : IDeckServices
    {
        private readonly IDeckRepository _deckRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeckServices(IDeckRepository deckRepo, UserManager<ApplicationUser> userManager)
        {
            _deckRepo = deckRepo;
            _userManager = userManager;
        }

        public async Task<ResultHandler<string>> CreateDeckAsync(AddNewDeckRequest request, string userEmail)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user == null)
                {
                    return ResultHandler<string>.Failure(
                        "User not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "UserNotFound" });
                }

                var result = await _deckRepo.AddNewDeckAsync(request.Name, request.Description, user);

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

        public async Task<ResultHandler<List<GetDeckResponse>>> GetAllDecksAsync(string userEmail)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    return ResultHandler<List<GetDeckResponse>>.Failure(
                        "User not foud",
                        StatusCodes.Status404NotFound
                        );
                }

                var decks = await _deckRepo.GetAllDecksAsync(user.Id);

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

        public async Task<ResultHandler<bool>> EditDeckAsync(EditDeckRequest request, string userEmail)
        {
            try
            {
                var isExist = await _deckRepo.IsDeckExist(request.Token);

                if (!isExist)
                {
                    return ResultHandler<bool>.Failure(
                        "Deck not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "DeckNotFound" });
                }

                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user == null)
                {
                    return ResultHandler<bool>.Failure(
                        "User not foud",
                        StatusCodes.Status404NotFound
                        );
                }

                bool isHisDeck = await _deckRepo.IsHisDeck(user.Id, request.Token);

                if (!isHisDeck)
                {
                    return ResultHandler<bool>.Failure(
                        "User is no authorize to interact with this deck",
                        StatusCodes.Status401Unauthorized
                        );
                }

                if (string.IsNullOrEmpty(request.Name) && string.IsNullOrEmpty(request.Description))
                {
                    return ResultHandler<bool>.Failure(
                        "No fields to update.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "NoUpdateFields" });
                }

                var deck = await _deckRepo.FindDeckAsync(request.Token);

                if (request.Name == deck.Name && request.Description == deck.Description)
                {
                    return ResultHandler<bool>.Failure(
                        "No changes detected.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "NoChangesDetected" });
                }

                var result = await _deckRepo.EditDeckAsync(request.Token, request.Name, request.Description);

                if (!result)
                {
                    return ResultHandler<bool>.Failure(
                        "Failed to update deck.",
                        StatusCodes.Status500InternalServerError,
                        new List<string> { "CannotUpdateDeck" });
                }

                return ResultHandler<bool>.Success(
                    "Deck updated successfully.",
                    StatusCodes.Status200OK,
                    true);
            }
            catch (Exception ex)
            {
                return ResultHandler<bool>.Failure(
                    "An error occurred while updating the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResultHandler<bool>> DeleteDeckAsync(string token, string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return ResultHandler<bool>.Failure(
                        "No token.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "NoToken" });
                }

                var isExist = await _deckRepo.FindDeckAsync(token);

                if (isExist == null)
                {
                    return ResultHandler<bool>.Failure(
                        "Deck not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "DeckNotFound" });
                }

                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user == null)
                {
                    return ResultHandler<bool>.Failure(
                        "User not foud",
                        StatusCodes.Status404NotFound
                        );
                }

                bool isHisDeck = await _deckRepo.IsHisDeck(user.Id, token);

                if (!isHisDeck)
                {
                    return ResultHandler<bool>.Failure(
                        "User is no authorize to interact with this deck",
                        StatusCodes.Status401Unauthorized
                        );
                }

                var result = await _deckRepo.DeleteDeckAsync(isExist);

                if (!result)
                {
                    return ResultHandler<bool>.Failure(
                        "Failed to delete deck.",
                        StatusCodes.Status500InternalServerError,
                        new List<string> { "CannotDeleteDeck" });
                }

                return ResultHandler<bool>.Success(
                    "Deck deleted successfully.",
                    StatusCodes.Status200OK,
                    true);
            }
            catch (Exception ex)
            {
                return ResultHandler<bool>.Failure(
                    "An error occurred while updating the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResultHandler<List<GetCardsForDeckResponse>>> GetDeckCardsAsync(string token, string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                        "No token.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "NoToken" });
                }

                bool isDeckExist = await _deckRepo.IsDeckExist(token);

                if (!isDeckExist)
                {
                    return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                        "Deck not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "DeckNotFound" });
                }

                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user == null)
                {
                    return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                        "User not foud",
                        StatusCodes.Status404NotFound
                        );
                }

                bool isHisDeck = await _deckRepo.IsHisDeck(user.Id, token);

                if (!isHisDeck)
                {
                    return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                        "User is no authorize to interact with this deck",
                        StatusCodes.Status401Unauthorized
                        );
                }

                var result = await _deckRepo.GetDeckCardsAsync(token);

                if (result == null || !result.Any())
                {
                    return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                        "Cards in deck not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "CardsNotFound" });
                }

                return ResultHandler<List<GetCardsForDeckResponse>>.Success(
                    "Carnd in deck reviewed successfuly.",
                    StatusCodes.Status200OK,
                    result);
            }
            catch (Exception ex)
            {
                return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                    "An error occurred while updating the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResultHandler<List<GetCardsForDeckResponse>>> GetDueCardForDeckAsync(string token, string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                        "No token.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "NoToken" });
                }

                bool isDeckExist = await _deckRepo.IsDeckExist(token);

                if (!isDeckExist)
                {
                    return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                        "Deck not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "DeckNotFound" });
                }

                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user == null)
                {
                    return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                        "User not foud",
                        StatusCodes.Status404NotFound
                        );
                }

                bool isHisDeck = await _deckRepo.IsHisDeck(user.Id, token);

                if (!isHisDeck)
                {
                    return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                        "User is no authorize to interact with this deck",
                        StatusCodes.Status401Unauthorized
                        );
                }

                var result = await _deckRepo.GetDueCardForDeckAsync(token);

                if (result == null || !result.Any())
                {
                    return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                        "Cards in deck not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "CardsNotFound" });
                }

                return ResultHandler<List<GetCardsForDeckResponse>>.Success(
                    "Carnd in deck reviewed successfuly.",
                    StatusCodes.Status200OK,
                    result);
            }
            catch (Exception ex)
            {
                return ResultHandler<List<GetCardsForDeckResponse>>.Failure(
                    "An error occurred while updating the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResultHandler<FileContentResult>> GetDeckToJsonAsync(string deckToken)
        {
            try
            {
                var isExist = await _deckRepo.IsDeckExist(deckToken);

                if (!isExist)
                {
                    return ResultHandler<FileContentResult>.Failure(
                        "Deck not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "DeckNotFound" });
                }

                var data = await _deckRepo.GetDeckToJsonAsync(deckToken);

                if (data == null)
                {
                    return ResultHandler<FileContentResult>.Failure(
                        "Failed to export deck.",
                        StatusCodes.Status500InternalServerError,
                        new List<string> { "CannotExportDeck" });
                }

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var file = new FileContentResult(
                    Encoding.UTF8.GetBytes(json),
                    "application/json")
                {
                    FileDownloadName = $"{data.Name}.json"
                };

                return ResultHandler<FileContentResult>.Success(
                    "Deck exported successfully.",
                    StatusCodes.Status200OK,
                    file);
            }
            catch (Exception ex)
            {
                return ResultHandler<FileContentResult>.Failure(
                    "An error occurred while exporting the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResultHandler<bool>> ImportDeckFromJson(IFormFile file, string userEmail)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ResultHandler<bool>.Failure(
                        "Invalid JSON file.",
                        StatusCodes.Status400BadRequest
                        );

                using var stream = file.OpenReadStream();

                var request = await JsonSerializer.DeserializeAsync<GetDeckJsonResponse>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                    return ResultHandler<bool>.Failure(
                        "Invalid JSON file.", 
                        StatusCodes.Status400BadRequest
                        );

                if (string.IsNullOrWhiteSpace(request.Name))
                    return ResultHandler<bool>.Failure(
                        "Deck name is required.", 
                        StatusCodes.Status400BadRequest
                        );

                if (request.Cards == null || !request.Cards.Any())
                    return ResultHandler<bool>.Failure(
                        "Deck must contain at least one card.", 
                        StatusCodes.Status400BadRequest
                        );

                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user == null)
                    return ResultHandler<bool>.Failure(
                        "User not foud",
                        StatusCodes.Status404NotFound
                        );

                var result = await _deckRepo.ImportDeckFromJson(request, user);

                if (!result)
                    return ResultHandler<bool>.Failure(
                        "Failed to import deck.",
                        StatusCodes.Status500InternalServerError,
                        new List<string> { "CannotImportDeck" });

                return ResultHandler<bool>.Success(
                    "Deck import successfuly.",
                    StatusCodes.Status200OK,
                    result);
            }
            catch (JsonException jex)
            {
                return ResultHandler<bool>.Failure(
                    "An error occurred while importing the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { jex.Message });
            }
            catch (Exception ex)
            {
                return ResultHandler<bool>.Failure(
                    "An error occurred while importing the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }
    }
}
