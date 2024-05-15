using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace deepdiveapi.Entities.Models
{
    /// <summary>
    /// Represents an excursion, typically a guided tour or trip.
    /// </summary>
    public class Excursion
    {
        /// <summary>
        /// Gets or sets the unique identifier for the excursion.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the excursion.
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the excursion.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the foreign key for the user who created the excursion.
        /// </summary>
        [Required]
        public string? CreatedByUserFK { get; set; }

        /// <summary>
        /// Navigation property for the user who created the excursion.
        /// </summary>
        [ForeignKey(nameof(CreatedByUserFK))]
        public User CreatedByUser { get; set; }

        /// <summary>
        /// Gets or sets the creation date and time for the excursion.
        /// </summary>
        [Required]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the scheduled date and time for the excursion.
        /// </summary>
        [Required]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the geographic location of the excursion using spatial data.
        /// </summary>
        [Required]
        public Point Location { get; set; }

        /// <summary>
        /// Gets or sets the image name associated with the excursion.
        /// </summary>
        [Required]
        public string ImageName { get; set; }

        /// <summary>
        /// Navigation property for the participants of the excursion.
        /// </summary>
        public ICollection<ExcursionParticipant> Participants { get; set; }
    }
}
