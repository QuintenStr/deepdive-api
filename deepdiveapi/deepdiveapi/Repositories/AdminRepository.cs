using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace deepdiveapi.Repositories
{
    /// <summary>
    /// The AdminRepository class manages operations related to administrator users.
    /// </summary>
    public class AdminRepository : IAdminRepository
    {
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor for AdminRepository.
        /// </summary>
        /// <param name="userManager">User manager for identity management.</param>
        public AdminRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves email addresses of admin users asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of email addresses of admin users.</returns>
        public async Task<List<string>> GetAdminUserEmailsAsync()
        {
            return _userManager.GetUsersInRoleAsync("Administrator").Result.Select(x => x.Email).ToList();
        }
    }
}
