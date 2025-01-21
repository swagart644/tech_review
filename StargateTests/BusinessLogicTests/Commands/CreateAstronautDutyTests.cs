using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Stargate.Server.Business.Commands;
using Stargate.Server.Data;
using Stargate.Server.Data.Models;
using Stargate.Server.Logging;

namespace StargateTests.BusinessLogicTests.Commands
{
    [TestClass]
    public class CreateAstronautDutyTests
    {
        Mock<ILoggingWrapper> _mockLogger;
        Mock<StargateContext> _mockContext;

        Mock<CreateAstronautDutyPreProcessor> _preprocessor;
        Mock<CreateAstronautDutyHandler> _handler;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILoggingWrapper>();
            _mockLogger.Setup(x => x.Log(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

            _mockContext = new Mock<StargateContext>();

            _preprocessor = new Mock<CreateAstronautDutyPreProcessor>(_mockContext.Object, _mockLogger.Object);
            _preprocessor.CallBase = true;
            _handler = new Mock<CreateAstronautDutyHandler>(_mockContext.Object, _mockLogger.Object);
            _handler.CallBase = true;
        }

        [TestMethod]
        [ExpectedException(typeof(BadHttpRequestException))]
        public async Task Preprocessor_FindsPrevious()
        {
            var request = new CreateAstronautDuty 
            { 
                Name = "Spaceman John", 
                Rank = "PVT", 
                DutyTitle = "New Title", 
                DutyStartDate = DateTime.Parse("2025-01-20 08:00:00") 
            };

            _preprocessor.Setup(x => x.GetPerson(It.IsAny<CreateAstronautDuty>())).Returns(new Person()
            {
                Id = 1,
                Name = "Spaceman John"
            });

            _preprocessor.Setup(x => x.SearchForRepeatDuty(It.IsAny<CreateAstronautDuty>(), It.IsAny<Person>())).Returns(new AstronautDuty()
            {
                Rank = "PVT",
                DutyTitle = "New Title"
            });

            await _preprocessor.Object.Process(request, CancellationToken.None);
        }

        [TestMethod]
        public async Task Preprocessor_DoesNotFindDutyPrevious()
        {
            var request = new CreateAstronautDuty
            {
                Name = "Ricky Bobby",
                Rank = "Number 2",
                DutyTitle = "New Title",
                DutyStartDate = DateTime.Parse("2025-01-20 08:00:00")
            };

            _preprocessor.Setup(x => x.GetPerson(It.IsAny<CreateAstronautDuty>())).Returns(new Person()
            {
                Id = 1,
                Name = "Ricky Bobby"
            });

            _preprocessor.Setup(x => x.SearchForRepeatDuty(It.IsAny<CreateAstronautDuty>(), It.IsAny<Person>())).Returns((AstronautDuty)null);

            await _preprocessor.Object.Process(request, CancellationToken.None);

            _mockLogger.Verify(x => x.Log(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(BadHttpRequestException))]
        public async Task Preprocessor_ThrowsUponNoPerson()
        {
            var request = new CreateAstronautDuty
            {
                Name = "Some dude",
                Rank = "SGT",
                DutyTitle = "New Title",
                DutyStartDate = DateTime.Parse("2025-01-20 08:00:00")
            };

            _preprocessor.Setup(x => x.GetPerson(It.IsAny<CreateAstronautDuty>())).Returns((Person)null);

            await _preprocessor.Object.Process(request, CancellationToken.None);

            _mockLogger.Verify(x => x.Log(It.IsAny<string>(), It.IsAny<bool>()), Times.Once());
        }

        [TestMethod]
        public async Task Handler_returns()
        {
            CreateAstronautDuty request = new CreateAstronautDuty
            {
                Name = "Spaceman John",
                Rank = "PVT",
                DutyTitle = "Spacewalker",
                DutyStartDate = DateTime.Parse("2024-07-15 08:00:00")
            };

            (Person? person, AstronautDetail? detail) response = (new Person()
            {
                Id = 100,
                Name = "Spaceman Rick"
            }, new AstronautDetail()
            {
                Id = 400,
                PersonId = 100,
                CurrentDutyTitle = "Telescope Operator",
                CurrentRank = "PVT",
                CareerStartDate = DateTime.Parse("1901-1-18 12:00:00")
            });


            _handler.Setup(x => x.GetPersonInfo(It.IsAny<CreateAstronautDuty>())).ReturnsAsync(response);
            _handler.Setup(x => x.GetAstronautDuties(It.IsAny<Person?>())).ReturnsAsync(new AstronautDuty()
            {
            });

            _handler.Setup(x => x.SaveNewAstronautDuties(It.IsAny<AstronautDuty>())).Returns(Task.CompletedTask);
            _handler.Setup(x => x.UpdateAstronautDuties(It.IsAny<AstronautDuty>())).Returns(Task.CompletedTask);
            _handler.Setup(x => x.UpdateDetails(It.IsAny<AstronautDetail?>())).Returns(Task.CompletedTask);
            _handler.Setup(x => x.SaveDetails(It.IsAny<AstronautDetail?>())).Returns(Task.CompletedTask);

            var retVal = await _handler.Object.Handle(request, CancellationToken.None);

            Assert.IsNotNull(retVal);
        }

        [TestMethod]
        public async Task Handler_NoDetails_NoPrevious()
        {
            CreateAstronautDuty request = new CreateAstronautDuty
            {
                Name = "Spaceman John",
                Rank = "PVT",
                DutyTitle = "Spacewalker",
                DutyStartDate = DateTime.Parse("2024-07-15 08:00:00")
            };

            (Person? person, AstronautDetail? detail) response = (new Person()
            {
                Id = 100,
                Name = "Spaceman Rick"
            }, null);

            _handler.Setup(x => x.GetPersonInfo(It.IsAny<CreateAstronautDuty>())).ReturnsAsync(response);
            _handler.Setup(x => x.GetAstronautDuties(It.IsAny<Person?>())).ReturnsAsync(new AstronautDuty()
            {
            });

            _handler.Setup(x => x.SaveNewAstronautDuties(It.IsAny<AstronautDuty>())).Returns(Task.CompletedTask);
            _handler.Setup(x => x.UpdateAstronautDuties(It.IsAny<AstronautDuty>())).Returns(Task.CompletedTask);
            _handler.Setup(x => x.UpdateDetails(It.IsAny<AstronautDetail?>())).Returns(Task.CompletedTask);
            _handler.Setup(x => x.SaveDetails(It.IsAny<AstronautDetail?>())).Returns(Task.CompletedTask);

            var retVal = await _handler.Object.Handle(request, CancellationToken.None);

            Assert.IsNotNull(retVal);
            _handler.Verify(x => x.UpdateDetails(It.IsAny<AstronautDetail>()), Times.Once);
            _handler.Verify(x => x.SaveNewAstronautDuties(It.IsAny<AstronautDuty>()), Times.Once);
        }
    }
}
