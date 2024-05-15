using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using deepdiveapi.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using deepdiveapi.JwtFeatures;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities;
using deepdiveapi.Repositories.Interfaces;
using Serilog;

/// <summary>
/// Controller responsible for managing user-specific operations such as retrieving and updating user information.
/// </summary>
[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IRegistrationDocumentRepository _registrationDocumentRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userManager">Provides the APIs for managing user in a persistence store.</param>
    /// <param name="dbContext">Database context for entity framework operations.</param>
    /// <param name="registrationDocumentRepository">Repository for handling registration documents.</param>
    /// <param name="usersRepository">Repository for user-specific operations.</param>
    /// <param name="httpContextAccessor">Accessor for retrieving information about the HTTP context.</param>
    public UserController(UserManager<User> userManager, ApplicationDbContext dbContext, IRegistrationDocumentRepository registrationDocumentRepository, IUsersRepository usersRepository, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _registrationDocumentRepository = registrationDocumentRepository;
        _usersRepository = usersRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Retrieves all users from the repository.
    /// </summary>
    /// <returns>A list of users.</returns>
    [HttpGet]
    [Authorize]
    [Route("GetUsers")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            List<User> users = await _usersRepository.GetUsers();
            Log.Information("Users has been succesfully loaded");
            return Ok(users);
        }
        catch (Exception ex)
        {
            Log.Error("Users has not been loaded");
            Log.Error(ex.ToString(), ex.Message);
            return StatusCode(500, "An error occurred.");
        }
    }

    /// <summary>
    /// Retrieves users based on a typeahead search.
    /// </summary>
    /// <param name="input">Input DTO containing the search string.</param>
    /// <returns>A list of users that match the search criteria.</returns>
    [HttpPost]
    [Authorize]
    [Authorize(Roles="Administrator")]
    [Route("GetUsersTypeahead")]
    public async Task<IActionResult> GetUsersSearchTypehead(StringInputDto input)
    {
        try
        {
            var users = await _usersRepository.SearchUsersAsync(input.input);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred.");
        }
    }

    /// <summary>
    /// Retrieves a list of safe user information, typically excluding sensitive details.
    /// </summary>
    /// <returns>A list of user information considered safe to share.</returns>
    [HttpGet]
    [Authorize]
    [Route("GetSafeUsers")]
    public async Task<IActionResult> GetSafeUsers()
    {
        try
        {
            List<UserForSafeListDto> users = await _usersRepository.GetSafeUsers();
            Log.Information("GetSafeUsers endpoint succeeded.");
            return Ok(users);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred in GetSafeUsers endpoint.");
            return StatusCode(500, "An error occurred.");
        }
    }

    /// <summary>
    /// Retrieves safe user information for a specific user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>Safe user information if found; otherwise, a not found result.</returns>
    [HttpGet]
    [Authorize]
    [Route("GetSafeUserFromId/{userId}")]
    public async Task<IActionResult> GetSafeUsers(string userId)
    {
        try
        {
            UserForSafeListDto userForSafeListDto = await _usersRepository.GetSafeUserFromID(userId);

            if (userForSafeListDto == null)
            {
                Log.Warning("User not found for ID:", userId);
                return BadRequest("User not found");
            }

            Log.Information($"GetSafeUserFromId endpoint succeeded for ID:", userId);
            return Ok(userForSafeListDto);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred in GetSafeUserFromId endpoint for ID: ", userId);
            return StatusCode(500, "An error occurred.");
        }
    }

    /// <summary>
    /// Views the registration application for a specific user by user ID.
    /// </summary>
    /// <param name="input">DTO containing the user ID.</param>
    /// <returns>The registration application details if found; otherwise, NotFound.</returns>
    [HttpPost]
    [Authorize]
    [Route("ViewApplication")]
    public async Task<IActionResult> ViewApplicationRegistration(IdInputDto input)
    {
        await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                User currentUser = await _userManager.FindByIdAsync(input.Id);

                if (currentUser == null)
                {
                    Log.Warning("User not found for ID: ", input.Id);
                    return NotFound();
                }

                RegistrationRequest registrationRequest = await _dbContext.RegistrationRequests.FirstOrDefaultAsync(u => u.UserIdFK == currentUser.Id);

                if (registrationRequest == null)
                {
                    Log.Warning("Registration request not found for user ID: ", currentUser.Id);
                    return NotFound();
                }

                var viewApp = new ViewApplicationResponseDto
                {
                    Id = registrationRequest.Id,
                    FirstName = currentUser.FirstName,
                    LastName = currentUser.LastName,
                    UserName = currentUser.UserName,
                    PhoneNumber = currentUser.PhoneNumber,
                    Email = currentUser.Email,
                    BirthDate = currentUser.BirthDate,
                    Status = registrationRequest.RegistrationStatus,
                    AdminComment = registrationRequest.AdminComment,
                    CreatedOn = registrationRequest.CreatedOn,
                    EditedOn = registrationRequest.EditedOn,
                    ApprovedOrDeniedOn = registrationRequest.ApprovedOrDeniedOn,
                    DocumentsInfo = (await _registrationDocumentRepository.GetDocumentsOfUser(currentUser.Id))
                        .Select(doc => new
                        {
                            Id = doc.Id,
                            DocumentName = doc.DocumentName,
                            UserIdFK = doc.UserIdFK,
                            DocumentType = doc.DocumentType,
                            CreatedOn = doc.CreatedOn
                        })
                        .ToList<object>()
                };

                await transaction.CommitAsync();
                Log.Information("Transaction committed successfully for user ID: ", input.Id);
                return Ok(viewApp);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Error occurred while processing transaction for user ID: }", input.Id);
                return StatusCode(500, "An error occurred.");
            }
        }
    }

    /// <summary>
    /// Updates the details of an existing user.
    /// </summary>
    /// <param name="input">DTO containing updated user details.</param>
    /// <returns>Response indicating success or failure of the update operation.</returns>
    [HttpPatch]
    [Authorize]
    [Route("Update")]
    public async Task<IActionResult> Update(UpdateUserInputDto input)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var user = await _userManager.FindByIdAsync(input.Id);

                if (user == null)
                {
                    Log.Warning("User not found for ID: ", input.Id);
                    return BadRequest(new UpdateUserResponseDto { IsSuccessfullUpdate = false, Errors = new List<string> { "User not found." } });
                }

                if (!ModelState.IsValid)
                {
                    Log.Warning("Invalid model state for user ID: ", input.Id);
                    return BadRequest(new UpdateUserResponseDto { IsSuccessfullUpdate = false, Errors = new List<string> { "Email already in use." } });
                }

                user.FirstName = input.FirstName;
                user.LastName = input.LastName;
                user.BirthDate = input.BirthDate;
                user.UserName = input.UserName;
                user.Email = input.Email;
                user.PhoneNumber = input.PhoneNumber;

                var existingUserWithEmail = await _userManager.FindByEmailAsync(input.Email);
                if (existingUserWithEmail != null && existingUserWithEmail.Id != user.Id)
                {
                    Log.Warning("Email already in use for user ID: ", input.Id);
                    return BadRequest(new UpdateUserResponseDto { IsSuccessfullUpdate = false, Errors = new List<string> { "Email already in use." } });
                }

                if (!string.IsNullOrEmpty(input.Password))
                {
                    var passwordValidator = _userManager.PasswordValidators.First();
                    var validationResult = await passwordValidator.ValidateAsync(_userManager, user, input.Password);

                    if (validationResult.Succeeded)
                    {
                        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, input.Password);
                    }
                    else
                    {
                        var errors = validationResult.Errors.Select(e => e.Description);
                        Log.Warning("Password validation failed for user ID: ", input.Id);
                        return BadRequest(new UpdateUserResponseDto { IsSuccessfullUpdate = false, Errors = errors });
                    }
                }

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    Log.Warning("Failed to update user with ID: ", input.Id);
                    return BadRequest(new UpdateUserResponseDto { IsSuccessfullUpdate = false, Errors = errors });
                }

                await transaction.CommitAsync();
                Log.Information("User updated successfully with ID: ", input.Id);
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Error occurred while updating user with ID: }", input.Id);
                return StatusCode(500, "An error occurred.");
            }
        }
    }

    /// <summary>
    /// Disables a user account.
    /// </summary>
    /// <param name="input">DTO containing the ID of the user to disable.</param>
    /// <returns>Response indicating success or failure of the disable operation.</returns>
    [HttpPost]
    [Authorize]
    [Route("Disable")]
    public async Task<IActionResult> DisableAccount(IdInputDto input)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                await _usersRepository.DisableAccount(input.Id);
                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred.");
            }
        }
    }

    /// <summary>
    /// Enables a user account.
    /// </summary>
    /// <param name="input">DTO containing the email of the user to enable.</param>
    /// <returns>Response indicating success or failure of the enable operation.</returns>
    [HttpPost]
    [Route("Enable")]
    public async Task<IActionResult> EnableAccount(EmailInputDto input)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                await _usersRepository.EnableAccount(input.Email);
                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred.");
            }
        }
    }

    /// <summary>
    /// Deletes a user account.
    /// </summary>
    /// <param name="input">DTO containing the ID of the user to delete.</param>
    /// <returns>Response indicating success or failure of the delete operation.</returns>
    [HttpDelete]
    [Authorize]
    [Route("Delete")]
    public async Task<IActionResult> DeleteAccount(IdInputDto input)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var userIdFromCall = JwtHandler.GetUserIdFromToken(_httpContextAccessor.HttpContext.Request);
                if (userIdFromCall == null)
                {
                    return BadRequest();
                }
                if (userIdFromCall != input.Id)
                {
                    return BadRequest(new ErrorResponseDto { Errors = new[] { "User making request is not user that is trying to be deleted." } });
                }

                await _usersRepository.DeleteAccount(input.Id);
                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred.");
            }
        }
    }
}