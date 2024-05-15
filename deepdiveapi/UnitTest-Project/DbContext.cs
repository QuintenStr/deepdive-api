using deepdiveapi.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents a wrapper for the application database context used in unit tests.
/// </summary>
namespace UnitTest_Project
{
    /// <summary>
    /// Provides methods to create an in-memory database context for unit testing purposes.
    /// </summary>
    public class DbContext
    {
        /// <summary>
        /// Gets or sets the application database context.
        /// </summary>
        public ApplicationDbContext AppDBContext { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContext"/> class with the specified database name.
        /// </summary>
        /// <param name="nameDB">The name of the in-memory database.</param>
        public DbContext(string nameDB)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: nameDB)
                .Options;

            ApplicationDbContext context = new ApplicationDbContext(options);
            AppDBContext = context;
        }
    }
}
