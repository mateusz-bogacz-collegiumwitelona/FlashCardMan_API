using Data.Interfaces;
using Data.Models;
using DTO.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Services
{
    public class FlashCardService : IFlashCardService
    {
        private readonly IFlashCardRepo _flashCardRepo;
        private readonly IDeckRepository _deckRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public FlashCardService(
            IFlashCardRepo flashCardRepo, 
            IDeckRepository deckRepo,
            UserManager<ApplicationUser> userManager)
        {
            _flashCardRepo = flashCardRepo;
            _deckRepo = deckRepo;
            _userManager = userManager;
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

        public async Task<ResultHandler<bool>> EditCardAsync(string cardToken, string? question, string? answare)
        {
            try
            {
                bool isCardExist = await _flashCardRepo.IsCardExistAsync(cardToken);

                if (!isCardExist)
                {
                    return ResultHandler<bool>.Failure(
                        "Card not found",
                        StatusCodes.Status404NotFound,
                        new List<string> { "CardNotFound" } 
                        );
                }

                if (
                    (string.IsNullOrEmpty(question) && string.IsNullOrEmpty(answare)) || 
                    (string.IsNullOrWhiteSpace(question) && string.IsNullOrWhiteSpace(answare))
                    )
                {
                    return ResultHandler<bool>.Failure(
                        "No fields to update.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "NoUpdateFields" });
                }

                bool result = await _flashCardRepo.EditCardAsync(cardToken, question, answare);

                if (!result)
                {
                    return ResultHandler<bool>.Failure(
                       "Failed to edit card to the deck.",
                       StatusCodes.Status500InternalServerError,
                       new List<string> { "EditCardsFailed" });
                }

                return ResultHandler<bool>.Success(
                    "Card edit successfully.",
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

        public async Task<ResultHandler<bool>> DeleteCardAsync(string cardToken)
        {
            try
            {
                bool isCardExist = await _flashCardRepo.IsCardExistAsync(cardToken);

                if (!isCardExist)
                {
                    return ResultHandler<bool>.Failure(
                        "Card not found",
                        StatusCodes.Status404NotFound,
                        new List<string> { "CardNotFound" } 
                        );
                }

                bool result = await _flashCardRepo.DeleteCardAsync(cardToken);

                if (!result)
                {
                    return ResultHandler<bool>.Failure(
                       "Failed to delete card from the deck.",
                       StatusCodes.Status500InternalServerError,
                       new List<string> { "DeleteCardFailed" });
                }

                return ResultHandler<bool>.Success(
                    "Card deleted successfully.",
                    StatusCodes.Status200OK,
                    true);
            }
            catch (Exception ex)
            {
                return ResultHandler<bool>.Failure(
                    "An error occurred while deleting the card from the deck.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }

       public async Task<ResultHandler<bool>> ReviewCardAsync(ReviewCardRequest request, string userEmail)
        {
            try
            {
                var card = await _flashCardRepo.GetCardByTokenAsync(request.CardToken);

                if (card == null)
                {
                    return ResultHandler<bool>.Failure(
                        "Card not found",
                        StatusCodes.Status404NotFound,
                        new List<string> { "CardNotFound" }
                        );
                }

                var sm2Result = Sm2Calc.CalculatNewState(
                    request.Grade,
                    card.Repetitions,
                    card.EasinessFactor,
                    card.IntervalDays
                    );

                card.Repetitions = sm2Result.Repetitions;
                card.EasinessFactor = sm2Result.EasinessFactor;
                card.IntervalDays = sm2Result.IntervalDays;
                card.NextReviewAt = sm2Result.NextReviewAt;
                card.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _flashCardRepo.UpdateCardAsync(card);

                if (!updateResult)
                {
                    return ResultHandler<bool>.Failure(
                       "Failed to review the card.",
                       StatusCodes.Status500InternalServerError,
                       new List<string> { "ReviewCardFailed" });
                }

                return ResultHandler<bool>.Success(
                    "Card reviewed successfully.",
                    StatusCodes.Status200OK,
                    true);
            }
            catch (Exception ex)
            {
                return ResultHandler<bool>.Failure(
                    "An error occurred while reviewing the card.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }
    }
}
