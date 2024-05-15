using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using deepdiveapi.Entities.Enum;
using deepdiveapi.Entities.Validation;

namespace deepdiveapi.Entities.Models
{
    /// <summary>
    /// Represents a request for user registration status change.
    /// </summary>
    public class RegistrationRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier for the registration request.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key of the user associated with this request.
        /// </summary>
        [Required]
        public string UserIdFK { get; set; }

        /// <summary>
        /// Navigation property for the user associated with this request.
        /// </summary>
        [ForeignKey(nameof(UserIdFK))]
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the current status of the registration request.
        /// </summary>
        [Required]
        [EnumDataType(typeof(RegistrationStatusEnum))]
        public RegistrationStatusEnum RegistrationStatus { get; set; }

        /// <summary>
        /// Optional comment by an administrator concerning the request.
        /// </summary>
        public string? AdminComment { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the request was created.
        /// </summary>
        [Required]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Optional date and time when the request was last edited.
        /// </summary>
        [LaterThan(nameof(CreatedOn))]
        public DateTime? EditedOn { get; set; }

        /// <summary>
        /// Optional date and time when the request was approved or denied.
        /// </summary>
        [LaterThan(nameof(CreatedOn))]
        public DateTime? ApprovedOrDeniedOn { get; set; }
    }

}
