namespace deepdiveapi.Entities.Enum
{
    /// <summary>
    /// Represents the possible statuses of a registration request.
    /// </summary>
    public enum RegistrationStatusEnum
    {
        /// <summary>
        /// The request has been made but not yet processed.
        /// </summary>
        Requested = 0,

        /// <summary>
        /// The request is waiting for the user to make necessary changes.
        /// </summary>
        WaitingForUserChanges = 1,

        /// <summary>
        /// The request has been approved.
        /// </summary>
        Approved = 2,

        /// <summary>
        /// The request has been denied.
        /// </summary>
        Denied = 3
    }
}
