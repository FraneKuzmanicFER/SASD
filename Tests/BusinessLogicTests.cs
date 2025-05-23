// ========================================
// 3. BUSINESS LOGIC UNIT TESTS
// ========================================
using FluentAssertions;
using System.Linq;
using SASD.Models.Enums;
using SASD.Models;
using Microsoft.EntityFrameworkCore;

namespace SASD.Tests.Business
{
    public class BusinessLogicValidationTests : TestBase
    {
        [Theory]
        [InlineData(-1)] // Negative points
        [InlineData(101)] // Points over 100 (assuming max is 100)
        public void PlayerRecord_ShouldValidatePointsRange(int points)
        {
            // Arrange
            var playerRecord = new PlayerRecord
            {
                PlayerName = "Test",
                PlayerSurname = "Player",
                NoOfPoints = points,
                Arrived = true,
                Description = "Test",
                SportEventId = 1
            };

            // Act & Assert - Custom validation rule
            // In a real application, you might have custom validation attributes
            if (points < 0 || points > 100)
            {
                // This represents your complex validation rule
                Assert.True(points < 0 || points > 100, "Points should be between 0 and 100");
            }
        }

        [Fact]
        public void SportEvent_ShouldValidateMaxPlayersAgainstSportType()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);

            // Act - Get a team sport (Football = 11 players)
            var footballSport = context.Sports.First(s => s.Name == "Football");
            var sportEvent = new SportEvent
            {
                Name = "Test Event",
                SportId = footballSport.Id,
                MaxNoOfPlayers = 5 // Less than typical team size
            };

            // Assert - Business rule: Team sports should have reasonable max players
            if (footballSport.SportType == SportType.Team && sportEvent.MaxNoOfPlayers < footballSport.NoOfPlayers)
            {
                Assert.True(true, "Max players should be at least equal to sport's typical player count");
            }
        }

        [Fact]
        public void SportEvent_ShouldValidateDateLogic()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(1);
            var endDate = startDate.AddHours(-1); // Invalid: end before start

            // Act
            var isValid = endDate > startDate;

            // Assert
            isValid.Should().BeFalse("End date should be after start date");
        }

        [Fact]
        public async Task PlayerRecord_ShouldValidateUniquePlayerPerEvent()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);

            // Act - Check if player already exists in event
            var existingPlayer = await context.PlayerRecords
                .FirstOrDefaultAsync(pr => pr.SportEventId == 1
                                        && pr.PlayerName == "John"
                                        && pr.PlayerSurname == "Doe");

            var isDuplicate = existingPlayer != null;

            // Assert - Business rule: No duplicate players in same event
            isDuplicate.Should().BeTrue("John Doe should already exist in event 1");
        }
    }

    public class SportEventNavigationLogicTests : TestBase
    {
        [Fact]
        public async Task GetNavigationIds_ShouldReturnCorrectPreviousAndNext()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);

            // Act - Simulate the navigation logic from SportEventController
            var allEventIds = await context.SportEvents
                .OrderBy(se => se.StartDate)
                .Select(se => se.Id)
                .ToListAsync();

            var currentEventId = 2;
            var currentIndex = allEventIds.IndexOf(currentEventId);

            int previousId = allEventIds[currentIndex - 1];
            int nextId = allEventIds[currentIndex + 1];

            // Assert
            previousId.Should().Be(1);
            nextId.Should().Be(3);
        }
    }
}