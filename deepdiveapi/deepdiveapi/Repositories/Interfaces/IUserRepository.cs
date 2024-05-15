using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;

namespace deepdiveapi.Repositories.Interfaces
{
    /// <summary>
    /// The IUsersRepository interface defines methods for user management.
    /// </summary>
    public interface IUsersRepository
    {
        public Task<List<User>> GetUsers();
        public Task<List<UserForSafeListDto>?> GetSafeUsers();
        public Task<UserForSafeListDto?> GetSafeUserFromID(string id);
        public Task<List<UsersTypeahead>> SearchUsersAsync(string searchString);
        public Task EnableAccount(string userId);
        public Task DisableAccount(string userId);
        public Task DeleteAccount(string userId);
    }
}
