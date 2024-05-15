using deepdiveapi.Entities.Models;

namespace deepdiveapi.Repositories.Interfaces
{
    /// <summary>
    /// The ITokenRepository interface defines methods for managing authentication tokens.
    /// </summary>
    public interface ITokenRepository
    {
        public Task StoreRefreshTokenAsync(User user, RefreshToken refreshToken);
        public Task InvalidateOldRefreshToken(User user, string oldRefreshToken, string newRefreshToken);
        public Task<bool> IsValidRefreshTokenAsync(string userId, string refreshToken);
    }
}
