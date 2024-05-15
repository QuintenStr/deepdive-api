using deepdiveapi.Entities.Enum;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for updating the registration status of a user.
    /// </summary>
    public class UpdateStatusRegistrationRequestDto
    {
        public string UserId { get; set; }
        public RegistrationStatusEnum Status { get; set; }
    }
}
