using deepdiveapi.Entities;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;
using deepdiveapi.JwtFeatures;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace deepdiveapi.Controllers
{
    /// <summary>
    /// Controller responsible for managing participants of excursions.
    /// </summary>
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class ExcursionParticipantsController : ControllerBase
    {
        private readonly IExcursionParticipantsRepository _excursionParticipantsRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcursionParticipantsController"/> class.
        /// </summary>
        /// <param name="excursionParticipantsRepository">Repository for managing excursion participants.</param>
        /// <param name="dbContext">Database context for accessing application data.</param>
        /// <param name="httpContextAccessor">Accessor for HttpContext.</param>
        /// <param name="userManager">Manager for handling user-related operations.</param>
        public ExcursionParticipantsController(IExcursionParticipantsRepository excursionParticipantsRepository, ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _excursionParticipantsRepository = excursionParticipantsRepository;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        /// <summary>
        /// Adds a participant to an excursion.
        /// </summary>
        /// <param name="input">DTO containing the participant and excursion IDs.</param>
        /// <returns>Ok if participant is added successfully, BadRequest if user not found or internal server error occurs.</returns>
        [HttpPut]
        [Route("Add")]
        public async Task<IActionResult> AddParticipant(ExcursionParticipantDto input)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var userId = JwtHandler.GetUserIdFromToken(_httpContextAccessor.HttpContext.Request);
                    if (userId == null)
                    {
                        return BadRequest();
                    }
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return BadRequest();
                    }
                    if (userId == input.UserId)
                    {
                        await _excursionParticipantsRepository.AddParticipantToExcursion(input.ExcursionId, input.UserId);
                    }
                    else
                    {
                        // if user that makes request is in admin, he can add any user
                        if (!await _userManager.IsInRoleAsync(user, "Administrator"))
                        {
                            return BadRequest();
                        }
                        // send mail with bijlage
                        await _excursionParticipantsRepository.AddParticipantToExcursion(input.ExcursionId, input.UserId);
                    }
                    await transaction.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Internal server error");
                }
            }
        }

        /// <summary>
        /// Adds multiple participants to an excursion.
        /// </summary>
        /// <param name="input">DTO containing the excursion ID and list of participant IDs.</param>
        /// <returns>Ok if participants are added successfully, BadRequest if user not found or internal server error occurs.</returns>
        [HttpPut]
        [Authorize(Roles = "Administrator")]
        [Route("AddMultiple")]
        public async Task<IActionResult> AddMultipleParticipant(ExcursionMultipleParticipantsDto input)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var userId = JwtHandler.GetUserIdFromToken(_httpContextAccessor.HttpContext.Request);
                    if (userId == null)
                    {
                        return BadRequest();
                    }
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return BadRequest();
                    }
                    await _excursionParticipantsRepository.AddMultipleParticipantsToExcursion(input.ExcursionId, input.UserId);
                    await transaction.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Internal server error");
                }
            }
        }

        /// <summary>
        /// Removes a participant from an excursion.
        /// </summary>
        /// <param name="input">DTO containing the participant and excursion IDs.</param>
        /// <returns>Ok if participant is removed successfully, BadRequest if user not found or internal server error occurs.</returns>
        [HttpDelete]
        [Route("Remove")]
        public async Task<IActionResult> RemoveParticipant(ExcursionParticipantDto input)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var userId = JwtHandler.GetUserIdFromToken(_httpContextAccessor.HttpContext.Request);
                    if (userId == null)
                    {
                        return BadRequest();
                    }
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return BadRequest();
                    }
                    if (userId == input.UserId)
                    {
                        await _excursionParticipantsRepository.RemoveParticipantFromExcursion(input.ExcursionId, input.UserId);
                    }
                    else
                    {
                        if (!await _userManager.IsInRoleAsync(user, "Administrator"))
                        {
                            return BadRequest();
                        }
                        await _excursionParticipantsRepository.RemoveParticipantFromExcursion(input.ExcursionId, input.UserId);
                    }
                    await transaction.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Internal server error");
                }
            }
        }
    }
}
