using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for a simple string input, typically used for searches or single field submissions.
    /// </summary>
    public class StringInputDto
    {
        [Required]
        public string input {  get; set; }
    }
}
