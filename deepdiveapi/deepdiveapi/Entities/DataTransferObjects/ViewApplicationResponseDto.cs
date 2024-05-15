using deepdiveapi.Entities.Enum;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for viewing application responses with user and application status details.
    /// </summary>
    public class ViewApplicationResponseDto
    {
        public required int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public required DateOnly BirthDate { get; set; }
        public required RegistrationStatusEnum Status { get; set; }
        public string? AdminComment { get; set; }
        public required DateTime CreatedOn { get; set; }
        public DateTime? EditedOn { get; set; }
        public DateTime? ApprovedOrDeniedOn { get; set; }
        public List<object> DocumentsInfo { get; set; }
    }
}
