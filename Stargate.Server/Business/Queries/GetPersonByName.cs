using MediatR;
using Stargate.Server.Controllers;
using Stargate.Server.Data.Models;
using Stargate.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Stargate.Server.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;
        public GetPersonByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            var result = new GetPersonByNameResult();

            var person = await _context.PersonAstronauts.FromSql(@$"SELECT a.Id as PersonId
                                                                        ,a.Name
                                                                        ,b.CurrentRank
                                                                        ,b.CurrentDutyTitle
                                                                        ,b.CareerStartDate
                                                                        ,b.CareerEndDate 
                                                                    FROM [Person] a 
                                                                    LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id
                                                                    WHERE {request.Name} = a.Name").FirstOrDefaultAsync();

            result.Person = person;

            return result;
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}
