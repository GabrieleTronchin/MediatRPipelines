using MediatR;
using Sample.MediatRPipelines.Persistence;
using Sample.MediatRPipelines.Persistence.Repository;

namespace Sample.MediatRPipelines.Domain.Queries.StreamEntity;

public class SampleStreamQueryHandler
    : IStreamRequestHandler<SampleStreamEntityQuery, SampleStreamEntityQueryResult>
{
    private readonly IRepository<SampleEntity> _repository;

    public SampleStreamQueryHandler(IRepository<SampleEntity> repository)
    {
        _repository = repository;
    }

    public async IAsyncEnumerable<SampleStreamEntityQueryResult> Handle(
        SampleStreamEntityQuery request,
        CancellationToken cancellationToken
    )
    {
        await foreach (var entity in _repository.GetStream(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            yield return new SampleStreamEntityQueryResult()
            {
                Id = entity.Id,
                Description = entity.Description,
                EventTime = entity.RegistrationTime,
            };
        }
    }
}
