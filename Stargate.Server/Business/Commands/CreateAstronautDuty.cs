using MediatR.Pipeline;
using MediatR;
using Stargate.Server.Data.Models;
using Stargate.Server.Data;
using Stargate.Server.Controllers;
using Microsoft.EntityFrameworkCore;
using Stargate.Server.Logging;
using Microsoft.Data.Sqlite;

namespace Stargate.Server.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;
        private readonly ILoggingWrapper _logger;

        public CreateAstronautDutyPreProcessor(StargateContext context, ILoggingWrapper logging)
        {
            _context = context;
            _logger = logging;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            Person? person = GetPerson(request);

            if (person is null)
            {
                BadHttpRequestException ex = new BadHttpRequestException($"Bad Request: {request.Name} Does not exist...");
                _logger.Log(ex.Message, false);
                throw ex;
            }

            AstronautDuty? previousDutyOfSameTitle = SearchForRepeatDuty(request, person);

            if (previousDutyOfSameTitle is not null)
            {
                BadHttpRequestException ex = new BadHttpRequestException("Bad Request: Person is already assigned that duty");
                _logger.Log(ex.Message, false);
                throw ex;
            }

            return Task.CompletedTask;
        }
        #region EntityGets
        public virtual AstronautDuty? SearchForRepeatDuty(CreateAstronautDuty request, Person person)
        {
            return _context.AstronautDuties.FirstOrDefault(x => x.PersonId == person.Id && x.DutyTitle == request.DutyTitle && x.DutyEndDate == null);
        }

        public virtual Person? GetPerson(CreateAstronautDuty request)
        {
            return _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);
        }
        #endregion
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;
        private readonly ILoggingWrapper _logger;

        public CreateAstronautDutyHandler(StargateContext context, ILoggingWrapper logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            try
            {
                request.Name = request.Name.Trim();

                (Person? person, AstronautDetail? astronautDetail) = await GetPersonInfo(request);

                if (astronautDetail == null)
                {
                    astronautDetail = new AstronautDetail();
                    astronautDetail.PersonId = person?.Id ?? -1;
                    astronautDetail.CurrentDutyTitle = request.DutyTitle;
                    astronautDetail.CurrentRank = request.Rank;
                    astronautDetail.CareerStartDate = request.DutyStartDate.Date;
                    if (request.DutyTitle == "RETIRED")
                    {
                        astronautDetail.CareerEndDate = request.DutyStartDate.Date;
                    }

                    await UpdateDetails(astronautDetail);
                }
                else
                {
                    astronautDetail.CurrentDutyTitle = request.DutyTitle;
                    astronautDetail.CurrentRank = request.Rank;
                    if (request.DutyTitle == "RETIRED")
                    {
                        astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                    }
                    await SaveDetails(astronautDetail);
                }

                AstronautDuty? astronautDuty = await GetAstronautDuties(person);

                if (astronautDuty != null)
                {
                    astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                    await UpdateAstronautDuties(astronautDuty);
                }

                var newAstronautDuty = new AstronautDuty()
                {
                    PersonId = person?.Id ?? -1,
                    Rank = request.Rank,
                    DutyTitle = request.DutyTitle,
                    DutyStartDate = request.DutyStartDate.Date,
                    DutyEndDate = null
                };

                await SaveNewAstronautDuties(newAstronautDuty);
                await _logger.Log($"Successfully set AstronautDuties for {request.Name}", true);

                return new CreateAstronautDutyResult()
                {
                    Id = newAstronautDuty.Id
                };
            }
            catch (Exception ex)
            {
                string message = $"There was a problem settings AstronautDuties for {request.Name} : {ex.Message}";
                await _logger.Log(message, false);
                return new CreateAstronautDutyResult()
                {
                    Id = -1,
                    Message = ex.Message,
                };
            }
        }
        #region EntityRetrieve
        public virtual async Task<AstronautDuty?> GetAstronautDuties(Person? person)
        {
            return await _context.AstronautDuties.FromSql($"SELECT * FROM [AstronautDuty] WHERE {person?.Id ?? -1} = PersonId Order By DutyStartDate Desc").FirstOrDefaultAsync();
        }

        public virtual async Task<(Person? person, AstronautDetail? astronautDetail)> GetPersonInfo(CreateAstronautDuty request)
        {
            Person? person = await _context.People.FromSql($"SELECT * FROM [Person] WHERE {request.Name} = Name").FirstOrDefaultAsync();

            AstronautDetail? astronautDetail = await _context.AstronautDetails.FromSql($"SELECT * FROM [AstronautDetail] WHERE {person?.Id ?? -1} = PersonId").FirstOrDefaultAsync();

            return (person, astronautDetail);
        }
        #endregion
        #region EntitySaveAdd
        public virtual async Task SaveNewAstronautDuties(AstronautDuty newAstronautDuty)
        {
            await _context.AstronautDuties.AddAsync(newAstronautDuty);
            await _context.SaveChangesAsync();
        }

        public virtual Task UpdateAstronautDuties(AstronautDuty? astronautDuty)
        {
            _context.AstronautDuties.Update(astronautDuty);
            return Task.CompletedTask;
        }

        public virtual async Task UpdateDetails(AstronautDetail? astronautDetail)
        {
            await _context.AstronautDetails.AddAsync(astronautDetail);
        }

        public virtual Task SaveDetails(AstronautDetail? astronautDetail)
        {
            _context.AstronautDetails.Update(astronautDetail);
            return Task.CompletedTask;
        }
        #endregion
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
