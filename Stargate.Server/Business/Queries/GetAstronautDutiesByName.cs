using MediatR;
using Stargate.Server.Controllers;
using Stargate.Server.Data.Models;
using Stargate.Server.Data;
using Microsoft.EntityFrameworkCore;
using Stargate.Server.Logging;

namespace Stargate.Server.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;
        private readonly ILoggingWrapper _logger;

        public GetAstronautDutiesByNameHandler(StargateContext context, ILoggingWrapper logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            request.Name = request.Name.Trim();
            var result = new GetAstronautDutiesByNameResult();
            PersonAstronaut? person = await GetPersonAstronaut(request);

            if (person is null)
            {
                string message = $"Could not find person: {request.Name}";
                await _logger.Log(message, false);
                throw new Exception(message);
            }

            result.Person = person;
            List<AstronautDuty>? duties = await GetAstronautDuties(person);

            if (duties is null)
                result.Message = $"Person was found but no duties were found for person: {request.Name}";

            result.AstronautDuties = duties ?? new();

            return result;
        }

        public virtual async Task<List<AstronautDuty>?> GetAstronautDuties(PersonAstronaut person)
        {
            return await _context.AstronautDuties.FromSqlInterpolated($"SELECT * FROM [AstronautDuty] WHERE {person.PersonId} = PersonId Order By DutyStartDate Desc").ToListAsync();
        }

        public virtual async Task<PersonAstronaut?> GetPersonAstronaut(GetAstronautDutiesByName request)
        {
            return await _context.PersonAstronauts.FromSqlInterpolated($"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE {request.Name} = a.Name").FirstOrDefaultAsync();
        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
