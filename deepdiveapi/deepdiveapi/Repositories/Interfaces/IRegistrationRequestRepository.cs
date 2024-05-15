using deepdiveapi.Entities.Models;

namespace deepdiveapi.Repositories.Interfaces
{
    /// <summary>
    /// The IRegistrationRequestRepository interface defines methods for managing registration requests.
    /// </summary>
    public interface IRegistrationRequestRepository
    {
        public Task AddRegistrationRequest(string userId);
        public Task<RegistrationRequest> GetRegistrationRequestById(int requestId);
        public Task<RegistrationRequest> GetRegistrationRequestByUserIdAsync(string userId);
        public Task UpdateRegistrationRequestAsync(RegistrationRequest registrationRequest);
    }
}
