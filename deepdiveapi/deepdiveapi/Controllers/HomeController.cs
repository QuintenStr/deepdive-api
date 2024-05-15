using deepdiveapi.Entities.Models;
using deepdiveapi.Entities;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using deepdiveapi.Entities.DataTransferObjects;

namespace deepdiveapi.Controllers
{
    /// <summary>
    /// Controller responsible for handling home-related requests.
    /// </summary>
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IExcursionRepository _excursionRepository;
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="excursionRepository">Repository for accessing excursion-related data.</param>
        /// <param name="dbContext">Database context for accessing application data.</param>
        public HomeController(IExcursionRepository excursionRepository, ApplicationDbContext dbContext)
        {
            _excursionRepository = excursionRepository;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves the home dashboard data for the specified user.
        /// </summary>
        /// <param name="input">DTO containing the user ID for which the dashboard data is requested.</param>
        /// <returns>Ok with the home dashboard data if successful, or InternalServerError on exception.</returns>
        [HttpPost]
        [Route("Dashboard")]
        public async Task<IActionResult> GetHomeDashboard(IdInputDto input)
        {
            await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<Excursion> allExcursions = await _excursionRepository.GetExcursions();
                    int totalExcursionsCount = allExcursions.Count();
                    int upcomingExcursions = allExcursions.Where(x => x.DateTime >= DateTime.UtcNow).Count();

                    var (pastExcursions, futureExcursions) = await _excursionRepository.GetUserExcursions(input.Id);

                    List<Excursion> randomExcursions = allExcursions
                        .OrderBy(x => Guid.NewGuid())
                        .Take(3)
                        .ToList();

                    List<ExcursionInfo> randomExcursionInfos = randomExcursions.Select(e => new ExcursionInfo
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        ImageName = e.ImageName
                    }).ToList();

                    HomeDashboardDto homeDashboardDto = new HomeDashboardDto()
                    {
                        TotalExcursions = totalExcursionsCount,
                        UpcomingExcursions = upcomingExcursions,
                        ParticipatedInExcursions = pastExcursions.Count(),
                        GoingToParticipateInExcursions = futureExcursions.Count(),
                        RandomExcursionInfos = randomExcursionInfos
                    };

                    await transaction.CommitAsync();
                    return Ok(homeDashboardDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "An error occurred.");
                }
            }
        }
    }
}
