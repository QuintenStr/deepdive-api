namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for passing a document ID, typically used in operations involving document retrieval or manipulation.
    /// </summary>
    public class DocuIdInput
    {
        public required string DocumentId { get; set; }
    }
}
