namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for displaying user details on a safe list, containing non-sensitive information.
    /// </summary>
    public class UserForSafeListDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public DateOnly Birthdate { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
