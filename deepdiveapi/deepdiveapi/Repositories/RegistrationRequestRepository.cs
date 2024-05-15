using deepdiveapi.Entities;
using deepdiveapi.Entities.Enum;
using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace deepdiveapi.Repositories
{
    /// <summary>
    /// The RegistrationRequestRepository class manages operations related to user registration requests.
    /// </summary>
    public class RegistrationRequestRepository : IRegistrationRequestRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for RegistrationRequestRepository.
        /// </summary>
        /// <param name="context">Application database context.</param>
        public RegistrationRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new registration request for a user.
        /// </summary>
        /// <param name="userId">The ID of the user to add a registration request for.</param>
        public async Task AddRegistrationRequest(string userId)
        {

            RegistrationRequest newRequest = new RegistrationRequest
            {
                UserIdFK = userId,
                RegistrationStatus = RegistrationStatusEnum.Requested,
                CreatedOn = DateTime.UtcNow
            };

            _context.RegistrationRequests.Add(newRequest);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a registration request by user ID.
        /// </summary>
        /// <param name="userId">The user ID to search for a registration request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a RegistrationRequest entity, or null if no request is found.</returns>
        public async Task<RegistrationRequest> GetRegistrationRequestByUserIdAsync(string userId)
        {
            return await _context.RegistrationRequests.FirstOrDefaultAsync(r => r.UserIdFK == userId);
        }

        /// <summary>
        /// Updates a registration request.
        /// </summary>
        /// <param name="registrationRequest">The registration request to update.</param>
        public async Task UpdateRegistrationRequestAsync(RegistrationRequest registrationRequest)
        {
            _context.Entry(registrationRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves a registration request by its ID.
        /// </summary>
        /// <param name="requestId">The ID of the registration request to search for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a RegistrationRequest entity, or null if no request is found.</returns>
        public async Task<RegistrationRequest> GetRegistrationRequestById(int requestId)
        {
            return await _context.RegistrationRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        }
    }
}