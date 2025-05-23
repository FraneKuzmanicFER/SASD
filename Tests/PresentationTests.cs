// ========================================
// 2. PRESENTATION LAYER UNIT TESTS
// ========================================
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using SASD.Models;
using SASD.Models.Enums;
using SASD.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace SASD.Tests.Presentation
{
    public class SportControllerTests : TestBase
    {
        [Fact]
        public async Task Sports_ShouldReturnViewWithSports()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportController(context);

            // Act
            var result = await controller.Sports();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<List<Sport>>();
            var model = viewResult.Model as List<Sport>;
            model.Should().HaveCount(3);
        }

        [Fact]
        public void CreateEditSportForm_WithId_ShouldReturnViewWithSport()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportController(context);

            // Act
            var result = controller.CreateEditSportForm(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<Sport>();
            var model = viewResult.Model as Sport;
            model!.Id.Should().Be(1);
            model.Name.Should().Be("Football");
        }

        [Fact]
        public void CreateEditSportForm_WithoutId_ShouldReturnEmptyView()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = new SportController(context);

            // Act
            var result = controller.CreateEditSportForm(null);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeNull();
        }

        [Fact]
        public async Task CreateEditSport_CreateNew_ShouldAddSportAndRedirect()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = new SportController(context);
            var newSport = new Sport
            {
                Id = 0,
                Name = "New Sport",
                NoOfPlayers = 7,
                NoOfSubscriptions = 25,
                SportType = SportType.Team
            };

            // Act
            var result = await controller.CreateEditSport(newSport);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult!.ActionName.Should().Be("Sports");

            var addedSport = await context.Sports.FirstOrDefaultAsync(s => s.Name == "New Sport");
            addedSport.Should().NotBeNull();
            addedSport!.NoOfPlayers.Should().Be(7);
        }

        [Fact]
        public async Task Delete_ExistingSport_ShouldRemoveAndRedirect()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportController(context);

            // Act
            var result = await controller.Delete(1);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var deletedSport = await context.Sports.FindAsync(1);
            deletedSport.Should().BeNull();
        }

        [Fact]
        public async Task Delete_NonExistingSport_ShouldReturnNotFound()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = new SportController(context);

            // Act
            var result = await controller.Delete(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }

    public class SportEventControllerTests : TestBase
    {
        [Fact]
        public async Task SportEvents_WithNoEvents_ShouldReturnViewModelWithNoEvents()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = new SportEventController(context);

            // Act
            var result = await controller.SportEvents(null);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var viewModel = viewResult!.Model as SportEventDetailViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.HasEvents.Should().BeFalse();
        }

        [Fact]
        public async Task SportEvents_WithEvents_ShouldReturnFirstEvent()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportEventController(context);

            // Act
            var result = await controller.SportEvents(null);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var viewModel = viewResult!.Model as SportEventDetailViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.HasEvents.Should().BeTrue();
            viewModel.CurrentSportEvent.Should().NotBeNull();
            viewModel.CurrentSportEvent!.Name.Should().Be("Championship Final");
        }

        [Fact]
        public async Task SportEvents_WithSpecificId_ShouldReturnCorrectEvent()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportEventController(context);

            // Act
            var result = await controller.SportEvents(2);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var viewModel = viewResult!.Model as SportEventDetailViewModel;
            viewModel!.CurrentSportEvent!.Id.Should().Be(2);
            viewModel.CurrentSportEvent.Name.Should().Be("Tennis Tournament");
        }

        [Fact]
        public async Task SportEvents_ShouldSetNavigationProperties()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportEventController(context);

            // Act
            var result = await controller.SportEvents(2); // Middle event

            // Assert
            var viewResult = result as ViewResult;
            var viewModel = viewResult!.Model as SportEventDetailViewModel;
            viewModel!.PreviousEventId.Should().Be(1);
            viewModel.NextEventId.Should().Be(3);
        }

        [Fact]
        public async Task CreateEditSportEventForm_WithoutId_ShouldReturnNewEvent()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportEventController(context);

            // Act
            var result = await controller.CreateEditSportEventForm(null);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<SportEvent>();
            var model = viewResult.Model as SportEvent;
            model!.Id.Should().Be(0);
            model.StartDate.Date.Should().Be(DateTime.Today);
        }

        [Fact]
        public async Task CreateEditSportEvent_WithValidation_ShouldReturnErrorForInvalidDates()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportEventController(context);
            var invalidEvent = new SportEvent
            {
                Id = 0,
                Name = "Invalid Event",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow, // End before start
                MaxNoOfPlayers = 10,
                Location = "Test Location",
                SportId = 1
            };

            // Act
            var result = await controller.CreateEditSportEvent(invalidEvent);

            // Assert
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
            controller.ModelState.Should().ContainKey("EndDate");
        }

        [Fact]
        public async Task CreateEditPlayerRecord_Create_ShouldAddPlayerRecord()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportEventController(context);
            var newRecord = new PlayerRecord
            {
                Id = 0,
                PlayerName = "New",
                PlayerSurname = "Player",
                NoOfPoints = 75,
                Arrived = true,
                Description = "Test record",
                SportEventId = 1
            };

            // Act
            var result = await controller.CreateEditPlayerRecord(newRecord);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var addedRecord = await context.PlayerRecords
                .FirstOrDefaultAsync(pr => pr.PlayerName == "New" && pr.PlayerSurname == "Player");
            addedRecord.Should().NotBeNull();
            addedRecord!.NoOfPoints.Should().Be(75);
        }

        [Fact]
        public async Task DeletePlayerRecord_ExistingRecord_ShouldRemoveAndRedirect()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = new SportEventController(context);

            // Act
            var result = await controller.DeletePlayerRecord(1);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var deletedRecord = await context.PlayerRecords.FindAsync(1);
            deletedRecord.Should().BeNull();
        }
    }

    public class ViewModelTests
    {
        [Fact]
        public void SportEventDetailViewModel_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var viewModel = new SportEventDetailViewModel
            {
                CurrentSportEvent = new SportEvent { Id = 1, Name = "Test Event" },
                PreviousEventId = 2,
                NextEventId = 3,
                HasEvents = true
            };

            // Assert
            viewModel.CurrentSportEvent.Should().NotBeNull();
            viewModel.CurrentSportEvent!.Id.Should().Be(1);
            viewModel.PreviousEventId.Should().Be(2);
            viewModel.NextEventId.Should().Be(3);
            viewModel.HasEvents.Should().BeTrue();
        }
    }
}