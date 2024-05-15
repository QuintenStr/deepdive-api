using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for creating a new excursion, including required information such as title, description, date/time, and location coordinates.
    /// </summary>
    public class NewExcursionDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        [Required]
        public string ImageName { get; set; }
        [Required]
        public CoordinatesDto Coordinates { get; set; }
    }
}
