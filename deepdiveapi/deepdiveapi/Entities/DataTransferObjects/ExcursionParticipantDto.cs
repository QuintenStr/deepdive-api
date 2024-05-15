using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for managing participants of an excursion.
    /// </summary>
    public class ExcursionParticipantDto
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string ExcursionId { get; set; }
    }
}
