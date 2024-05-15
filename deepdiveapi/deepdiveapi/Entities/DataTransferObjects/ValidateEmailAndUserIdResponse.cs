using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO to communicate the confirmation status of a user's email.
    /// </summary>
    public class ValidateEmailAndUserIdResponse
    {
        [Required]
        public required bool EmailHasBeenConfirmed { get; set; }
    }
}
