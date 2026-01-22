using Data.Context;
using Data.Helpers;
using Data.Interfaces;

namespace Data.Repositories
{
    public class DeckRepository : TokenCreator, IDeckRepository 
    {
        private readonly ApplicationDbContext _dbContext;
        public DeckRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddNewDeckAsync(string name, string description)
        {
            var newDeck = new Models.Deck
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Token = GenerateToken()
            };

            await _dbContext.Decks.AddAsync(newDeck);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }
    }
}
