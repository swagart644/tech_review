using MediatR.Pipeline;
using MediatR;
using Stargate.Server.Controllers;
using Stargate.Server.Data.Models;
using Stargate.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using Stargate.Server.Logging;

namespace Stargate.Server.Business.Commands
{
    public class CreatePerson : IRequest<CreatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
    {
        private readonly StargateContext _context;
        private readonly ILoggingWrapper _logger;
        public CreatePersonPreProcessor(StargateContext context, ILoggingWrapper logging)
        {
            _context = context;
            _logger = logging;
        }
        public Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            request.Name = request.Name.Trim();

            Person? person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is not null) 
            { 
                BadHttpRequestException ex = new BadHttpRequestException("Bad Request: Person already exists with that name");
                _logger.Log(ex.Message, false);
                throw ex;
            }

            return Task.CompletedTask;
        }
    }

    public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
    {
        private readonly StargateContext _context;
        private readonly ILoggingWrapper _logger;

        public CreatePersonHandler(StargateContext context, ILoggingWrapper logging)
        {
            _context = context;
            _logger = logging;
        }
        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            bool success = false;
            string message = string.Empty;
            int personId = -1;

            Person newPerson = new Person()
            {
                Name = request.Name.Trim()
            };

            try
            {
                await _context.People.AddAsync(newPerson);
                int rows = await _context.SaveChangesAsync();

                if (rows > 0)
                {
                    message = $"Successfully created {request.Name}.";
                    success = true;
                } else
                    throw new Exception($"Failed to create Person: {request.Name}");
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            await _logger.Log(message, success);

            personId = newPerson?.Id ?? -1;

            return new CreatePersonResult()
            {
                Id = personId,
                Success = success,
                Message = message
            };
        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
