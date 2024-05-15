using Azure;

namespace deepdiveapi.Repositories.Interfaces
{
    /// <summary>
    /// The IExcursionParticipantsRepository interface defines operations related to administrator users.
    /// </summary>
    public interface IAdminRepository
    {
        public Task<List<string>> GetAdminUserEmailsAsync();
    }
}