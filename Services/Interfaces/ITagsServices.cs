using Services.Helpers;

namespace Services.Interfaces
{
    public interface ITagsServices
    {
        Task<ResultHandler<bool>> AddNewTagAsync(string name);
    }
}
