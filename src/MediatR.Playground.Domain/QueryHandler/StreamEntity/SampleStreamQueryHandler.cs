using MediatR;
using MediatR.Playground.Model.Queries.StreamEntity;
using MediatR.Playground.Persistence;
using MediatR.Playground.Persistence.Repository;

namespace MediatR.Playground.Domain.QueryHandler.StreamEntity;

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
