namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for constructing an email with subject, content, recipients, and attachments.
    /// </summary>
    public class EmailModelDto
    {
        public string Subject { get; set; }

        public string PlainTextContent { get; set; }
        public string HtmlContent { get; set; }

        public List<string> toRecipients { get; set; }
        public List<string> ccRecipients { get; set; }
        public List<string> bccRecipients { get; set; }

        public List<EmailAttachmentBinaryBypass> Attachments { get; set; }
    }
}
