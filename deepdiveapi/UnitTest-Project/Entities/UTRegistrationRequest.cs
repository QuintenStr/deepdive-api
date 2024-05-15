using deepdiveapi.Entities.Enum;
using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories;
using deepdiveapi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest_Project.Entities
{
    [TestClass]
    public class UTRegistrationRequest
    {
        private static DbContext _dbContext = new DbContext("DB_RegisterRequests");
        public void AssemblyInit()
        {
            _dbContext.AppDBContext.RegistrationRequests.AddRange(
                new RegistrationRequest { Id = 22, UserIdFK = "userId95", RegistrationStatus = (RegistrationStatusEnum)0, AdminComment = "", CreatedOn = DateTime.Now.AddYears(1).AddDays(173), EditedOn = null, ApprovedOrDeniedOn = null },
                new RegistrationRequest { Id = 23, UserIdFK = "userId92", RegistrationStatus = (RegistrationStatusEnum)3, AdminComment = "User is already banned", CreatedOn = DateTime.Now.AddYears(-8).AddDays(56), EditedOn = DateTime.Now.AddYears(-1).AddDays(6), ApprovedOrDeniedOn = DateTime.Now.AddYears(-1).AddDays(6) }
            );
            _dbContext.AppDBContext.SaveChanges();
        }

        public void DestroyAssesmblyAfterTest()
        {
            _dbContext.AppDBContext.RegistrationRequests.RemoveRange(_dbContext.AppDBContext.RegistrationRequests);
            _dbContext.AppDBContext.SaveChanges();
        }

        [TestMethod]
        public async Task TestAddRegistrationRequest()
        {
            AssemblyInit();
            RegistrationRequestRepository registrationRequestRepository = new RegistrationRequestRepository(_dbContext.AppDBContext);
            // Act
            await registrationRequestRepository.AddRegistrationRequest("userId101");
            // Assert
            Assert.AreEqual(3, _dbContext.AppDBContext.RegistrationRequests.Count());
            DestroyAssesmblyAfterTest();
        }

        [TestMethod]
        public async Task TestGetRegistrationRequest()
        {
            AssemblyInit();
            RegistrationRequestRepository registrationRequestRepository = new RegistrationRequestRepository(_dbContext.AppDBContext);
            // Act
            RegistrationRequest registrationRequest1 = await registrationRequestRepository.GetRegistrationRequestById(22);
            RegistrationRequest registrationRequest2 = await registrationRequestRepository.GetRegistrationRequestByUserIdAsync("userId95");
            // Assert
            Assert.AreEqual(registrationRequest1.CreatedOn, registrationRequest2.CreatedOn);
            DestroyAssesmblyAfterTest();
        }

        [TestMethod]
        public async Task TestUpdateRegistrationRequest()
        {
            AssemblyInit();
            RegistrationRequestRepository registrationRequestRepository = new RegistrationRequestRepository(_dbContext.AppDBContext);
            // Act
            RegistrationRequest registrationRequest = registrationRequestRepository.GetRegistrationRequestByUserIdAsync("userId95").Result;
            DateTime? approuvedorDenied = registrationRequest.ApprovedOrDeniedOn;
            string adminComment = registrationRequest.AdminComment;
            registrationRequest.AdminComment = "This user is a danger for other members";
            registrationRequest.ApprovedOrDeniedOn = DateTime.Now;
            await registrationRequestRepository.UpdateRegistrationRequestAsync(registrationRequest);
            RegistrationRequest registrationRequest2 = registrationRequestRepository.GetRegistrationRequestByUserIdAsync("userId95").Result;
            bool isNotEqual = adminComment != registrationRequest2.AdminComment && approuvedorDenied != registrationRequest2.ApprovedOrDeniedOn;
            // Assert
            Assert.AreEqual(true, isNotEqual);
            DestroyAssesmblyAfterTest();
        }
    }
}
