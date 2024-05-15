using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for adding multiple participants to an excursion.
    /// </summary>
    public class ExcursionMultipleParticipantsDto
    {
        [Required]
        public List<string> UserId { get; set; }
        [Required]
        public string ExcursionId { get; set; }
    }
}
