using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for outputting an ID.
    /// </summary>
    public class IdOutputDto
    {
        [Required]
        public required string id { get; set; }
    }
}
