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
    public class CreatePersonTests
    {
        Mock<ILoggingWrapper> _mockLogger;
        Mock<StargateContext> _mockContext;
        Mock<DbSet<Person>> _mockPersonSet;
        CreatePersonPreProcessor _preprocessor;
        CreatePersonHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILoggingWrapper>();
            _mockLogger.Setup(x => x.Log(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

            _mockContext = new Mock<StargateContext>();
            _mockPersonSet = new Mock<DbSet<Person>>();

            var people = new List<Person>() 
            {
                new Person { Id = 1, Name = "John Doe" }
            }.AsQueryable();

            _mockPersonSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.Provider);
            _mockPersonSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.Expression);
            _mockPersonSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.ElementType);
            _mockPersonSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(people.GetEnumerator());

            _mockContext.Setup(c => c.People).Returns(_mockPersonSet.Object);

            _preprocessor = new CreatePersonPreProcessor(_mockContext.Object, _mockLogger.Object);
            _handler = new CreatePersonHandler(_mockContext.Object, _mockLogger.Object);
        }


        [TestMethod]
        public async Task Preprocess_WithNewName_ShouldNotThrowException()
        {
            var request = new CreatePerson { Name = "New Person" };

            await _preprocessor.Process(request, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(BadHttpRequestException))]
        public async Task Preprocess_WithNewName_ThrowsException()
        {
            var request = new CreatePerson { Name = "John Doe" };

            await _preprocessor.Process(request, CancellationToken.None);
        }

        [TestMethod]
        public void Handler_flowtest_CreatePerson()
        {
            var request = new CreatePerson { Name = "Gordon Freeman" };

            var retVal = _handler.Handle(request, CancellationToken.None).Result;

            Assert.IsNotNull(retVal);

            _mockPersonSet.Verify(x => x.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once());
            _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
