using MediatR;
using Sample.MediatRPipelines.Domain.Queries;
using Sample.MediatRPipelines.Persistence;
using Sample.MediatRPipelines.Persistence.Repository;

namespace Sample.MediatRPipelines.Domain.Commands.SampleRequest;

public class SampleStreamQueryHandler
    : IStreamRequestHandler<SampleStreamEntityQuery, SampleStreamEntityQueryComplete>
{
    private readonly IRepository<SampleEntity> _repository;

    public SampleStreamQueryHandler(IRepository<SampleEntity> repository)
    {
        _repository = repository;
    }

    public async IAsyncEnumerable<SampleStreamEntityQueryComplete> Handle(
        SampleStreamEntityQuery request,
        CancellationToken cancellationToken
    )
    {
        foreach (var entity in await _repository.GetAll())
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            yield return new SampleStreamEntityQueryComplete()
            {
                Id = entity.Id,
                Description = entity.Description,
                EventTime = entity.RegistrationTime
            };
        }
    }
}
