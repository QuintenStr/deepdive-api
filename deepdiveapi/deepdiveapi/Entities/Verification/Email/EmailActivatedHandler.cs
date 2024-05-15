using deepdiveapi.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace deepdiveapi.Entities.Verification.Email
{
    /// <summary>
    /// Handles the authorization requirement checking for a user's email activation status.
    /// </summary>
    public class EmailActivatedHandler : AuthorizationHandler<EmailActivatedRequirement>
    {
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="EmailActivatedHandler"/> class.
        /// </summary>
        /// <param name="userManager">User manager to access user information.</param>
        public EmailActivatedHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Processes the authorization request to determine if the user has an activated email.
        /// </summary>
        /// <param name="context">Authorization handling context.</param>
        /// <param name="requirement">The authorization requirement to evaluate.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmailActivatedRequirement requirement)
        {
            var user = await _userManager.GetUserAsync(context.User);

            if (user != null && user.EmailConfirmed)
            {
                context.Succeed(requirement);
            }

            await Task.CompletedTask;
        }
    }
}
