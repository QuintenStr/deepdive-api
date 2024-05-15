using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;

namespace deepdiveapi.Repositories.Interfaces
{
    /// <summary>
    /// The IPasswordResetRepository interface defines methods for managing password reset requests.
    /// </summary>
    public interface IPasswordResetRepository
    {
        public Task AddPasswordReset(string email);
        public PasswordReset FindByEmailAndToken(ValidatePasswordReset input);
    }
}
