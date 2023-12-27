using AccountsUpdate.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using AccountsUpdate.Domain.Common;
using AccountsUpdate.Application.Extensions;
using System.Reflection.Metadata.Ecma335;
using MassTransit.Contracts;

namespace AccountsUpdate.Application.AccountUpdate.Commands
{
    public sealed record UpdateAccountRequest(string message, string? correlationId = null) : IRequest<bool>
    {

        public UpdateAccountRequest(string message) : this(message, Guid.NewGuid().ToString()) { }

    }
    public class UpdateAccountRequestHandler : IRequestHandler<UpdateAccountRequest, bool>
    {
        private readonly ILogger _logger;
        private readonly IUpdateService _updateService;

        public UpdateAccountRequestHandler(ILogger logger, IUpdateService updateService)
        {
            _logger = logger;
            _updateService = updateService;
        }
        public async Task<bool> Handle(UpdateAccountRequest request, CancellationToken cancellationToken)
        {
            //_logger.LogInformation("req {@req}", request);
            //var sleepTime = new Random().Next(1, 10);
            _logger.LogInformation("{@request} starting the handlin {@logDetails}", request, LogEvent.HandingReuest.AddCorelationId(request.correlationId));
            //Thread.Sleep(sleepTime * 1000);
            return _updateService.UpdateAccount(new TrackableMessage(request.message, request.correlationId))
                .Match(
                    rowsAffected =>
                    {
                        switch (rowsAffected)
                        {
                            case 0:
                                _logger.LogError("error {@logDetails} ", AccountErrors.UnAffected.AddCorelationId(request.correlationId));
                                return true;
                            case 1:
                                _logger.LogInformation("handled {@logDetails} ", AccountEvents.SingleAffected.AddCorelationId(request.correlationId));
                                return true;
                            default:
                                _logger.LogError("error {@logDetails} ", AccountErrors.MultipleAffected.AddCorelationId(request.correlationId));
                                return true;
                        }
                    },
                    error =>
                    {
                        _logger.LogError("error {@logDetails} ", error.AddCorelationId(request.correlationId));
                        return false;
                    }
                );


        }
    }
}