namespace Data.Interfaces
{
    public interface IDeckRepository
    {
        Task<bool> AddNewDeckAsync(string name, string description);
    }
}
