using Data.Context;
using Data.Helpers;
using Data.Interfaces;
using Data.Models;
using DTO.Response;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Data.Repositories
{
    public class DeckRepository : TokenCreator, IDeckRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public DeckRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddNewDeckAsync(string name, string? description, ApplicationUser user)
        {
            var newDeck = new Deck
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description ?? null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Token = GenerateToken(),
                User = user
            };

            await _dbContext.Decks.AddAsync(newDeck);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<GetDeckResponse>> GetAllDecksAsync(Guid userId)
            => await _dbContext.Decks
                    .Include(d => d.User)
                    .Where(d => d.User.Id == userId)
                    .Select(d => new GetDeckResponse
                    {
                        Name = d.Name,
                        Description = d.Description ?? null,
                        DeckToken = d.Token
                    }).ToListAsync();


        public async Task<Deck> FindDeckAsync(string token)
            => await _dbContext.Decks.FirstOrDefaultAsync(d => d.Token == token);

        public async Task<bool> EditDeckAsync(string token, string? name, string? description)
        {
            var deck = await _dbContext.Decks.FirstOrDefaultAsync(d => d.Token == token);

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
                    .ThenInclude(fc => fc.FlashCardTags)
                        .ThenInclude(fct => fct.Tag)
                .SelectMany(d => d.FlashCards.Select(fc => new GetCardsForDeckResponse
                {
                    Question = fc.Question,
                    Answer = fc.Answer,
                    CreatedAt = fc.CreatedAt,
                    UpdatedAt = fc.UpdatedAt,
                    CardToken= fc.Token,
                    Tags = fc.FlashCardTags.Select(fct => new GetTagsResponse
                    {
                        Tag = fct.Tag.Tag,
                        TagToken = fct.Tag.Token
                    }).ToList()
                }))
            .ToListAsync();

        public async Task<bool> IsDeckExist(string token)
            => await _dbContext.Decks.AnyAsync(d => d.Token == token);

        public async Task<bool> IsHisDeck(Guid userId, string deckToken)
            => await _dbContext.Decks.AnyAsync(d => d.Token == deckToken && d.User.Id == userId);

        public async Task<List<GetCardsForDeckResponse>> GetDueCardForDeckAsync(string token)
             => await _dbContext.FlashCards
                .Include(fc => fc.Deck)
                .Include(fc => fc.FlashCardTags)
                    .ThenInclude(fct => fct.Tag)
                .Where(fc => fc.Deck.Token == token && fc.NextReviewAt <= DateTime.UtcNow)
                .OrderBy(fc => fc.NextReviewAt)
                .Select(fc => new GetCardsForDeckResponse
                {
                    Question = fc.Question,
                    Answer = fc.Answer,
                    CreatedAt = fc.CreatedAt,
                    UpdatedAt = fc.UpdatedAt,
                    CardToken = fc.Token,
                    Tags = fc.FlashCardTags.Select(fct => new GetTagsResponse
                    {
                        Tag = fct.Tag.Tag,
                        TagToken = fct.Tag.Token
                    }).ToList()
                })
                .ToListAsync();

        public async Task<GetDeckJsonResponse?> GetDeckToJsonAsync(string deckToken)
            => await _dbContext.Decks
                .Where(d => d.Token == deckToken)
                .Select(d => new GetDeckJsonResponse
                {
                    Name = d.Name,
                    Description = d.Description ?? $"{DateTime.UtcNow}",
                    Cards = d.FlashCards
                        .Select(fc => new GetCardJsonResponse
                        {
                            Question = fc.Question,
                            Answer = fc.Answer,
                            Tags = fc.FlashCardTags
                                        .Select(fct => fct.Tag.Tag)
                                        .ToList()
                        })
                        .ToList(),
                    
                })
                .FirstOrDefaultAsync();

        public async Task<bool> ImportDeckFromJson(GetDeckJsonResponse request, ApplicationUser user)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            
            try
            {
                Guid deckId = Guid.NewGuid();

                var newDeck = new Deck
                {
                    Id = deckId,
                    Name = request.Name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Description = request.Description ?? null,
                    User = user,
                    Token = GenerateToken()
                };

                await _dbContext.Decks.AddAsync(newDeck);

                var flashcards = request.Cards.Select(card => new FlashCards
                {
                    Id = Guid.NewGuid(),
                    Question = card.Question,
                    Answer = card.Answer,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeckId = deckId,
                    Token = GenerateToken(),
                    NextReviewAt = DateTime.UtcNow,
                    Repetitions = 0,
                    EasinessFactor = 2.5,
                    IntervalDays = 0
                });

                await _dbContext.FlashCards.AddRangeAsync(flashcards);

                var result = await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}