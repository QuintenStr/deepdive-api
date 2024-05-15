using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using deepdiveapi.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities;
using deepdiveapi.Repositories.Interfaces;
using deepdiveapi.Entities.Enum;
using Serilog;

/// <summary>
/// Controller responsible for handling administrative tasks.
/// </summary>
[Route("[controller]")]
[Authorize(Roles = "Administrator", Policy = "EmailActivatedPolicy")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRegistrationDocumentRepository _registrationDocumentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="dbContext">Database context for accessing application data.</param>
    /// <param name="registrationDocumentRepository">Repository for managing registration documents.</param>
    public AdminController(ApplicationDbContext dbContext, IRegistrationDocumentRepository registrationDocumentRepository)
    {
        _dbContext = dbContext;
        _registrationDocumentRepository = registrationDocumentRepository;
    }

    /// <summary>
    /// Retrieves applications with requested status.
    /// </summary>
    /// <returns>List of application details with requested status.</returns>
    [HttpGet]
    [Route("ViewApplications")]
    public async Task<IActionResult> ViewApplicationsWithRequestedStatus()
    {
        await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                List<User> usersWithRequestedStatus = await _dbContext.Users
                    .Include(u => u.RegistrationRequest)
                    .Where(u => u.RegistrationRequest.RegistrationStatus == RegistrationStatusEnum.Requested)
                    .ToListAsync();

                List<ViewApplicationResponseDto> viewApplications = new List<ViewApplicationResponseDto>();

                foreach (var currentUser in usersWithRequestedStatus)
                {
                    var viewApp = new ViewApplicationResponseDto
                    {
                        Id = currentUser.RegistrationRequest.Id,
                        FirstName = currentUser.FirstName,
                        LastName = currentUser.LastName,
                        UserName = currentUser.UserName,
                        PhoneNumber = currentUser.PhoneNumber,
                        Email = currentUser.Email,
                        BirthDate = currentUser.BirthDate,
                        Status = currentUser.RegistrationRequest.RegistrationStatus,
                        AdminComment = currentUser.RegistrationRequest.AdminComment,
                        CreatedOn = currentUser.RegistrationRequest.CreatedOn,
                        EditedOn = currentUser.RegistrationRequest.EditedOn,
                        ApprovedOrDeniedOn = currentUser.RegistrationRequest.ApprovedOrDeniedOn,
                        DocumentsInfo = (await _registrationDocumentRepository.GetDocumentsOfUser(currentUser.Id))
                            .Select(doc => new
                            {
                                Id = doc.Id,
                                DocumentName = doc.DocumentName,
                                UserIdFK = doc.UserIdFK,
                                DocumentType = doc.DocumentType,
                                CreatedOn = doc.CreatedOn
                            })
                            .ToList<object>()
                    };

                    viewApplications.Add(viewApp);
                }

                await transaction.CommitAsync();

                Log.Information("Viewing applications with requested status completed successfully.");
                return Ok(viewApplications);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Error occurred while viewing applications with requested status.");
                return StatusCode(500, "An error occurred.");
            }
        }
    }

}