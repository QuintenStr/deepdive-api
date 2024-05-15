namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO representing the response for a registration operation, indicating success or failure and providing details on any errors.
    /// </summary>
    public class RegistrationResponseDto
    {
        public bool IsSuccessfulRegistration { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
