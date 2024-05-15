using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;

namespace deepdiveapi.Repositories.Interfaces
{
    /// <summary>
    /// The IExcursionRepository interface defines methods for managing excursions.
    /// </summary>
    public interface IExcursionRepository
    {
        public Task<List<Excursion>> GetExcursions();
        public Task<Excursion> GetExcursionById(string input);
        public Task<string> AddNewExcursion(NewExcursionDto input, string userId);
        public Task DeleteExcursion(string id);
        public Task UpdateExcursionAsync(Excursion excursion);
        public Task<(List<Excursion> PastExcursions, List<Excursion> FutureExcursions)> GetUserExcursions(string userId);

    }
}
