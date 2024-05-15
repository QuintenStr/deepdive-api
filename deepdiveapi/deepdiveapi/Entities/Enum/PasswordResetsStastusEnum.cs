namespace deepdiveapi.Entities.Enum
{
    /// <summary>
    /// Represents the status of a password reset request.
    /// </summary>
    public enum PasswordResetsStastusEnum
    {
        /// <summary>
        /// The password reset has been requested but not completed.
        /// </summary>
        Requested = 0,

        /// <summary>
        /// The password has been changed following the reset request.
        /// </summary>
        PwdChanged = 1
    }
}

