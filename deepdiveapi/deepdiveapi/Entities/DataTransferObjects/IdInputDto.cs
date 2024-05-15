using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for inputting an ID.
    /// </summary>
    public class IdInputDto
    {
        [Required]
        public required string Id { get; set; }
    }
}