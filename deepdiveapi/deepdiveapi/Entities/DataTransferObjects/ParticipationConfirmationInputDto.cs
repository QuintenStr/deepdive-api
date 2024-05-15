using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO for confirming participation in an event, including the event time, location, and a QR code key for validation.
    /// </summary>
    public class ParticipationConfirmationInputDto
    {
        [Required]
        public DateTime DateTime { get; set; }

        [Required]
        public CoordinatesDto Coordinates { get; set; }

        [Required]
        public string QrCodeKey { get; set; }
    }
}
