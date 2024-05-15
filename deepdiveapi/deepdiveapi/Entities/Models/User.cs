using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.Models
{
    /// <summary>
    /// Represents user information derived from IdentityUser.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the birth date of the user.
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateOnly BirthDate { get; set; }

        /// <summary>
        /// Indicates whether the user has been logically deleted from the system. (=disabled)
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation properties
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public ICollection<UserRegisterDocument> UserRegisterDocuments { get; set; }
        public RegistrationRequest RegistrationRequest { get; set; }
        public ICollection<PasswordReset> PasswordResets { get; set; }
        public ICollection<Excursion> Excursions { get; set; }
        public ICollection<ExcursionParticipant> ParticipatingInExcursions { get; set; }
    }
}