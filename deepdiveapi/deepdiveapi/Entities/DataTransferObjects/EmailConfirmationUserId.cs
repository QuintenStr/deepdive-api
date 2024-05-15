using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for confirming an email address associated with a specific user ID.
    /// </summary>
    public class EmailConfirmationUserId
    {
        [Required]
        public required string UserId { get; set; }
        [Required]
        public required string Email { get; set; }
    }
}
