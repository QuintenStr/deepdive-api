using deepdiveapi.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace deepdiveapi.JwtFeatures
{
    /// <summary>
    /// Provides methods for managing JWT tokens including creation and validation.
    /// </summary>
    public class JwtHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _jwtSettings;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Initializes a new instance of the JwtHandler class.
        /// </summary>
        /// <param name="configuration">Application configuration containing JWT settings.</param>
        /// <param name="userManager">User manager for accessing user data and roles.</param>
        public JwtHandler(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _jwtSettings = _configuration.GetSection("JwtSettings");
            _userManager = userManager;
        }

        /// <summary>
        /// Generates signing credentials using the security key from configuration.
        /// </summary>
        /// <returns>Signing credentials for creating secure JWTs.</returns>
        public SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.GetSection("securityKey").Value);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// Asynchronously generates a list of claims for a specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate claims.</param>
        /// <returns>A task representing the asynchronous operation, containing the list of user claims.</returns>
        public async Task<List<Claim>> GetClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (user.EmailConfirmed == false)
            {
                claims.Add(new Claim("EmailVerified", "false"));

            }

            return claims;
        }

        /// <summary>
        /// Generates a JWT token using the provided signing credentials and claims.
        /// </summary>
        /// <param name="signingCredentials">Signing credentials used to sign the token.</param>
        /// <param name="claims">Claims to include in the token.</param>
        /// <returns>A JwtSecurityToken object.</returns>
        public JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken(
                issuer: _jwtSettings["validIssuer"],
                audience: _jwtSettings["validAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_jwtSettings["expiryInMinutes"])),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }

        /// <summary>
        /// Generates a refresh token.
        /// </summary>
        /// <returns>A randomly generated refresh token.</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Retrieves the principal from an expired JWT token.
        /// </summary>
        /// <param name="accessToken">The expired JWT token.</param>
        /// <returns>ClaimsPrincipal extracted from the token.</returns>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string? accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetSection("securityKey").Value)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");

                return principal;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Generates an access token using the specified claims.
        /// </summary>
        /// <param name="claims">Claims to include in the token.</param>
        /// <returns>A string representation of the JWT token.</returns>
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings["securityKey"]));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenOptions = new JwtSecurityToken(
                issuer: _jwtSettings["validIssuer"],
                audience: _jwtSettings["validAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_jwtSettings["expiryInMinutes"])),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        /// <summary>
        /// Extracts and decodes the user ID from a JWT token.
        /// </summary>
        /// <param name="request">The HttpRequest containing the token.</param>
        /// <returns>The user ID if present, otherwise null.</returns>
        public static string GetUserIdFromToken(HttpRequest request)
        {
            var token = request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                return DecodeJwtToken(token);
            }
            return null;
        }

        /// <summary>
        /// Decodes a JWT token and extracts the user ID.
        /// </summary>
        /// <param name="token">The JWT token to decode.</param>
        /// <returns>The user ID from the token.</returns>
        private static string DecodeJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken == null)
                throw new ArgumentException("Invalid JWT token");

            var userId = jsonToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            return userId;
        }

    }
}
