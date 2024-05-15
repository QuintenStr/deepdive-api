using deepdiveapi.Entities;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;
using deepdiveapi.JwtFeatures;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace deepdiveapi.Controllers
{
    /// <summary>
    /// Controller responsible for managing excursions.
    /// </summary>
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class ExcursionController : ControllerBase
    {
        private readonly IExcursionRepository _excursionRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcursionController"/> class.
        /// </summary>
        /// <param name="excursionRepository">Repository for managing excursions.</param>
        /// <param name="dbContext">Database context for accessing application data.</param>
        /// <param name="httpContextAccessor">Accessor for HttpContext.</param>
        /// <param name="userManager">Manager for handling user-related operations.</param>
        public ExcursionController(IExcursionRepository excursionRepository, ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _excursionRepository = excursionRepository;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves a list of excursions.
        /// </summary>
        /// <returns>List of excursions.</returns>
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetExcursionsList()
        {
            try
            {
                var excursions = await _excursionRepository.GetExcursions();
                var excursionData = excursions.Select(excursion => new
                {
                    excursion.Id,
                    excursion.Title,
                    excursion.Description,
                    excursion.DateTime,
                    excursion.ImageName
                }).ToList();

                Log.Information("Excursion data retrieved successfully.");
                return Ok(excursionData);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving excursion data.");
                return StatusCode(500, "An error occurred.");
            }
        }

        /// <summary>
        /// Retrieves details of a specific excursion.
        /// </summary>
        /// <param name="input">DTO containing the ID of the excursion.</param>
        /// <returns>Details of the excursion.</returns>
        [HttpPost]
        [Route("Details")]
        public async Task<IActionResult> GetExcursionDetails(IdInputDto input)
        {
            try
            {
                var excursion = await _excursionRepository.GetExcursionById(input.Id);
                if (excursion == null)
                {
                    Log.Warning("Excursion not found with ID: ", input.Id);
                    return BadRequest(new ErrorResponseDto { Errors = new[] { "Can't find excursion." } });
                }
                var user = await _userManager.FindByIdAsync(excursion.CreatedByUserFK);

                var excursionDto = new ExcursionDetailsDto
                {
                    Id = excursion.Id,
                    Title = excursion.Title,
                    Description = excursion.Description,
                    CreatedOn = excursion.CreatedOn,
                    DateTime = excursion.DateTime,
                    ImageName = excursion.ImageName,
                    CreatedByUser = MapUserDto(user),
                    Coordinates = new CoordinatesDto
                    {
                        Lat = excursion.Location.Y,
                        Long = excursion.Location.X
                    },
                    Participants = excursion.Participants.Select(p => MapParticipantDto(p.User)).ToList()
                };
                Log.Information("Excursion details retrieved successfully for excursion with ID: ", input.Id);
                return Ok(excursionDto);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving excursion details for ID: ", input.Id);
                return StatusCode(500, "An error occurred.");
            }
        }

        /// <summary>
        /// Private method to map user to userdto.
        /// </summary>
        /// <param name="input">User object.</param>
        /// <returns>UserDto object.</returns>
        private UserDto? MapUserDto(User user) =>
            user != null ? new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName
            } : null;

        /// <summary>
        /// Private method to map user to participantdto.
        /// </summary>
        /// <param name="input">User object.</param>
        /// <returns>ParticipantDto object.</returns>
        private ParticipantDto? MapParticipantDto(User user) =>
            user != null ? new ParticipantDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName
            } : null;

        /// <summary>
        /// Adds a new excursion.
        /// </summary>
        /// <param name="input">DTO containing details of the new excursion.</param>
        /// <returns>ID of the newly created excursion.</returns>
        [HttpPost]
        [Route("AddExcursion")]
        public async Task<IActionResult> AddExcursion(NewExcursionDto input)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var userId = JwtHandler.GetUserIdFromToken(_httpContextAccessor.HttpContext.Request);
                    if (userId == null)
                    {
                        Log.Warning("User ID not found in token.");
                        return Unauthorized();
                    }
                    string createdExcursionId = await _excursionRepository.AddNewExcursion(input, userId);
                    await transaction.CommitAsync();
                    IdOutputDto output = new IdOutputDto()
                    {
                        id = createdExcursionId
                    };

                    Log.Information("New excursion added successfully by user ID: ", userId);
                    return Ok(output);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Log.Error(ex, "An error occurred while adding new excursion.");
                    return StatusCode(500, "Internal server error");
                }
            }
        }

        /// <summary>
        /// Deletes an excursion.
        /// </summary>
        /// <param name="input">DTO containing the ID of the excursion to be deleted.</param>
        /// <returns>Ok if excursion is deleted successfully, BadRequest if excursion not found, or internal server error occurs.</returns>
        [HttpDelete]
        [Route("Remove")]
        public async Task<IActionResult> DeleteExcursion([FromBody] IdInputDto input)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await _excursionRepository.DeleteExcursion(input.Id);
                    await transaction.CommitAsync();
                    Log.Information("Excursion deleted successfully for ID: ", input.Id);
                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Log.Error(ex, "An error occurred while deleting excursion for ID: ", input.Id);
                    return StatusCode(500, "Internal server error");
                }
            }
        }

        /// <summary>
        /// Updates details of an existing excursion.
        /// </summary>
        /// <param name="input">DTO containing details to be updated.</param>
        /// <returns>Ok if excursion details are updated successfully, BadRequest if excursion not found, or internal server error occurs.</returns>
        [HttpPatch]
        [Route("Update")]
        public async Task<IActionResult> UpdateStatus(UpdateExcursionRequestDto input)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var excursion = await _excursionRepository.GetExcursionById(input.Id);
                    if (excursion != null)
                    {
                        excursion.Title = input.Title;
                        excursion.Description = input.Description;
                        excursion.DateTime = input.DateTime;
                        excursion.Location = new Point(input.Coordinates.Long, input.Coordinates.Lat) { SRID = 4326 };

                        if (input.ImageName != null)
                        {
                            excursion.ImageName = input.ImageName;
                        }

                        await _excursionRepository.UpdateExcursionAsync(excursion);
                        await transaction.CommitAsync();
                        Log.Information("Excursion updated successfully for ID: ", input.Id);
                        return Ok();
                    }
                    else
                    {
                        Log.Warning("Excursion not found for ID: ", input.Id);
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Log.Error(ex, "An error occurred while updating excursion for ID: ", input.Id);
                    return StatusCode(500, "Internal server error");
                }
            }
        }
    }
}
