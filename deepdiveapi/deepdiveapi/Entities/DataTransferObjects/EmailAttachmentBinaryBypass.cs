namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for handling binary content of an email attachment, specifying the file name and media type.
    /// </summary>
    public class EmailAttachmentBinaryBypass
    {
        public string FileName { get; set; }
        public string MediaType { get; set; }
        public byte[] Content { get; set; }

        public EmailAttachmentBinaryBypass(string fileName, string mediaType, byte[] content)
        {
            FileName = fileName;
            MediaType = mediaType;
            Content = content;
        }
    }
}
