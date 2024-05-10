using MediatR;
using Sample.MediatRPipelines.Persistence;
using Sample.MediatRPipelines.Persistence.Repository;

namespace Sample.MediatRPipelines.Domain.Queries.StreamEntityWithFilter;

public class SampleStreamQueryWithPipeFilterHandler
    : IStreamRequestHandler<
        SampleStreamEntityWithPipeFilterQuery,
        SampleStreamEntityWithPipeFilterQueryResult
    >
{
    private readonly IRepository<SampleEntity> _repository;

    public SampleStreamQueryWithPipeFilterHandler(IRepository<SampleEntity> repository)
    {
        _repository = repository;
    }

    public async IAsyncEnumerable<SampleStreamEntityWithPipeFilterQueryResult> Handle(
        SampleStreamEntityWithPipeFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        await foreach (var entity in _repository.GetStream(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            yield return new SampleStreamEntityWithPipeFilterQueryResult()
            {
                Id = entity.Id,
                Description = entity.Description,
                EventTime = entity.RegistrationTime
            };
        }
    }
}
