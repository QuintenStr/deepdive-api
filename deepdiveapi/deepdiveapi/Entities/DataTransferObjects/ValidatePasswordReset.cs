using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for validating a password reset operation using ID and token.
    /// </summary>
    public class ValidatePasswordReset
    {
        [Required]
        public required string Id { get; set; }
        [Required]
        public required string Token { get; set; }

    }
}