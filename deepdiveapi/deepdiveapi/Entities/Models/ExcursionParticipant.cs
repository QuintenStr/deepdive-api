using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.Models
{
    /// <summary>
    /// Represents a participant in an excursion.
    /// </summary>
    public class ExcursionParticipant
    {
        /// <summary>
        /// Gets or sets the unique identifier for the excursion participant.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key for the excursion this participant is registered to.
        /// </summary>
        [Required]
        public string ExcursionId { get; set; }

        /// <summary>
        /// Navigation property for the excursion associated with this participant.
        /// </summary>
        [ForeignKey(nameof(ExcursionId))]
        public Excursion Excursion { get; set; }

        /// <summary>
        /// Gets or sets the foreign key for the user who is participating in the excursion.
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Navigation property for the user associated with this participant.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
