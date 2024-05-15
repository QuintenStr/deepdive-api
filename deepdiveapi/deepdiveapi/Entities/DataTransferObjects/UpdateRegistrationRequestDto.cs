using deepdiveapi.Entities.Enum;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for updating a registration request with status and optional administrator comments.
    /// </summary>
    public class UpdateRegistrationRequestDto
    {
        public required int RequestId {  get; set; }
        public required RegistrationStatusEnum RegistrationStatus { get; set; }
        public string? AdminComment { get; set; }
    }
}
