using Moq;
using Stargate.Server.Business.Queries;
using Stargate.Server.Data;
using Stargate.Server.Data.Models;
using Stargate.Server.Logging;

namespace StargateTests.BusinessLogicTests.Queries
{
    [TestClass]
    public class GetAstronautDutiesByNameTests
    {
        Mock<ILoggingWrapper> _mockLogger;
        Mock<StargateContext> _mockContext;
        Mock<GetAstronautDutiesByNameHandler> _handler;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILoggingWrapper>();
            _mockLogger.Setup(x => x.Log(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

            _mockContext = new Mock<StargateContext>();

            _handler = new Mock<GetAstronautDutiesByNameHandler>(_mockContext.Object, _mockLogger.Object);
            _handler.CallBase = true;
        }

        [TestMethod]
        public async Task Handler_FindsEverything()
        {
            GetAstronautDutiesByName request = new GetAstronautDutiesByName();

            _handler.Setup(x => x.GetPersonAstronaut(It.IsAny<GetAstronautDutiesByName>())).ReturnsAsync(new PersonAstronaut()
            {
                PersonId = 1,
                Name = "Illidan Stormrage",
                CurrentRank = "NoIdea",
                CurrentDutyTitle = "Demon Hunter",
                CareerStartDate = DateTime.Parse("2004-11-23 23:59:00")
            });

            _handler.Setup(x => x.GetAstronautDuties(It.IsAny<PersonAstronaut>())).ReturnsAsync(new List<AstronautDuty>()
            {
                new AstronautDuty()
                {
                    Id = 1,
                    PersonId = 1,
                    Rank = "NoIdea",
                    DutyTitle = "Demon Hunter",
                    DutyStartDate = DateTime.Parse("2004-11-23 12:00:00")
                }
            });

            var retVal = await _handler.Object.Handle(request, CancellationToken.None);

            Assert.IsNotNull(retVal);
        }

        [TestMethod]
        public async Task Handler_FindsNoDuties()
        {
            GetAstronautDutiesByName request = new GetAstronautDutiesByName();

            _handler.Setup(x => x.GetPersonAstronaut(It.IsAny<GetAstronautDutiesByName>())).ReturnsAsync(new PersonAstronaut()
            {
                PersonId = 1,
                Name = "Illidan Stormrage",
                CurrentRank = "NoIdea",
                CurrentDutyTitle = "Demon Hunter",
                CareerStartDate = DateTime.Parse("2004-11-23 23:59:00")
            });

            _handler.Setup(x => x.GetAstronautDuties(It.IsAny<PersonAstronaut>())).ReturnsAsync((List<AstronautDuty>)null);

            var retVal = await _handler.Object.Handle(request, CancellationToken.None);

            Assert.IsNotNull(retVal);
            Assert.IsTrue(retVal.Message.Contains("Person was found but no duties were found for person"));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task Handler_FindsNoPerson()
        {
            GetAstronautDutiesByName request = new GetAstronautDutiesByName();

            _handler.Setup(x => x.GetPersonAstronaut(It.IsAny<GetAstronautDutiesByName>())).ReturnsAsync((PersonAstronaut)null);

            _handler.Setup(x => x.GetAstronautDuties(It.IsAny<PersonAstronaut>())).ReturnsAsync((List<AstronautDuty>)null);

            var retVal = await _handler.Object.Handle(request, CancellationToken.None);

            Assert.IsNotNull(retVal);
            _mockLogger.Verify(x => x.Log(It.IsAny<string>(), It.IsAny<bool>()), Times.Once());
            _handler.Verify(x => x.GetAstronautDuties(It.IsAny<PersonAstronaut>()), Times.Never);
        }
    }
}
