using Data.Context;
using Data.Helpers;
using Data.Interfaces;
using Data.Models;
using DTO.Response;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class DeckRepository : TokenCreator, IDeckRepository 
    {
        private readonly ApplicationDbContext _dbContext;
        public DeckRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddNewDeckAsync(string name, string description, ApplicationUser user)
        {
            var newDeck = new Deck
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Token = GenerateToken(),
                User = user
            };

            await _dbContext.Decks.AddAsync(newDeck);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<GetDeckResponse>> GetAllDecksAsync()
        {
            var decks = await _dbContext.Decks.ToListAsync();

            return decks.Select(deck => new GetDeckResponse
            {
                Name = deck.Name,
                Description = deck.Description,
                Token = deck.Token
            }).ToList();
        }

        public async Task<Deck> FindDeckAsync(string token)
            => await _dbContext.Decks.FirstOrDefaultAsync(d => d.Token == token);
        
        public async Task<bool> EditDeckAsync(string token, string? name, string? description)
        {
            var deck =  await _dbContext.Decks.FirstOrDefaultAsync(d => d.Token == token);

            if (!string.IsNullOrEmpty(name))
                deck.Name = name;

            if (!string.IsNullOrEmpty(description))
                deck.Description = description;

            deck.UpdatedAt = DateTime.UtcNow;

            _dbContext.Decks.Update(deck);

            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteDeckAsync(Deck deck)
        {
            _dbContext.Decks.Remove(deck);

            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<List<GetCardsForDeckResponse>> GetDeckCardsAsync(string token)
            => await _dbContext.Decks
                .Where(d => d.Token == token)
                .Include(d => d.FlashCards)
                .SelectMany(d => d.FlashCards.Select(fc => new GetCardsForDeckResponse
                {
                    Question = fc.Question,
                    Answer = fc.Answer,
                    CreatedAt = fc.CreatedAt,
                    UpdatedAt = fc.UpdatedAt,
                    Token = fc.Token
                }))
            .ToListAsync();

        public async Task<bool> IsDeckExist(string token)
            => await _dbContext.Decks.AnyAsync(d => d.Token == token);
    }
}
