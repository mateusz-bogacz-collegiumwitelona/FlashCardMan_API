using Data.Context;
using Data.Helpers;
using Data.Interfaces;
using Data.Models;

namespace Data.Repositories
{
    public class TagsRepo : TokenCreator, ITagsRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public TagsRepo (ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddNewTagAsync(string name)
        {
            var newTag = new Tags
            {
                Id = Guid.NewGuid(),
                Tag = name,
                CreatedAt = DateTime.UtcNow,
                Token = GenerateToken()
            };

            await _dbContext.Tags.AddAsync(newTag);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
