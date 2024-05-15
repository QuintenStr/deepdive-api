using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest_Project.Entities
{
    [TestClass]
    public class UTExcursionParticipants
    {
        private static DbContext _dbContext = new DbContext("DB_ExcursionsParticipants");

        public void AssemblyInit()
        {
            _dbContext.AppDBContext.Database.EnsureDeleted();
            _dbContext.AppDBContext.ExcursionParticipants.Add(new ExcursionParticipant
            {
                Id = "id1",
                ExcursionId = "99",
                UserId = "70"
            });
            _dbContext.AppDBContext.SaveChanges();
        }

        public void DestroyAssesmblyAfterTest()
        {
            _dbContext.AppDBContext.ExcursionParticipants.RemoveRange(_dbContext.AppDBContext.ExcursionParticipants);
            _dbContext.AppDBContext.SaveChanges();
        }

        [TestMethod]
        public async Task TestExcursionParticipant()
        {
            AssemblyInit();
            ExcursionParticipantsRepository excursionParticipantsRepository = new ExcursionParticipantsRepository(_dbContext.AppDBContext);

            // Act 
            await excursionParticipantsRepository.AddParticipantToExcursion("99", "21");

            // Assert
            Assert.AreEqual(2, _dbContext.AppDBContext.ExcursionParticipants.Count());
            DestroyAssesmblyAfterTest();
        }

        [TestMethod]
        public async Task TestExcursionParticipants()
        {
            AssemblyInit();
            ExcursionParticipantsRepository excursionParticipantsRepository = new ExcursionParticipantsRepository(_dbContext.AppDBContext);

            // Act 
            List<string> idList = new List<string>{"72", "73","74"};
            await excursionParticipantsRepository.AddMultipleParticipantsToExcursion("99", idList);

            // Assert
            Assert.AreEqual(4, _dbContext.AppDBContext.ExcursionParticipants.Count());
            DestroyAssesmblyAfterTest();
        }

        [TestMethod]
        public async Task TestDeletParticipan()
        {
            AssemblyInit();
            ExcursionParticipantsRepository excursionParticipantsRepository = new ExcursionParticipantsRepository(_dbContext.AppDBContext);

            // Act 
            excursionParticipantsRepository.AddParticipantToExcursion("99", "61");
            //Assert.AreEqual(2, _dbContext.AppDBContext.ExcursionParticipants.Count());

            // Assert
            await excursionParticipantsRepository.RemoveParticipantFromExcursion("99", "70");
            Assert.AreEqual(1, _dbContext.AppDBContext.ExcursionParticipants.Count());
            DestroyAssesmblyAfterTest();
        }
    }
}
