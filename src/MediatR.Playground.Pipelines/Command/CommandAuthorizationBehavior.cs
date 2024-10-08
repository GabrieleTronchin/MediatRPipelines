﻿using FakeAuth.Service;
using MediatR.Playground.Model.Primitives.Request;

namespace MediatR.Playground.Pipelines.Command;

public class CommandAuthorizationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IAuthService _authService;

    public CommandAuthorizationBehavior(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var response = _authService.OperationAlowed();

        if (!response.IsSuccess)
            throw response.Exception ?? new Exception();

        return await next();
    }
}
