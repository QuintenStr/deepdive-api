using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using deepdiveapi.Entities.Validation;

namespace deepdiveapi.Entities.Models
{
    /// <summary>
    /// Represents a refresh token for maintaining user authentication sessions.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Gets or sets the unique identifier for the refresh token.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the token string.
        /// </summary>
        [Required]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time for the token.
        /// </summary>
        [Required]
        public DateTime Expires { get; set; }

        /// <summary>
        /// Determines if the token is expired.
        /// </summary>
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= Expires;

        /// <summary>
        /// Gets or sets the creation date and time for the token.
        /// </summary>
        [Required]
        public DateTime Created { get; set; }

        /// <summary>
        /// Optional date and time when the token was revoked.
        /// </summary>
        [LaterThan(nameof(Created))]
        public DateTime? Revoked { get; set; }

        /// <summary>
        /// Optional token that replaces this one.
        /// </summary>
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// Determines if the token is active and not expired or revoked.
        /// </summary>
        [NotMapped]
        public bool IsActive => Revoked == null && !IsExpired;

        /// <summary>
        /// Gets or sets the foreign key of the user who owns this token.
        /// </summary>

        public string UserIdFK { get; set; }

        /// <summary>
        /// Navigation property for the user associated with this token.
        /// </summary>
        public User User { get; set; }
    }
}
