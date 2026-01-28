using Services.Helpers;

namespace Services.Interfaces
{
    public interface ITagsServices
    {
        Task<ResultHandler<bool>> AddTagToTokenIfNew(string name, string cardToken);
    }
}
