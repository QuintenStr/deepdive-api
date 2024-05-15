using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using deepdiveapi.Entities.Enum;

namespace deepdiveapi.Entities.Models
{
    /// <summary>
    /// Represents a password reset request.
    /// </summary>
    public class PasswordReset
    {
        /// <summary>
        /// Gets or sets the unique identifier for the password reset request.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the token associated with the password reset.
        /// </summary>
        [Required]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the foreign key of the user associated with this password reset.
        /// </summary>
        [Required]
        public string UserIdFK { get; set; }

        /// <summary>
        /// Navigation property for the user associated with this password reset.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the password reset was created.
        /// </summary>
        [Required]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time for the password reset token.
        /// </summary>
        [Required]
        public DateTime ExpireOn { get; set; }

        /// <summary>
        /// Gets or sets the status of the password reset process.
        /// </summary>
        [Required]
        public PasswordResetsStastusEnum Status { get; set; }
    }
}
