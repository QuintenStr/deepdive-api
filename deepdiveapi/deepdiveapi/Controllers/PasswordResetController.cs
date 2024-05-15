using deepdiveapi.Entities;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Enum;
using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace deepdiveapi.Controllers
{
    /// <summary>
    /// Controller responsible for handling password reset requests.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class PasswordResetController : ControllerBase
    {
        private readonly IPasswordResetRepository _passwordResetrepo;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordResetController"/> class.
        /// </summary>
        /// <param name="passwordResetrepo">Repository for handling password reset operations.</param>
        /// <param name="dbContext">Database context for accessing application data.</param>
        /// <param name="userManager">User manager for user-related operations.</param>
        public PasswordResetController(IPasswordResetRepository passwordResetrepo, ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            _passwordResetrepo = passwordResetrepo;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        /// <summary>
        /// Initiates a password reset request for the given email address.
        /// </summary>
        /// <param name="email">DTO containing the email address for which the password reset is requested.</param>
        /// <returns>Ok if the request is successful, or InternalServerError on exception.</returns>
        [HttpPost]
        [Route("Reset")]
        public async Task<IActionResult> AddPasswordRequest(EmailInputDto email)
        {
            try
            {
                await _passwordResetrepo.AddPasswordReset(email.Email);
                Log.Information("Password reset request added successfully for email: ", email.Email);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding password reset request for email: ", email.Email);
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
            return Ok();
        }

        /// <summary>
        /// Validates a password reset request based on the provided input.
        /// </summary>
        /// <param name="input">DTO containing information required to validate the password reset request.</param>
        /// <returns>Ok if the request is valid, BadRequest if not found, expired, or already used, or InternalServerError on exception.</returns>
        [HttpPost]
        [Route("ValidateReset")]
        public async Task<IActionResult> ValidateReset(ValidatePasswordReset input)
        {
            try
            {
                var passwordReset = _passwordResetrepo.FindByEmailAndToken(input);

                if (passwordReset == null)
                {
                    Log.Warning("Password reset entry not found for input: ", input);
                    return BadRequest(new ErrorResponseDto { Errors = new[] { "Can not find password reset entry." } });
                }

                if (passwordReset.Status == PasswordResetsStastusEnum.PwdChanged)
                {
                    Log.Warning("Password reset entry already used for input: ", input);
                    return BadRequest(new ErrorResponseDto { Errors = new[] { "This entry for password reset has already been used." } });
                }

                if (DateTime.UtcNow >= passwordReset.ExpireOn)
                {
                    Log.Warning("Password reset entry expired for input: {@Input}", input);
                    return BadRequest(new ErrorResponseDto { Errors = new[] { "Password reset entry expired." } });
                }

                Log.Information("Password reset entry found and valid for input: {@Input}", input);
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing password reset for input: {@Input}", input);
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates the password for a user based on the provided input.
        /// </summary>
        /// <param name="input">DTO containing information required to update the user's password.</param>
        /// <returns>Ok if the password is updated successfully, BadRequest if not found, expired, or update failed, or InternalServerError on exception.</returns>
        [HttpPost]
        [Route("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(UpdateUserPassword input)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var validation = new ValidatePasswordReset
                    {
                        Id = input.Id,
                        Token = input.Token,
                    };

                    var passwordReset = _passwordResetrepo.FindByEmailAndToken(validation);

                    if (passwordReset == null)
                    {
                        Log.Warning("Password reset entry not found for input: ", validation);
                        return BadRequest(new ErrorResponseDto { Errors = new[] { "Can not find password reset entry." } });
                    }

                    if (DateTime.UtcNow >= passwordReset.ExpireOn)
                    {
                        Log.Warning("Password reset entry expired for input: ", validation);
                        return BadRequest(new ErrorResponseDto { Errors = new[] { "Password reset entry expired." } });
                    }

                    var user = await _userManager.FindByIdAsync(input.Id);

                    if (user == null)
                    {
                        Log.Warning("User not found for input: ", input);
                        return BadRequest(new ErrorResponseDto { Errors = new[] { "User not found." } });
                    }

                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, input.Password);
                    if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(e => e.Description);
                        Log.Warning($"Password reset failed for input: {input},  Errors: {errors}");
                        return BadRequest(new ErrorResponseDto { Errors = errors });
                    }

                    passwordReset.Status = PasswordResetsStastusEnum.PwdChanged;

                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    Log.Information($"Password reset successful for input: {input}");
                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Log.Error(ex, $"An error occurred while processing password reset for input: {input}");
                    return StatusCode(500, "An error occurred.");
                }
            }
        }
    }
}
