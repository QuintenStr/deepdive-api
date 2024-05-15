using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;

namespace deepdiveapi.Repositories.Interfaces
{
    /// <summary>
    /// The IRegistrationDocumentRepository interface defines methods for managing registration documents.
    /// </summary>
    public interface IRegistrationDocumentRepository
    {
        public Task AddUserDocumentAsync(UploadRegisterDocumentDto input);
        public Task<List<UserRegisterDocument>> GetDocumentsOfUser(string id);
        public Task DeleteDocumentAsync(string docuId, string userId);
    }
}
