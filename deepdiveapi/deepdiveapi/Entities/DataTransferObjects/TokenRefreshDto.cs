namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for refreshing an authentication token.
    /// </summary>
    public class TokenRefreshDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}