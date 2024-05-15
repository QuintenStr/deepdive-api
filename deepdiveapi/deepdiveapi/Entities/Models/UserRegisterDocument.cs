using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using deepdiveapi.Entities.Enum;

namespace deepdiveapi.Entities.Models
{
    /// <summary>
    /// Represents a document info registered by a user.
    /// </summary>
    public class UserRegisterDocument
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user register document.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the document. Must equal the name of the file in Azure Blob Storage.
        /// </summary>
        [Required]
        public string DocumentName { get; set; }

        /// <summary>
        /// Gets or sets the foreign key for the user who owns this document.
        /// </summary>
        [Required]
        public string UserIdFK { get; set; }

        /// <summary>
        /// Navigation property for the user associated with this document.
        /// </summary>
        [ForeignKey(nameof(UserIdFK))]
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the type of the document.
        /// </summary>
        [Required]
        [EnumDataType(typeof(RegistrationDocumentTypes))]
        public RegistrationDocumentTypes DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the document was created.
        /// </summary>
        [Required]
        public DateTime CreatedOn { get; set; }
    }

}
