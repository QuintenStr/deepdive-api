using Microsoft.AspNetCore.Authorization;

namespace deepdiveapi.Entities.Verification.Email
{
    /// <summary>
    /// Represents an authorization requirement that checks if a user's email is activated.
    /// </summary>
    public class EmailActivatedRequirement : IAuthorizationRequirement
    {
    }
}