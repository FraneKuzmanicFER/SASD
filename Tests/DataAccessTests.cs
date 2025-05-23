// ========================================
// 1. DATA ACCESS LAYER UNIT TESTS
// ========================================
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using SASD.Models;
using SASD.Models.Enums;

namespace SASD.Tests.DataAccess
{
    public class ApplicationDbContextTests : TestBase
    {
        [Fact]
        public void DbContext_ShouldCreateDatabaseAndTables()
        {
            // Arrange & Act
            using var context = GetInMemoryContext();
            context.Database.EnsureCreated();

            // Assert
            context.Sports.Should().NotBeNull();
            context.SportEvents.Should().NotBeNull();
            context.PlayerRecords.Should().NotBeNull();
        }

        [Fact]
        public async Task DbContext_ShouldSaveAndRetrieveData()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var sport = new Sport
            {
                Name = "Test Sport",
                NoOfPlayers = 5,
                NoOfSubscriptions = 10,
                SportType = SportType.Team
            };

            // Act
            context.Sports.Add(sport);
            await context.SaveChangesAsync();
            var retrievedSport = await context.Sports.FirstOrDefaultAsync(s => s.Name == "Test Sport");

            // Assert
            retrievedSport.Should().NotBeNull();
            retrievedSport!.Name.Should().Be("Test Sport");
            retrievedSport.NoOfPlayers.Should().Be(5);
        }

        [Fact]
        public async Task DbContext_ShouldHandleRelationships()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);

            // Act
            var sportEvent = await context.SportEvents
                .Include(se => se.Sport)
                .Include(se => se.PlayerRecords)
                .FirstOrDefaultAsync(se => se.Id == 1);

            // Assert
            sportEvent.Should().NotBeNull();
            sportEvent!.Sport.Should().NotBeNull();
            sportEvent.Sport!.Name.Should().Be("Football");
            sportEvent.PlayerRecords.Should().HaveCount(2);
        }
    }

    public class ModelValidationTests : TestBase
    {
        [Fact]
        public void Sport_ShouldHaveValidProperties()
        {
            // Arrange & Act
            var sport = new Sport
            {
                Id = 1,
                Name = "Test Sport",
                NoOfPlayers = 11,
                NoOfSubscriptions = 50,
                SportType = SportType.Team
            };

            // Assert
            sport.Id.Should().Be(1);
            sport.Name.Should().Be("Test Sport");
            sport.NoOfPlayers.Should().Be(11);
            sport.NoOfSubscriptions.Should().Be(50);
            sport.SportType.Should().Be(SportType.Team);
        }

        [Fact]
        public void SportEvent_ShouldHaveValidProperties()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddHours(2);

            // Act
            var sportEvent = new SportEvent
            {
                Id = 1,
                Name = "Test Event",
                StartDate = startDate,
                EndDate = endDate,
                MaxNoOfPlayers = 20,
                Location = "Test Location",
                SportId = 1
            };

            // Assert
            sportEvent.Id.Should().Be(1);
            sportEvent.Name.Should().Be("Test Event");
            sportEvent.StartDate.Should().Be(startDate);
            sportEvent.EndDate.Should().Be(endDate);
            sportEvent.MaxNoOfPlayers.Should().Be(20);
            sportEvent.Location.Should().Be("Test Location");
            sportEvent.SportId.Should().Be(1);
        }

        [Fact]
        public void PlayerRecord_ShouldHaveValidProperties()
        {
            // Arrange & Act
            var playerRecord = new PlayerRecord
            {
                Id = 1,
                PlayerName = "John",
                PlayerSurname = "Doe",
                NoOfPoints = 85,
                Arrived = true,
                Description = "Test player",
                SportEventId = 1
            };

            // Assert
            playerRecord.Id.Should().Be(1);
            playerRecord.PlayerName.Should().Be("John");
            playerRecord.PlayerSurname.Should().Be("Doe");
            playerRecord.NoOfPoints.Should().Be(85);
            playerRecord.Arrived.Should().BeTrue();
            playerRecord.Description.Should().Be("Test player");
            playerRecord.SportEventId.Should().Be(1);
        }
    }
}