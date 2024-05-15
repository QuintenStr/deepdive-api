using deepdiveapi.Entities.DataTransferObjects;
using deepdiveapi.Entities.Models;
using deepdiveapi.Repositories;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest_Project.Entities
{
    [TestClass]
    public class UTExcursion
    {

        private static DbContext _dbContext = new DbContext("DB_Excursions");

        public void AssemblyInit()
        {
            _dbContext.AppDBContext.Database.EnsureDeleted();
            _dbContext.AppDBContext.Excursions.Add(new Excursion
            {
                Id = "id1",
                Title = "Forest Trail Exploration",
                Description = "Discover the beauty of the forest with this relaxing trail walk. Suitable for all ages.",
                CreatedByUserFK = "User010",
                CreatedOn = new DateTime(2024, 04, 09),
                DateTime = new DateTime(2024, 09, 10, 10, 0, 0),
                Location = new Point(35.6895, 139.6917),
                ImageName = "forest_trail.jpg"
            });

            _dbContext.AppDBContext.Excursions.Add(new Excursion
            {
                Id = "id2",
                Title = "Riverside Cycling Adventure",
                Description = "Enjoy a thrilling cycling journey along the scenic riverside. Ideal for cycling enthusiasts.",
                CreatedByUserFK = "User011",
                CreatedOn = new DateTime(2024, 04, 10),
                DateTime = new DateTime(2024, 10, 20, 7, 30, 0),
                Location = new Point(-0.1276, 51.5072),
                ImageName = "riverside_cycling.jpg"
            });

            _dbContext.AppDBContext.Excursions.Add(new Excursion
            {
                Id = "id3",
                Title = "Historical Castle Tour",
                Description = "Step back in time with a guided tour of ancient castles. Perfect for history buffs.",
                CreatedByUserFK = "User012",
                CreatedOn = new DateTime(2024, 04, 11),
                DateTime = new DateTime(2024, 11, 05, 11, 0, 0),
                Location = new Point(48.8566, 2.3522),
                ImageName = "castle_tour.jpg"
            });

            _dbContext.AppDBContext.SaveChanges();
        }

        public void DestroyAssesmblyAfterTest()
        {
            _dbContext.AppDBContext.Excursions.RemoveRange(_dbContext.AppDBContext.Excursions);
            _dbContext.AppDBContext.SaveChanges();
        }

        [TestMethod]
        public async Task TestGetAllExcusions()
        {
            AssemblyInit();
            ExcursionRepository _excursionRepository = new ExcursionRepository(_dbContext.AppDBContext);

            // Act
            var excursions = await _excursionRepository.GetExcursions();

            // Assert
            Assert.AreEqual(3, excursions.Count);
            DestroyAssesmblyAfterTest();
        }

        [TestMethod]
        public async Task TestAddExcusion()
        {
            AssemblyInit();
            ExcursionRepository _excursionRepository = new ExcursionRepository(_dbContext.AppDBContext);
            NewExcursionDto newExcursionDTO = new NewExcursionDto { Title = "New", Description = "Excursion Description", DateTime = new DateTime(2024, 4, 9), ImageName = "imageTest.jpg", Coordinates = new CoordinatesDto { Lat = 50.0754, Long = 4.1314 } };

            // Act
            await _excursionRepository.AddNewExcursion(newExcursionDTO, "ID545464");
            var excursions = _excursionRepository.GetExcursions().Result;

            // Assert
            Assert.AreEqual(4, excursions.Count);
            DestroyAssesmblyAfterTest();
        }

        [TestMethod]
        public async Task TestDeleteExcusion()
        {
            AssemblyInit();
            ExcursionRepository _excursionRepository = new ExcursionRepository(_dbContext.AppDBContext);

            // Act
            await _excursionRepository.DeleteExcursion("id3");
            Task<List<Excursion>> excursions = _excursionRepository.GetExcursions();

            // Assert
            Assert.AreEqual(2, excursions.Result.Count);
            DestroyAssesmblyAfterTest();
        }

        [TestMethod]
        public async Task TestUpdateExcusion()
        {
            AssemblyInit();
            ExcursionRepository _excursionRepository = new ExcursionRepository(_dbContext.AppDBContext);
            string excursionBefore = _excursionRepository.GetExcursions().Result.FirstOrDefault().Title;
            DateTime excursionBeforeDatetime = _excursionRepository.GetExcursions().Result.FirstOrDefault().DateTime;

            // Act
            Excursion excursion = _excursionRepository.GetExcursions().Result.FirstOrDefault();
            excursion.Title = "NewTITLE";
            excursion.DateTime = DateTime.Now;
            await _excursionRepository.UpdateExcursionAsync(excursion);
            excursion = _excursionRepository.GetExcursions().Result.FirstOrDefault();
            int a = 5;
            bool AreSame = excursionBefore == excursion.Title || excursionBeforeDatetime == excursion.DateTime;

            // Assert
            Assert.AreEqual(false, AreSame);
            DestroyAssesmblyAfterTest();
        }
    }
}
