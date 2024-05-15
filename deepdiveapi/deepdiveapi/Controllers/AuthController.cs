using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using deepdiveapi.Entities.Models;
using deepdiveapi.JwtFeatures;
using deepdiveapi.Entities.DataTransferObjects;
using System.IdentityModel.Tokens.Jwt;
using deepdiveapi.Repositories.Interfaces;
using deepdiveapi.Entities;
using System.Text;
using System.Text.Json;
using Serilog;

/// <summary>
/// Controller responsible for user authentication and registration.
/// </summary>
[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly JwtHandler _jwtHandler;
    private readonly ITokenRepository _tokenRepository;
    private readonly IRegistrationRequestRepository _registrationRequestRepository;
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="userManager">Manager for handling user-related operations.</param>
    /// <param name="jwtHandler">Handler for JWT token generation and validation.</param>
    /// <param name="tokenRepository">Repository for managing refresh tokens.</param>
    /// <param name="registrationRequestRepository">Repository for managing registration requests.</param>
    /// <param name="dbContext">Database context for accessing application data.</param>
    /// <param name="configuration">Represents the application configuration properties.</param>
    public AuthController(UserManager<User> userManager, JwtHandler jwtHandler, ITokenRepository tokenRepository, IRegistrationRequestRepository registrationRequestRepository, ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtHandler = jwtHandler;
        _tokenRepository = tokenRepository;
        _registrationRequestRepository = registrationRequestRepository;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    /// <summary>
    /// Validates email with user ID for confirmation.
    /// </summary>
    /// <param name="input">DTO containing user ID and email for validation.</param>
    /// <returns>Result of email validation.</returns>
    [HttpPost("ValidateIdWithEmail")]
    public async Task<IActionResult> ValidateEmailWithId([FromBody] EmailConfirmationUserId input)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == input.UserId && u.Email == input.Email);

                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        Log.Warning($"Email is already verified for user with ID: {input.UserId}, Email: {input.Email}");
                        return BadRequest(new ErrorResponseDto { Errors = new[] { "Email is already verified." } });
                    }

                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    await transaction.CommitAsync();

                    Log.Information($"Email verified successfully for user with ID: {input.UserId}, Email: {input.Email}");
                    return Ok(new ValidateEmailAndUserIdResponse { EmailHasBeenConfirmed = true });
                }
                else
                {
                    Log.Warning($"User not found with ID: {input.UserId} and Email: {input.Email}");
                    return BadRequest(new ErrorResponseDto { Errors = new[] { "Can't find user with id and email." } });
                }

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error($"An error occurred while verifying email for user with ID: {input.UserId}, Email: {input.Email}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    /// <summary>
    /// Logs in a user.
    /// </summary>
    /// <param name="userForAuthentication">DTO containing user credentials for authentication.</param>
    /// <returns>Authentication result with access token and refresh token.</returns>
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] UserForAuthenticationDto userForAuthentication)
    {
        // make sure to use transaction if something fails previous steps get undone
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var user = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == userForAuthentication.Email);
                if (user != null && user.IsDeleted)
                {
                    Log.Warning($"Deleted user tried logging in");
                    return Unauthorized(new AuthResponseDto { ErrorMessage = "Deleted User" });
                }
                if (user == null || !await _userManager.CheckPasswordAsync(user, userForAuthentication.Password))
                {
                    Log.Warning($"Invalid authentication attempt for email: {userForAuthentication.Email}");
                    return Unauthorized(new AuthResponseDto { ErrorMessage = "Invalid Authentication" });
                }

                var signingCredentials = _jwtHandler.GetSigningCredentials();
                var claims = await _jwtHandler.GetClaims(user);
                var tokenOptions = _jwtHandler.GenerateTokenOptions(signingCredentials, claims);
                var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                var refreshToken = new RefreshToken
                {
                    Token = _jwtHandler.GenerateRefreshToken(),
                    Created = DateTime.Now,
                    Expires = DateTime.Now.AddDays(7),
                };
                await _tokenRepository.StoreRefreshTokenAsync(user, refreshToken);

                await transaction.CommitAsync();

                Log.Information($"User: {userForAuthentication.Email} authenticated successfully.");
                return Ok(new AuthResponseDto { IsAuthSuccessful = true, Token = accessToken, RefreshToken = refreshToken.Token });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error($"An error occurred during authentication for email: {userForAuthentication.Email}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    /// <summary>
    /// Resends email validation confirmation.
    /// </summary>
    /// <param name="userForRegistration">DTO containing user email for resending confirmation email.</param>
    /// <returns>Result of resending confirmation email.</returns>
    [HttpPost("ResendValidationEmail")]
    public async Task<IActionResult> ResendValidationEmail([FromBody] EmailInputDto userForRegistration)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                string htmlTemplate = System.IO.File.ReadAllText($"Entities/Emails/Html/confirmemail.html");
                string plaintextTemplate = System.IO.File.ReadAllText($"Entities/Emails/Plaintext/confirmemail.txt");

                string confirmationEndpoint = $"{_configuration["ApiSettings:DbAPI"]}/auth/confirm-email/{userForRegistration.Email}";

                htmlTemplate = htmlTemplate.Replace("{EmailConfirmationEndpoint}", confirmationEndpoint);
                plaintextTemplate = plaintextTemplate.Replace("{EmailConfirmationEndpoint}", confirmationEndpoint);

                EmailModelDto input = new EmailModelDto
                {
                    Subject = "Email Confirmation Required",
                    PlainTextContent = plaintextTemplate,
                    HtmlContent = htmlTemplate,
                    toRecipients = new List<string> { userForRegistration.Email },
                    ccRecipients = new List<string>(),
                    bccRecipients = new List<string>(),
                    Attachments = new List<EmailAttachmentBinaryBypass>()
                };

                string apiUrl = _configuration["ApiSettings:MailAPI"];

                string jsonInput = JsonSerializer.Serialize(input);

                var content = new StringContent(jsonInput, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    Log.Information($"Email sent successfully for confirmation to: {userForRegistration.Email}");
                    return Ok();
                }
                else
                {
                    Log.Error($"Failed to send confirmation email to: {userForRegistration.Email}. Status code: {response.StatusCode}");
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred while sending confirmation email to: {userForRegistration.Email}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="userForRegistration">DTO containing user details for registration.</param>
    /// <returns>Authentication result with access token and refresh token for the newly registered user.</returns>
    [HttpPost("Registration")]
    public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDto userForRegistration)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                if (userForRegistration == null || !ModelState.IsValid)
                {
                    Log.Warning("Invalid user registration data.");
                    return BadRequest();
                }

                var existingUserWithUsername = await _userManager.FindByNameAsync(userForRegistration.UserName);
                if (existingUserWithUsername != null)
                {
                    Log.Warning($"Username: {userForRegistration.UserName} is already taken.");
                    return BadRequest(new RegistrationResponseDto { Errors = new[] { "Username is already taken." } });
                }

                var existingUserWithEmail = await _userManager.FindByEmailAsync(userForRegistration.Email);
                if (existingUserWithEmail != null)
                {
                    Log.Warning($"Email: {userForRegistration.Email} is already registered.");
                    return BadRequest(new RegistrationResponseDto { Errors = new[] { "Email is already registered." } });
                }

                User newUser = new User
                {
                    UserName = userForRegistration.UserName,
                    FirstName = userForRegistration.FirstName,
                    LastName = userForRegistration.LastName,
                    Email = userForRegistration.Email,
                    PhoneNumber = userForRegistration.PhoneNumber,
                    BirthDate = userForRegistration.birthDate
                };

                var result = await _userManager.CreateAsync(newUser, userForRegistration.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    Log.Warning("Failed to create user, Errors: ", string.Join(",", errors));
                    return BadRequest(new RegistrationResponseDto { Errors = errors });
                }

                await _userManager.AddToRoleAsync(newUser, "CandidateUser");

                await _registrationRequestRepository.AddRegistrationRequest(newUser.Id);

                var signingCredentials = _jwtHandler.GetSigningCredentials();
                var claims = await _jwtHandler.GetClaims(newUser);
                var tokenOptions = _jwtHandler.GenerateTokenOptions(signingCredentials, claims);
                var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                var refreshToken = new RefreshToken
                {
                    Token = _jwtHandler.GenerateRefreshToken(),
                    Created = DateTime.Now,
                    Expires = DateTime.Now.AddDays(7),
                };
                await _tokenRepository.StoreRefreshTokenAsync(newUser, refreshToken);

                await transaction.CommitAsync();

                Log.Information("User registered successfully: ", newUser.Id);
                return Ok(new AuthResponseDto { IsAuthSuccessful = true, Token = accessToken, RefreshToken = refreshToken.Token });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Error occurred during user registration.");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    /// <summary>
    /// Validates user password.
    /// </summary>
    /// <param name="userForAuthentication">DTO containing user email and password for validation.</param>
    /// <returns>Result of password validation.</returns>
    [HttpPost("ValidatePassword")]
    public async Task<IActionResult> ValidatePassword([FromBody] UserForAuthenticationDto userForAuthentication)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userForAuthentication.Email);
                if (user == null)
                {
                    Log.Warning("User not found for email: ", userForAuthentication.Email);
                    return BadRequest(new ErrorResponseDto { Errors = new[] { "User not found" } });
                }
                if (!await _userManager.CheckPasswordAsync(user, userForAuthentication.Password))
                {
                    Log.Warning("Current password is not correct for user with email: ", userForAuthentication.Email);
                    return BadRequest(new ErrorResponseDto { Errors = new[] { "Current password is not correct!" } });
                }

                Log.Information("User authentication successful for email: ", userForAuthentication.Email);
                return Ok(new AuthResponseDto { IsAuthSuccessful = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Error occurred during user authentication for email: ", userForAuthentication.Email);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}