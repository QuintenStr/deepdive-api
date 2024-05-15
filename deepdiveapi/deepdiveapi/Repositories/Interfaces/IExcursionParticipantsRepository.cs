namespace deepdiveapi.Repositories.Interfaces
{
    /// <summary>
    /// The IExcursionParticipantsRepository interface defines methods for managing participants of excursions.
    /// </summary>
    public interface IExcursionParticipantsRepository
    {
        public Task AddParticipantToExcursion(string excursionId, string userId);
        public Task RemoveParticipantFromExcursion(string excursionId, string userId);
        public Task AddMultipleParticipantsToExcursion(string excursionId, List<string> userIds);
    }
}
