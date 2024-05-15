namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for inputting an email address, typically for sending emails or verifying email-related operations.
    /// </summary>
    public class EmailInputDto
    {
        public required string Email { get; set; }
    }
}
