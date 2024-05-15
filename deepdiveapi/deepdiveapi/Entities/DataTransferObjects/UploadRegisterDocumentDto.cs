using deepdiveapi.Entities.Enum;
using deepdiveapi.Entities.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for uploading a registration document, specifying the document type and associated user.
    /// </summary>
    public class UploadRegisterDocumentDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string DocumentName { get; set; }

        [Required]
        public string UserIdFK { get; set; }

        [Required]
        public RegistrationDocumentTypes DocumentType { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }
    }
}
