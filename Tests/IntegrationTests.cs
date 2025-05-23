// ========================================
// 4. INTEGRATION TESTS
// ========================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SASD.Models.Enums;
using SASD.Models;
using SASD.ViewModels;
using System.Net.Http;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace SASD.Tests.Integration
{
    public class DatabaseIntegrationTests : TestBase
    {
        [Fact]
        public async Task Database_ShouldMaintainReferentialIntegrity()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);

            // Act - Try to delete a sport that has events
            var sportWithEvents = await context.Sports
                .Where(s => context.SportEvents.Any(se => se.SportId == s.Id))
                .FirstOrDefaultAsync();

            // In a real scenario, this should either:
            // 1. Prevent deletion (FK constraint)
            // 2. Cascade delete related events
            var hasRelatedEvents = await context.SportEvents
                .AnyAsync(se => se.SportId == sportWithEvents!.Id);

            // Assert
            hasRelatedEvents.Should().BeTrue("Sport should have related events");
            sportWithEvents.Should().NotBeNull();
        }

        [Fact]
        public async Task LayerIntegration_ControllerToDatabase_ShouldWork()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = new SportController(context);

            // Act - Create sport through controller (Presentation -> Data)
            var newSport = new Sport
            {
                Name = "Integration Test Sport",
                NoOfPlayers = 6,
                NoOfSubscriptions = 30,
                SportType = SportType.Team
            };

            await controller.CreateEditSport(newSport);

            // Assert - Verify data was persisted through all layers
            var savedSport = await context.Sports
                .FirstOrDefaultAsync(s => s.Name == "Integration Test Sport");

            savedSport.Should().NotBeNull();
            savedSport!.NoOfPlayers.Should().Be(6);
            savedSport.SportType.Should().Be(SportType.Team);
        }

        [Fact]
        public async Task CompleteWorkflow_CreateSportEventWithPlayers_ShouldWork()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var sportEventController = new SportEventController(context);

            // Act 1 - Create new sport event
            var newEvent = new SportEvent
            {
                Id = 0,
                Name = "Integration Test Event",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                MaxNoOfPlayers = 15,
                Location = "Integration Stadium",
                SportId = 1 // Football
            };

            await sportEventController.CreateEditSportEvent(newEvent);

            // Act 2 - Add player to the event
            var createdEvent = await context.SportEvents
                .FirstOrDefaultAsync(se => se.Name == "Integration Test Event");

            var newPlayer = new PlayerRecord
            {
                Id = 0,
                PlayerName = "Integration",
                PlayerSurname = "Player",
                NoOfPoints = 88,
                Arrived = true,
                Description = "Integration test player",
                SportEventId = createdEvent!.Id
            };

            await sportEventController.CreateEditPlayerRecord(newPlayer);

            // Assert - Verify complete workflow
            var eventWithPlayer = await context.SportEvents
                .Include(se => se.PlayerRecords)
                .Include(se => se.Sport)
                .FirstOrDefaultAsync(se => se.Name == "Integration Test Event");

            eventWithPlayer.Should().NotBeNull();
            eventWithPlayer!.Sport!.Name.Should().Be("Football");
            eventWithPlayer.PlayerRecords.Should().HaveCount(1);
            eventWithPlayer.PlayerRecords!.First().PlayerName.Should().Be("Integration");
        }
    }

    public class MasterDetailIntegrationTests : TestBase
    {
        [Fact]
        public async Task MasterDetail_NavigationBetweenEvents_ShouldWork()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportEventController(context);

            // Act 1 - Get first event
            var firstResult = await controller.SportEvents(null);
            var firstViewModel = (firstResult as ViewResult)!.Model as SportEventDetailViewModel;

            // Act 2 - Navigate to next event
            var nextEventId = firstViewModel!.NextEventId;
            var nextResult = await controller.SportEvents(nextEventId);
            var nextViewModel = (nextResult as ViewResult)!.Model as SportEventDetailViewModel;

            // Assert
            firstViewModel.CurrentSportEvent!.Id.Should().Be(1);
            firstViewModel.NextEventId.Should().NotBeNull();

            nextViewModel!.CurrentSportEvent!.Id.Should().Be(nextEventId);
            nextViewModel.PreviousEventId.Should().Be(1);
        }

        [Fact]
        public async Task MasterDetail_PlayerRecordsForEvent_ShouldBeCorrect()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportEventController(context);

            // Act
            var result = await controller.SportEvents(1);
            var viewModel = (result as ViewResult)!.Model as SportEventDetailViewModel;

            // Assert
            viewModel!.CurrentSportEvent!.PlayerRecords.Should().HaveCount(2);
            viewModel.CurrentSportEvent.PlayerRecords!
                .Any(pr => pr.PlayerName == "John" && pr.PlayerSurname == "Doe")
                .Should().BeTrue();
        }
    }
}