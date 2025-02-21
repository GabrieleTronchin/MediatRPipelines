using MediatR.Playground.Domain.ExceptionsHandler.Commands;
using System;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.ExceptionsHandler;

internal class GlobalExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<GlobalExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public GlobalExceptionHandlingBehavior(ILogger<GlobalExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"---- Exception Handler: '{nameof(GlobalExceptionHandlingBehavior<TRequest, TResponse>)}'"); 
            throw;
        }
    }
}
