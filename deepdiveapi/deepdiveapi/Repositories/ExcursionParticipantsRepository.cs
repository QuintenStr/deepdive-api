using deepdiveapi.Entities;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace deepdiveapi.Repositories
{
    /// <summary>
    /// The ExcursionParticipantsRepository class manages operations related to participants of excursions.
    /// </summary>
    public class ExcursionParticipantsRepository : IExcursionParticipantsRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor for ExcursionParticipantsRepository.
        /// </summary>
        /// <param name="dbContext">Application database context.</param>
        /// <param name="configuration">Configuration interface to access application settings.</param>
        public ExcursionParticipantsRepository(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        /// <summary>
        /// Adds a user as a participant to an excursion.
        /// </summary>
        /// <param name="excursionId">The ID of the excursion to add the participant to.</param>
        /// <param name="userId">The ID of the user to add as a participant.</param>
        public async Task AddParticipantToExcursion(string excursionId, string userId)
        {
            ExcursionParticipant newParticipant = new ExcursionParticipant()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                ExcursionId = excursionId,
            };

            _dbContext.ExcursionParticipants.Add(newParticipant);

            var user = await _dbContext.Users.FindAsync(userId);
            var excursion = await _dbContext.Excursions.FindAsync(excursionId);

            using (var httpClient = new HttpClient())
            {
                ParticipationConfirmationInputDto inputPdf = new ParticipationConfirmationInputDto
                {
                    QrCodeKey = excursion.Id + "///" + excursionId + "///" + userId,
                    DateTime = excursion.DateTime,
                    Coordinates = new CoordinatesDto()
                    {
                        Lat = excursion.Location.X,
                        Long = excursion.Location.Y,
                    }
                };

                
                string apiUrlPdf = _configuration["ApiSettings:PdfAPI"] + "/excursion-participation-confirmation";

                string jsonInputPdf = JsonSerializer.Serialize(inputPdf);
                var contentPdf = new StringContent(jsonInputPdf, Encoding.UTF8, "application/json");

                HttpResponseMessage pdfResponse = await httpClient.PostAsync(apiUrlPdf, contentPdf);
                if (pdfResponse.IsSuccessStatusCode)
                {
                    byte[] pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();

                    string htmlTemplate = System.IO.File.ReadAllText($"Entities/Emails/Html/addexcursionparticipant.html");
                    string plaintextTemplate = System.IO.File.ReadAllText($"Entities/Emails/Plaintext/addexcursionparticipant.txt");

                    EmailModelDto emailInput = new EmailModelDto
                    {
                        Subject = "Participation confirmation",
                        PlainTextContent = plaintextTemplate,
                        HtmlContent = htmlTemplate,
                        toRecipients = new List<string> { user.Email },
                        ccRecipients = new List<string>(),
                        bccRecipients = new List<string>(),
                        Attachments = new List<EmailAttachmentBinaryBypass>
                        {
                            new EmailAttachmentBinaryBypass("excursion_participation_confirmation.pdf", "application/pdf", pdfBytes)
                        }
                    };

                    string emailApiUrl = _configuration["ApiSettings:MailAPI"];
                    string emailJsonInput = JsonSerializer.Serialize(emailInput);
                    var emailContent = new StringContent(emailJsonInput, Encoding.UTF8, "application/json");

                    HttpResponseMessage emailResponse = await httpClient.PostAsync(emailApiUrl, emailContent);
                }
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Removes a participant from an excursion.
        /// </summary>
        /// <param name="excursionId">The ID of the excursion to remove the participant from.</param>
        /// <param name="userId">The ID of the user to remove as a participant.</param>
        public async Task RemoveParticipantFromExcursion(string excursionId, string userId)
        {
            var excursionParticipant = await _dbContext.ExcursionParticipants.FirstOrDefaultAsync(row => row.ExcursionId == excursionId && row.UserId == userId);

            if (excursionParticipant != null)
            {
                _dbContext.ExcursionParticipants.Remove(excursionParticipant);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Adds multiple participants to an excursion.
        /// </summary>
        /// <param name="excursionId">The ID of the excursion to add participants to.</param>
        /// <param name="userIds">The IDs of the users to add as participants.</param>
        public async Task AddMultipleParticipantsToExcursion(string excursionId, List<string> userIds)
        {
            foreach (var userId in userIds)
            {

                var participant = new ExcursionParticipant
                {
                    Id = Guid.NewGuid().ToString(),
                    ExcursionId = excursionId,
                    UserId = userId,
                };

                _dbContext.ExcursionParticipants.Add(participant);
                await _dbContext.SaveChangesAsync();
            }
        }

        private static string ByteArrayToBinaryString(byte[] byteArray)
        {
            string result = string.Empty;
            foreach (byte b in byteArray)
            {
                result += Convert.ToString(b, 2).PadLeft(8, '0');
            }
            return result;
        }

    }
}
