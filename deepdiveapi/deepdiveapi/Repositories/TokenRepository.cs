using deepdiveapi.Entities.Models;
using deepdiveapi.Entities;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace deepdiveapi.Repositories
{
    /// <summary>
    /// The TokenRepository class manages operations related to user authentication tokens, including storing and invalidating refresh tokens.
    /// </summary>
    public class TokenRepository : ITokenRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for TokenRepository.
        /// </summary>
        /// <param name="context">Application database context.</param>
        public TokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Stores a new refresh token for a user.
        /// </summary>
        /// <param name="user">The user to associate the refresh token with.</param>
        /// <param name="refreshToken">The refresh token to store.</param>
        public async Task StoreRefreshTokenAsync(User user, RefreshToken refreshToken)
        {
            if (user.RefreshTokens == null)
            {
                user.RefreshTokens = new List<RefreshToken>();
            }

            user.RefreshTokens.Add(refreshToken);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Invalidates an old refresh token and optionally replaces it with a new one.
        /// </summary>
        /// <param name="user">The user associated with the refresh token.</param>
        /// <param name="oldRefreshToken">The old refresh token to be invalidated.</param>
        /// <param name="newRefreshToken">The new refresh token to replace the old one, if any.</param>
        public async Task InvalidateOldRefreshToken(User user, string oldRefreshToken, string newRefrehToken)
        {
            RefreshToken refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == oldRefreshToken && rt.UserIdFK == user.Id && rt.Revoked == null && rt.Expires > DateTime.UtcNow);

            if (refreshToken != null)
            {
                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.ReplacedByToken = newRefrehToken;
                _context.RefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Checks if a given refresh token is valid for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="refreshToken">The refresh token to validate.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the token is valid.</returns>
        public async Task<bool> IsValidRefreshTokenAsync(string userId, string refreshToken)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserIdFK == userId && rt.Token == refreshToken)
                .AnyAsync(rt => rt.Revoked == null && rt.Expires > DateTime.UtcNow);
        }
    }
}
