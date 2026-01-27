using Data.Context;
using Data.Helpers;
using Data.Interfaces;
using Data.Models;
using DTO.Request;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class FlashCardRepo : TokenCreator, IFlashCardRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public FlashCardRepo(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<string?> AddCardsToDeckAsync(Guid deckId, List<AddCardsRequest> requests)
        {
            var deck = await _dbContext.Decks.FindAsync(deckId);

            if (deck == null) return null;

            foreach (var request in requests)
            {
                var flashCard = new FlashCards
                {
                    Id = Guid.NewGuid(),
                    Question = request.Question,
                    Answer = request.Answer,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeckId = deckId,
                    Token = GenerateToken(),
                    NextReviewAt = DateTime.UtcNow,
                    Repetitions = 0,
                    EasinessFactor = 2.5,
                    IntervalDays = 0
                };

                await _dbContext.FlashCards.AddAsync(flashCard);
            }

            var result = await _dbContext.SaveChangesAsync();

            return result > 0 ? "Cards added successfully" : null;
        }

        public async Task<bool> IsCardExistAsync(string token)
            => await _dbContext.FlashCards.AnyAsync(fc => fc.Token == token);

        public async Task<bool> EditCardAsync(string cardToken, string? question, string? answare)
        {
            var card = await _dbContext.FlashCards.FirstOrDefaultAsync(fc => fc.Token == cardToken);

            if (card == null) return false;

            if (!string.IsNullOrEmpty(question)) card.Question = question;
            if (!string.IsNullOrEmpty(answare)) card.Question = answare;

            card.UpdatedAt = DateTime.UtcNow;

            _dbContext.FlashCards.Update(card);

            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteCardAsync(string token)
        {
            var card = await _dbContext.FlashCards.FirstOrDefaultAsync(fc => fc.Token == token);
            if (card == null) return false;
            _dbContext.FlashCards.Remove(card);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<FlashCards?> GetCardByTokenAsync(string token)
            => await _dbContext.FlashCards.FirstOrDefaultAsync(fc => fc.Token == token);

        public async Task<bool> UpdateCardAsync(FlashCards card)
        {
            _dbContext.FlashCards.Update(card);
            return await _dbContext.SaveChangesAsync() >0;
        }
    }
}
