namespace deepdiveapi.Entities.DataTransferObjects
{
    /// <summary>
    /// DTO representing the response from a user update operation, indicating success or failure and any errors.
    /// </summary>
    public class UpdateUserResponseDto
    {
        public bool IsSuccessfullUpdate { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
