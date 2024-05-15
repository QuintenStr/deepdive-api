using deepdiveapi.Entities;
using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace deepdiveapi.Repositories
{
    /// <summary>
    /// The RegistrationDocumentRepository class manages operations related to user registration documents.
    /// </summary>
    public class RegistrationDocumentRepository : IRegistrationDocumentRepository
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Constructor for RegistrationDocumentRepository.
        /// </summary>
        /// <param name="dbContext">Application database context.</param>
        public RegistrationDocumentRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a new document to a user's profile.
        /// </summary>
        /// <param name="input">Data transfer object containing information about the document to add.</param>
        public async Task AddUserDocumentAsync(UploadRegisterDocumentDto input)
        {
            var userDocument = new UserRegisterDocument
            {
                Id = input.Id,
                DocumentName = input.DocumentName,
                UserIdFK = input.UserIdFK,
                DocumentType = input.DocumentType,
                CreatedOn = input.CreatedOn
            };

            _dbContext.UserRegisterDocuments.Add(userDocument);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves all documents associated with a specific user.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve documents for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of UserRegisterDocument entities.</returns>
        public async Task<List<UserRegisterDocument>> GetDocumentsOfUser(string id)
        {
            return await _dbContext.UserRegisterDocuments
                                    .Where(doc => doc.UserIdFK == id)
                                    .ToListAsync();
        }

        /// <summary>
        /// Deletes a specific document associated with a user.
        /// </summary>
        /// <param name="documentId">The ID of the document to delete.</param>
        /// <param name="userId">The ID of the user the document is associated with.</param>
        public async Task DeleteDocumentAsync(string documentId, string userId)
        {
            var document = await _dbContext.UserRegisterDocuments
                .FirstOrDefaultAsync(doc => doc.Id == documentId && doc.UserIdFK == userId);

            if (document != null)
            {
                _dbContext.UserRegisterDocuments.Remove(document);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
