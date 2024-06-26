﻿using MediatR;
using Microsoft.Extensions.Logging;
using Sample.MediatRPipelines.Persistence;
using Sample.MediatRPipelines.Persistence.Repository;

namespace Sample.MediatRPipelines.Domain.TransactionCommand;

public class AddSampleEntityCommandHandler
    : IRequestHandler<AddSampleEntityCommand, AddSampleEntityCommandComplete>
{
    private readonly ILogger<AddSampleEntityCommandHandler> _logger;
    private readonly IRepository<SampleEntity> _repository;

    public AddSampleEntityCommandHandler(
        ILogger<AddSampleEntityCommandHandler> logger,
        IRepository<SampleEntity> repository
    )
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<AddSampleEntityCommandComplete> Handle(
        AddSampleEntityCommand request,
        CancellationToken cancellationToken
    )
    {
        await _repository.Add(
            new SampleEntity()
            {
                Id = request.Id,
                Description = request.Description,
                RegistrationTime = request.EventTime
            }
        );

        _logger.LogInformation(
            "Command Executed Id:{Id};Description:{Description};EventTime:{EventTime}",
            request.Id,
            request.Description,
            request.EventTime
        );

        return new AddSampleEntityCommandComplete() { IsSuccess = true };
    }
}
