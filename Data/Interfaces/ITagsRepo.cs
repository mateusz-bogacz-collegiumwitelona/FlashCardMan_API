namespace Data.Interfaces
{
    public interface ITagsRepo
    {
        Task<bool> AddTagToTokenIfNew(string name, Guid cardId);
        Task<bool> IsCardHaveThisTag(Guid cardId, string name);
    }
}
