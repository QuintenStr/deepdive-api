namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for encapsulating error responses from API calls.
    /// </summary>
    public class ErrorResponseDto
    {
        public IEnumerable<string>? Errors { get; set; }
    }
}
