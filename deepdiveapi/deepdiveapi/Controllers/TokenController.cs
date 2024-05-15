using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;
using deepdiveapi.JwtFeatures;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace deepdiveapi.Controllers
{
    /// <summary>
    /// Controller responsible for managing JWT token operations such as refreshing tokens.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenRepository _tokenRepository;
        private readonly JwtHandler _jwtHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager for performing user-related operations.</param>
        /// <param name="tokenRepository">The repository handling token storage and validation.</param>
        /// <param name="jwtHandler">The JWT handler for managing JWT creation and validation.</param>
        public TokenController(UserManager<User> userManager, ITokenRepository tokenRepository, JwtHandler jwtHandler)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
            _jwtHandler = jwtHandler;
        }

        /// <summary>
        /// Refreshes a JWT token using an old token and a refresh token.
        /// </summary>
        /// <param name="tokenRefreshDto">The DTO containing the access and refresh tokens.</param>
        /// <returns>A new access token and refresh token if validation is successful; otherwise, unauthorized response.</returns>
        [HttpPost]
        [Route("Refresh")]
        public async Task<IActionResult> RefreshAsync(TokenRefreshDto tokenRefreshDto)
        {
            Console.WriteLine("REFRESH CALLED");
            ClaimsPrincipal principal = _jwtHandler.GetPrincipalFromExpiredToken(tokenRefreshDto.AccessToken);
            string userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            User user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // user not found
                Log.Warning("User not found with ID : ", userId);
                return Unauthorized("User not found.");
            }

            bool isValidRefreshToken = await _tokenRepository.IsValidRefreshTokenAsync(user.Id, tokenRefreshDto.RefreshToken);
            if (!isValidRefreshToken)
            {
                // validrefreshtoken
                Log.Warning("Invalid refresh token for user ID: ", userId);
                return Unauthorized("Invalid refresh token");
            }

            List<Claim> newClaims = await _jwtHandler.GetClaims(user);

            var newAccessToken = _jwtHandler.GenerateAccessToken(newClaims);

            var newRefreshToken = new RefreshToken
            {
                Token = _jwtHandler.GenerateRefreshToken(),
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
            };

            await _tokenRepository.StoreRefreshTokenAsync(user, newRefreshToken);
            await _tokenRepository.InvalidateOldRefreshToken(user, tokenRefreshDto.RefreshToken, newRefreshToken.Token);

            Log.Information("New tokens generated for user ID: ", userId);
            return Ok(new AuthResponseDto { Token = newAccessToken, RefreshToken = newRefreshToken.Token, IsAuthSuccessful = true });
        }
    }
}