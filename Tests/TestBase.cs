// ========================================
// TestBase.cs - Base class for all tests
// ========================================
using Microsoft.EntityFrameworkCore;
using SASD.Models;
using SASD.Models.Enums;

namespace SASD.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        protected void SeedTestData(ApplicationDbContext context)
        {
            // Seed Sports
            var sports = new List<Sport>
            {
                new Sport { Id = 1, Name = "Football", NoOfPlayers = 11, NoOfSubscriptions = 100, SportType = SportType.Team },
                new Sport { Id = 2, Name = "Tennis", NoOfPlayers = 1, NoOfSubscriptions = 50, SportType = SportType.Individual },
                new Sport { Id = 3, Name = "Basketball", NoOfPlayers = 5, NoOfSubscriptions = 80, SportType = SportType.Team }
            };
            context.Sports.AddRange(sports);

            // Seed SportEvents
            var sportEvents = new List<SportEvent>
            {
                new SportEvent
                {
                    Id = 1,
                    Name = "Championship Final",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                    MaxNoOfPlayers = 22,
                    Location = "Stadium A",
                    SportId = 1
                },
                new SportEvent
                {
                    Id = 2,
                    Name = "Tennis Tournament",
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(2).AddHours(3),
                    MaxNoOfPlayers = 2,
                    Location = "Court 1",
                    SportId = 2
                },
                new SportEvent
                {
                    Id = 3,
                    Name = "Basketball League",
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(3).AddHours(2),
                    MaxNoOfPlayers = 10,
                    Location = "Gym B",
                    SportId = 3
                }
            };
            context.SportEvents.AddRange(sportEvents);

            // Seed PlayerRecords
            var playerRecords = new List<PlayerRecord>
            {
                new PlayerRecord
                {
                    Id = 1,
                    PlayerName = "John",
                    PlayerSurname = "Doe",
                    NoOfPoints = 85,
                    Arrived = true,
                    Description = "Star player",
                    SportEventId = 1
                },
                new PlayerRecord
                {
                    Id = 2,
                    PlayerName = "Jane",
                    PlayerSurname = "Smith",
                    NoOfPoints = 92,
                    Arrived = false,
                    Description = "Top scorer",
                    SportEventId = 1
                },
                new PlayerRecord
                {
                    Id = 3,
                    PlayerName = "Mike",
                    PlayerSurname = "Johnson",
                    NoOfPoints = 78,
                    Arrived = true,
                    Description = "Reliable defender",
                    SportEventId = 2
                }
            };
            context.PlayerRecords.AddRange(playerRecords);

            context.SaveChanges();
        }

        public virtual void Dispose()
        {
            // Base cleanup
        }
    }
}