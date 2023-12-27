
using Microsoft.Extensions.Logging;
using MediatR;
using AccountsUpdate.Application.Services;
using AccountsUpdate.Application.AccountUpdate.Commands;
using AccountsUpdate.Application.Extensions;
using AccountsUpdate.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace AccountsUpdate.Application.Pipelines;
public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    public UnhandledExceptionBehaviour(ILogger<TRequest> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;

    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is UpdateAccountRequest UAReq)
        {
            try
            {
                _logger.LogInformation("incomming req to pipeline {@logDetails}", LogEvent.Incomming.AddCorelationId(UAReq.correlationId));
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogError(ex, "CleanArchitecture Request: Unhandled Exception for Request {Name} {@Request} {@logDetails}", requestName, request, AccountErrors.Unhandled.AddCorelationId(UAReq.correlationId));

                //throw;
                return default;
            }
        }
        return await next();

    }
}