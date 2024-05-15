using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Enum;
using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UnitTest_Project.Entities
{
    [TestClass]
    public class UTRegisterDocument
    {
        private static DbContext _dbContext = new DbContext("DB_RegisterDocument");

        public void AssemblyInit()
        {

            _dbContext.AppDBContext.Database.EnsureDeleted();
            _dbContext.AppDBContext.UserRegisterDocuments.AddRange(
            new UserRegisterDocument { Id = "id1", DocumentName = "Demo.pdf", UserIdFK = "userId1", DocumentType = (RegistrationDocumentTypes)0, CreatedOn = DateTime.Now },
            new UserRegisterDocument { Id = "id5", DocumentName = "MedicalAttest.pdf", UserIdFK = "userId99", DocumentType = (RegistrationDocumentTypes)1, CreatedOn = DateTime.Now.AddYears(-2).AddMonths(4).AddDays(9) },
            new UserRegisterDocument { Id = "id6", DocumentName = "ID.pdf", UserIdFK = "userId99", DocumentType = (RegistrationDocumentTypes)1, CreatedOn = DateTime.Now.AddYears(-8).AddMonths(3).AddDays(28) }
            );

            _dbContext.AppDBContext.SaveChanges();
        }

        public void DestroyAssesmblyAfterTest()
        {
            _dbContext.AppDBContext.UserRegisterDocuments.RemoveRange(_dbContext.AppDBContext.UserRegisterDocuments);
            _dbContext.AppDBContext.SaveChanges();
        }

        [TestMethod]
        public async Task TestAddDocument()
        {
            AssemblyInit();
            RegistrationDocumentRepository registrationDocumentRepository = new RegistrationDocumentRepository(_dbContext.AppDBContext);

            // Act
            UploadRegisterDocumentDto newDoc = new UploadRegisterDocumentDto { Id = "id2", DocumentName = "MedischeAttest.png", UserIdFK = "userId2", DocumentType = (RegistrationDocumentTypes)1, CreatedOn = DateTime.Now.AddYears(1).AddMonths(5).AddDays(15) };
            await registrationDocumentRepository.AddUserDocumentAsync(newDoc);

            // Assert
            Assert.AreEqual(4, _dbContext.AppDBContext.UserRegisterDocuments.Count());
            DestroyAssesmblyAfterTest();
        }

        [TestMethod]
        public async Task TestGetDocumentsOfUser()
        {
            AssemblyInit();
            RegistrationDocumentRepository registrationDocumentRepository = new RegistrationDocumentRepository(_dbContext.AppDBContext);

            // Act
            List<UserRegisterDocument> userRegisterDocuments = await registrationDocumentRepository.GetDocumentsOfUser("userId99");

            // Assert
            Assert.AreEqual(2, userRegisterDocuments.Count);
            DestroyAssesmblyAfterTest();
        }

        [TestMethod]
        public async Task TestDeleteDocument()
        {
            AssemblyInit();
            RegistrationDocumentRepository registrationDocumentRepository = new RegistrationDocumentRepository(_dbContext.AppDBContext);

            // Act
            await registrationDocumentRepository.DeleteDocumentAsync("id5", "userId99");

            // Assert
            Assert.AreEqual(2, _dbContext.AppDBContext.UserRegisterDocuments.Count());
            DestroyAssesmblyAfterTest();
        }
    }
}
