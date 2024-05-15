using Microsoft.AspNetCore.Mvc;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;
using Microsoft.AspNetCore.Identity;
using deepdiveapi.Entities;
using deepdiveapi.Repositories.Interfaces;
using deepdiveapi.Entities.Enum;
using System.Text;
using System.Text.Json;
using Serilog;

namespace deepdiveapi.Controllers
{
    /// <summary>
    /// Controller responsible for managing registration requests.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class RegistrationRequestController : ControllerBase
    {
        private readonly IRegistrationRequestRepository _registrationRequestRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationRequestController"/> class.
        /// </summary>
        /// <param name="registrationRequestRepository">Repository for accessing and updating registration requests.</param>
        /// <param name="dbContext">Database context for entity operations.</param>
        /// <param name="userManager">User manager for user-related operations.</param>
        /// <param name="configuration">Configuration for accessing application settings.</param>
        public RegistrationRequestController(IRegistrationRequestRepository registrationRequestRepository, ApplicationDbContext dbContext, UserManager<User> userManager, IConfiguration configuration)
        {
            _registrationRequestRepository = registrationRequestRepository;
            _dbContext = dbContext;
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Updates the status of a registration request for a specific user.
        /// </summary>
        /// <param name="input">DTO containing the user ID and new status for the registration request.</param>
        /// <returns>Ok if the update is successful, BadRequest if the request is not found, or InternalServerError on exception.</returns>
        [HttpPatch]
        [Route("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(UpdateStatusRegistrationRequestDto input)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var registrationRequest = await _registrationRequestRepository.GetRegistrationRequestByUserIdAsync(input.UserId);
                    if (registrationRequest != null)
                    {
                        registrationRequest.RegistrationStatus = input.Status;
                        registrationRequest.EditedOn = DateTime.UtcNow;
                        await _registrationRequestRepository.UpdateRegistrationRequestAsync(registrationRequest);
                        await transaction.CommitAsync();
                        Log.Information("Registration request updated successfully for user ID: ", input.UserId);
                        return Ok();
                    }
                    else
                    {
                        Log.Warning("Registration request not found for user ID: ", input.UserId);
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Log.Error(ex, "Error occurred while updating registration request for user ID: ", input.UserId);
                    return StatusCode(500, "Internal server error");
                }
            }
        }

        /// <summary>
        /// Gets the HTML template path based on the registration status.
        /// </summary>
        /// <param name="status">The registration status.</param>
        /// <returns>The path to the HTML template.</returns>
        private string GetHtmlTemplatePath(RegistrationStatusEnum status)
        {
            return $"Entities/Emails/Html/registrationrequeststatusupdate_{status.ToString().ToLower()}.html";
        }

        /// <summary>
        /// Gets the plaintext template path based on the registration status.
        /// </summary>
        /// <param name="status">The registration status.</param>
        /// <returns>The path to the plaintext template.</returns>
        private string GetPlaintextTemplatePath(RegistrationStatusEnum status)
        {
            return $"Entities/Emails/Plaintext/registrationrequeststatusupdate_{status.ToString().ToLower()}.txt";
        }

        /// <summary>
        /// Replaces template variables in the provided template.
        /// </summary>
        /// <param name="template">The template to replace variables in.</param>
        /// <param name="input">The input DTO containing variable values.</param>
        /// <param name="firstName">The first name of the user.</param>
        /// <returns>The template with replaced variables.</returns>
        private string ReplaceTemplateVariables(string template, UpdateRegistrationRequestDto input, string firstName)
        {
            if (input.AdminComment != null)
            {
                template = template.Replace("{AdminComment}", input.AdminComment);
            }
            return template.Replace("{FirstName}", firstName);
        }

        /// <summary>
        /// Updates the status of a registration request for a specific user.
        /// </summary>
        /// <param name="input">DTO containing the user ID and new status for the registration request.</param>
        /// <returns>Ok if the update is successful, BadRequest if the request is not found, or InternalServerError on exception.</returns>
        [HttpPatch]
        [Route("Update")]
        public async Task<IActionResult> UpdateStatus(UpdateRegistrationRequestDto input)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var registrationRequest = await _registrationRequestRepository.GetRegistrationRequestById(input.RequestId);
                    if (registrationRequest != null)
                    {
                        registrationRequest.RegistrationStatus = input.RegistrationStatus;
                        if (input.AdminComment != null)
                        {
                            registrationRequest.AdminComment = input.AdminComment;
                        }
                        registrationRequest.ApprovedOrDeniedOn = DateTime.UtcNow;

                        await _registrationRequestRepository.UpdateRegistrationRequestAsync(registrationRequest);
                        var user = await _dbContext.Users.FindAsync(registrationRequest.UserIdFK);

                        if (input.RegistrationStatus == RegistrationStatusEnum.Approved)
                        {
                            if (user != null)
                            {
                                var existingRoles = await _userManager.GetRolesAsync(user);
                                await _userManager.RemoveFromRolesAsync(user, existingRoles);
                                await _userManager.AddToRoleAsync(user, "User");
                            }
                        }

                        await transaction.CommitAsync();

                        using (var httpClient = new HttpClient())
                        {
                            string htmlTemplate = System.IO.File.ReadAllText(GetHtmlTemplatePath(input.RegistrationStatus));
                            string plaintextTemplate = System.IO.File.ReadAllText(GetPlaintextTemplatePath(input.RegistrationStatus));

                            htmlTemplate = ReplaceTemplateVariables(htmlTemplate, input, user?.FirstName);
                            plaintextTemplate = ReplaceTemplateVariables(plaintextTemplate, input, user?.FirstName);

                            EmailModelDto emailInput = new EmailModelDto
                            {
                                Subject = "Status Update",
                                PlainTextContent = plaintextTemplate,
                                HtmlContent = htmlTemplate,
                                toRecipients = new List<string> { user?.Email },
                                ccRecipients = new List<string>(),
                                bccRecipients = new List<string>(),
                                Attachments = new List<EmailAttachmentBinaryBypass>()
                            };

                            string apiUrl = _configuration["ApiSettings:MailAPI"];

                            string jsonInput = JsonSerializer.Serialize(emailInput);

                            var content = new StringContent(jsonInput, Encoding.UTF8, "application/json");

                            HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                            if (response.IsSuccessStatusCode)
                            {
                                Log.Information("Email sent successfully to user ID: ", user?.Id);
                            }
                            else
                            {
                                Log.Warning("Failed to send email to user ID: ", user?.Id);
                            }
                        }
                        return Ok();
                    }
                    else
                    {
                        Log.Warning("Registration request not found for request ID: ", input.RequestId);
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Log.Error(ex, "Error occurred while processing registration request ID: ", input.RequestId);
                    return StatusCode(500, "Internal server error");
                }
            }
        }

    }
}