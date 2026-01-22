using DTO.Request;
using Services.Helpers;

namespace Services.Interfaces
{
    public interface IDeckServices
    {
        Task<ResultHandler<string>> CreateDeckAsync(AddNewDeckRequest request);
    }
}
