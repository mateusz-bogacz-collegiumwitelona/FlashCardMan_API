using Data.Context;
using Data.Helpers;
using Data.Interfaces;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class TagsRepo : TokenCreator, ITagsRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public TagsRepo (ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddTagToTokenIfNew(string name, Guid cardId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                name = name.Trim().ToLower();

                var newTag = new Tags
                {
                    Id = Guid.NewGuid(),
                    Tag = name,
                    CreatedAt = DateTime.UtcNow,
                    Token = GenerateToken()
                };

                _dbContext.Tags.Add(newTag);

                _dbContext.FlashCardTag.Add(new FlashCardTag
                {
                    FlashCardId = cardId,
                    TagId = newTag.Id
                });

                var result = await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return result > 0;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> IsCardHaveThisTag(Guid cardId, string name)
            => await _dbContext.FlashCardTag
                .AnyAsync(ft => ft.FlashCardId == cardId &&
                                ft.Tag.Tag.ToLower() == name.Trim().ToLower());
    }
}
