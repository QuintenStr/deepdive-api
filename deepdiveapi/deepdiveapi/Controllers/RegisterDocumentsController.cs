using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using deepdiveapi.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Repositories.Interfaces;
using System.Security.Claims;
using Serilog;

/// <summary>
/// Controller responsible for managing user registration documents.
/// </summary>
[Route("[controller]")]
[Authorize]
[ApiController]
public class RegisterDocumentsController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IRegistrationDocumentRepository _registrationDocumentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterDocumentsController"/> class.
    /// </summary>
    /// <param name="userManager">User manager for user-related operations.</param>
    /// <param name="registrationDocumentRepository">Repository for accessing and managing registration documents.</param>
    public RegisterDocumentsController(UserManager<User> userManager, IRegistrationDocumentRepository registrationDocumentRepository)
    {
        _userManager = userManager;
        _registrationDocumentRepository = registrationDocumentRepository;
    }

    /// <summary>
    /// Uploads a document for user registration.
    /// </summary>
    /// <param name="input">DTO containing information about the document to upload.</param>
    /// <returns>Ok if the document is uploaded successfully, or InternalServerError on exception.</returns>
    [HttpPost]
    [Route("UploadDocument")]
    public async Task<IActionResult> UploadDocument([FromBody] UploadRegisterDocumentDto input)
    {
        try
        {
            await _registrationDocumentRepository.AddUserDocumentAsync(input);
            Log.Information("User document added successfully.");
            return Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while adding user document.");
            return StatusCode(500, "An error occurred.");
        }
    }

    /// <summary>
    /// Deletes a document associated with a user.
    /// </summary>
    /// <param name="input">DTO containing the ID of the document to delete.</param>
    /// <returns>Ok if the document is deleted successfully, BadRequest if the user is not found, or InternalServerError on exception.</returns>
    [HttpDelete]
    [Route("Remove")]
    public async Task<IActionResult> DeleteDocument([FromBody] DocuIdInput input)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                Log.Warning("User not found with ID: ", userId);
                return BadRequest(new ErrorResponseDto { Errors = new[] { "User not found." } });
            }
            await _registrationDocumentRepository.DeleteDocumentAsync(input.DocumentId, user.Id);
            Log.Information("Document deleted successfully by user: " + userId + ", DocumentId: ", input.DocumentId);
            return Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while deleting document.");
            return StatusCode(500, "Internal server error");
        }
    }
}