using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for updating an excursion's details including title, description, and location coordinates.
    /// </summary>
    public class UpdateExcursionRequestDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        public string? ImageName { get; set; }

        [Required]
        public CoordinatesDto2 Coordinates { get; set; }
    }

    /// <summary>
    /// Nested DTO representing coordinates spatial data.
    /// </summary>
    public class CoordinatesDto2
    {
        [Required]
        public double Lat { get; set; }

        [Required]
        public double Long { get; set; }
    }
}
