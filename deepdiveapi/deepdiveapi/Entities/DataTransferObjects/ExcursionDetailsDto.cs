namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO that provides detailed information about an excursion, including details about the creator and participants.
    /// </summary>
    public class ExcursionDetailsDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public UserDto CreatedByUser { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime DateTime { get; set; }
        public string ImageName { get; set; }
        public CoordinatesDto Coordinates { get; set; }
        public List<ParticipantDto> Participants { get; set; }
    }

    /// <summary>
    /// Nested DTO representing basic user information within an excursion context.
    /// </summary>
    public class UserDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    /// <summary>
    /// Nested DTO representing coordinates spatial data.
    /// </summary>
    public class CoordinatesDto
    {
        public double Lat { get; set; }
        public double Long { get; set; }
    }

    /// <summary>
    /// Nested DTO representing detailed participant information.
    /// </summary>
    public class ParticipantDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
    }
}
