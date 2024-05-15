using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for updating a user's password, requiring verification via token.
    /// </summary>
    public class UpdateUserPassword
    {
        [Required]
        public required string Id { get; set; }
        [Required]
        public required string Token { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
