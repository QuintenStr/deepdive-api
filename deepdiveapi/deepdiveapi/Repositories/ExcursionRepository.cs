using deepdiveapi.Entities;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace deepdiveapi.Repositories
{
    /// <summary>
    /// The ExcursionRepository class manages operations related to excursions.
    /// </summary>
    public class ExcursionRepository : IExcursionRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for ExcursionRepository.
        /// </summary>
        /// <param name="context">Application database context.</param>
        public ExcursionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all excursions.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of Excursion entities.</returns>
        public async Task<List<Excursion>> GetExcursions()
        {
            return _context.Excursions.ToList();
        }

        /// <summary>
        /// Retrieves an excursion by its ID, including its participants and their user details.
        /// </summary>
        /// <param name="input">The ID of the excursion to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an Excursion entity, or null if not found.</returns>
        public async Task<Excursion> GetExcursionById(string input)
        {
            return await _context.Excursions
                .Include(e => e.Participants) // Include the participants
                    .ThenInclude(p => p.User) // Include the user for each participant
                .FirstOrDefaultAsync(x => x.Id == input);
        }

        /// <summary>
        /// Creates a new excursion and automatically adds the creator as a participant.
        /// </summary>
        /// <param name="input">Data transfer object containing information about the new excursion.</param>
        /// <param name="userId">The ID of the user creating the excursion.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the newly created excursion.</returns>
        public async Task<string> AddNewExcursion(NewExcursionDto input, string userId)
        {
            string excursionId = Guid.NewGuid().ToString();

            Excursion newEntry = new Excursion()
            {
                Id = excursionId,
                Title = input.Title,
                Description = input.Description,
                DateTime = input.DateTime,
                CreatedOn = DateTime.UtcNow,
                CreatedByUserFK = userId,
                ImageName = input.ImageName,
                Location = new Point(input.Coordinates.Long, input.Coordinates.Lat) { SRID = 4326 }
            };

            // add creator automatically to participant of new excursion
            ExcursionParticipant newParticipant = new ExcursionParticipant()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                ExcursionId = excursionId,
            };

            _context.Excursions.Add(newEntry);
            _context.ExcursionParticipants.Add(newParticipant);
            await _context.SaveChangesAsync();
            return excursionId;
        }

        /// <summary>
        /// Deletes an excursion by its ID.
        /// </summary>
        /// <param name="id">The ID of the excursion to delete.</param>
        public async Task DeleteExcursion(string id)
        {
            var excursion = await _context.Excursions.FirstOrDefaultAsync(exc => exc.Id == id);

            if (excursion != null)
            {
                _context.Excursions.Remove(excursion);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Updates an existing excursion.
        /// </summary>
        /// <param name="excursion">The excursion entity to update.</param>
        public async Task UpdateExcursionAsync(Excursion excursion)
        {
            _context.Entry(excursion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves past and future excursions for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve excursions for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains two lists of Excursion entities, one for past and one for future excursions.</returns>
        public async Task<(List<Excursion> PastExcursions, List<Excursion> FutureExcursions)> GetUserExcursions(string userId)
        {
            var nowUtc = DateTime.UtcNow;

            var excursions = await _context.ExcursionParticipants.Include(ep => ep.Excursion)
                .Where(ep => ep.UserId == userId)
                .Select(ep => ep.Excursion)
                .ToListAsync();

            var pastExcursions = excursions
                .Where(e => e.DateTime < nowUtc)
                .ToList();

            var futureExcursions = excursions
                .Where(e => e.DateTime >= nowUtc)
                .ToList();

            return (PastExcursions: pastExcursions, FutureExcursions: futureExcursions);
        }
    }
}
