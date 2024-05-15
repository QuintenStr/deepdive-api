namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO representing the response for authentication operations, detailing success, errors, and token data.
    /// </summary>
    public class AuthResponseDto
    {
        public bool IsAuthSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Token { get; set; }
        public string?  RefreshToken { get; set; }
    }
}
