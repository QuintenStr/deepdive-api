using deepdiveapi.Entities;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace deepdiveapi.Repositories
{
    /// <summary>
    /// The UsersRepository class is responsible for managing user-related data operations, including CRUD operations and specialized queries.
    /// </summary>
    public class UsersRepository : IUsersRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for UsersRepository.
        /// </summary>
        /// <param name="userManager">Manages user data from a database.</param>
        /// <param name="context">Application database context.</param>
        public UsersRepository(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all users.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of User entities.</returns>
        public async Task<List<User>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return users;
        }

        /// <summary>
        /// Retrieves a list of users with safe-to-display data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of UserForSafeListDto entities, or null if no users are found.</returns>
        public async Task<List<UserForSafeListDto>?> GetSafeUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            if (users == null)
            {
                return null;
            }
            List<UserForSafeListDto> usersSafe = new List<UserForSafeListDto>();
            foreach (var item in users)
            {
                var user = new UserForSafeListDto
                {
                    Id = item.Id,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    Username = item.UserName,
                    Birthdate = item.BirthDate,
                    Email = item.Email,
                    PhoneNumber = item.PhoneNumber
                };
                usersSafe.Add(user);
            }
            return usersSafe;
        }


        /// <summary>
        /// Retrieves safe-to-display user data for a specific user based on ID.
        /// </summary>
        /// <param name="id">The user ID to search for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a UserForSafeListDto entity, or null if the user is not found.</returns>
        public async Task<UserForSafeListDto?> GetSafeUserFromID(string id)
        {
            var item = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return null;
            }
            var userSafe = new UserForSafeListDto
            {
                Id = item.Id,
                FirstName = item.FirstName,
                LastName = item.LastName,
                Username = item.UserName,
                Birthdate = item.BirthDate,
                Email = item.Email,
                PhoneNumber = item.PhoneNumber
            };
            return userSafe;
        }

        /// <summary>
        /// Searches for users by a given search string which can match several user attributes.
        /// </summary>
        /// <param name="searchString">The string to search for, ignoring case.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of UsersTypeahead entities.</returns>
        public async Task<List<UsersTypeahead>> SearchUsersAsync(string searchString)
        {
            var normalizedSearchString = searchString.ToUpperInvariant();

            var users = await _userManager.Users
                .Where(u => EF.Functions.Like(u.UserName.ToUpper(), $"%{normalizedSearchString}%")
                    || EF.Functions.Like(u.Email.ToUpper(), $"%{normalizedSearchString}%")
                    || EF.Functions.Like(u.FirstName.ToUpper(), $"%{normalizedSearchString}%")
                    || EF.Functions.Like(u.LastName.ToUpper(), $"%{normalizedSearchString}%")
                    || EF.Functions.Like((u.FirstName.ToUpper() + " " + u.LastName.ToUpper()), $"%{normalizedSearchString}%")
                    || EF.Functions.Like((u.LastName.ToUpper() + " " + u.FirstName.ToUpper()), $"%{normalizedSearchString}%"))
                .ToListAsync();

            var formattedUsers = users.Select(item => new UsersTypeahead
            {
                FirstName = item.FirstName,
                LastName = item.LastName,
                Id = item.Id,
                UserName = item.UserName
            }).ToList();

            return formattedUsers;
        }

        /// <summary>
        /// Disables a user account by marking it as deleted.
        /// </summary>
        /// <param name="userId">The ID of the user whose account is to be disabled.</param>
        public async Task DisableAccount(string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Enables a user account by unmarking it as deleted.
        /// </summary>
        /// <param name="email">The email address of the user whose account is to be enabled.</param>
        public async Task EnableAccount(string email)
        {
            // have to use context because usermanager doesnt have ignore query filters
            User user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.NormalizedEmail == email.Normalize());
            if (user != null)
            {
                user.IsDeleted = false;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Deletes a user account by removing it from the database.
        /// </summary>
        /// <param name="userId">The ID of the user whose account is to be deleted.</param>
        public async Task DeleteAccount(string userId)
        {
            User user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}