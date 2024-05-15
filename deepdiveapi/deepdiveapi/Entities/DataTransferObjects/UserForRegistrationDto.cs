using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for user registration details, including validations for required fields and data consistency.
    /// </summary>
    public class UserForRegistrationDto
    {
        [Required(ErrorMessage = "First name is required.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "User name is required.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Email confirmation is required.")]
        [Compare("Email", ErrorMessage = "The email and confirmation email do not match.")]
        [EmailAddress]
        public string? EmailConfirmation { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? PasswordConfirmation { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        public string? PhoneNumber {  get; set; }

        [Required(ErrorMessage = "Birthdate is required.")]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateOnly birthDate { get; set; }
    }
}
