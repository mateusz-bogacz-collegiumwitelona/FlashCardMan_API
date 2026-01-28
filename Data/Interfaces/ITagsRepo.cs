namespace Data.Interfaces
{
    public interface ITagsRepo
    {
        Task<bool> AddNewTagAsync(string name);
    }
}
