using Data.Context;
using Data.Helpers;
using Data.Interfaces;
using Data.Models;
using DTO.Request;

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
                    Token = GenerateToken()
                };

                await _dbContext.FlashCards.AddAsync(flashCard);
            }

            var result = await _dbContext.SaveChangesAsync();

            return result > 0 ? "Cards added successfully" : null;
        }
    }
}
