using deepdiveapi.Entities;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Enum;
using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Text.Json;

namespace deepdiveapi.Repositories
{
    /// <summary>
    /// The PasswordResetRepository class manages operations related to user password resets.
    /// </summary>
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor for PasswordResetRepository.
        /// </summary>
        /// <param name="dbContext">Application database context.</param>
        /// <param name="userManager">Manages user data from a database.</param>
        /// <param name="configuration">Configuration interface to access application settings.</param>
        public PasswordResetRepository(ApplicationDbContext dbContext, UserManager<User> userManager, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Initiates a password reset process for a user by adding a password reset entry to the database and sending an email with reset instructions.
        /// </summary>
        /// <param name="email">The email address of the user requesting the password reset.</param>
        public async Task AddPasswordReset(string email)
        {
            User userPassword = await _userManager.FindByEmailAsync(email);
            if (userPassword != null)
            {
                PasswordReset newEntry = new PasswordReset()
                {
                    Token = Guid.NewGuid().ToString(),
                    UserIdFK = userPassword.Id,
                    CreatedOn = DateTime.UtcNow,
                    ExpireOn = DateTime.UtcNow.AddMinutes(10),
                    Status = PasswordResetsStastusEnum.Requested
                };
                _dbContext.PasswordResets.Add(newEntry);

                using (var httpClient = new HttpClient())
                {
                    string htmlTemplate = System.IO.File.ReadAllText($"Entities/Emails/Html/resetpassword.html");
                    string plaintextTemplate = System.IO.File.ReadAllText($"Entities/Emails/Plaintext/resetpassword.txt");

                    string resetUrl = $"{_configuration["ApiSettings:Frontend"]}/auth/complete-pwdreset?id={userPassword.Id}&token={newEntry.Token}";

                    htmlTemplate = htmlTemplate.Replace("{ResetPasswordUrl}", resetUrl);
                    plaintextTemplate = plaintextTemplate.Replace("{ResetPasswordUrl}", resetUrl);

                    EmailModelDto input = new EmailModelDto
                    {
                        Subject = "Password Reset",
                        PlainTextContent = plaintextTemplate,
                        HtmlContent = htmlTemplate,
                        toRecipients = new List<string> { userPassword.Email },
                        ccRecipients = new List<string>(),
                        bccRecipients = new List<string>(),
                        Attachments = new List<EmailAttachmentBinaryBypass>()
                    };

                    string apiUrl = _configuration["ApiSettings:MailAPI"];

                    string jsonInput = JsonSerializer.Serialize(input);

                    var content = new StringContent(jsonInput, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                }

                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Finds a password reset entry based on a user ID and token.
        /// </summary>
        /// <param name="input">Data transfer object containing the user ID and token to validate.</param>
        /// <returns>PasswordReset entity if found; otherwise null.</returns>
        public PasswordReset FindByEmailAndToken(ValidatePasswordReset input)
        {
            return _dbContext.PasswordResets.FirstOrDefault(pwdr => pwdr.UserIdFK == input.Id && pwdr.Token == input.Token);
        }

    }
}
