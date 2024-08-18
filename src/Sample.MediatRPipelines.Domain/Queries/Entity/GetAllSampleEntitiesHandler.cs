using MediatR;
using Sample.MediatRPipelines.Persistence;
using Sample.MediatRPipelines.Persistence.Repository;

namespace Sample.MediatRPipelines.Domain.Queries.Entity;

public class GetAllSampleEntitiesHandler
    : IRequestHandler<GetAllSampleEntitiesQuery, IEnumerable<GetAllSampleEntitiesQueryResult>>
{
    private readonly IRepository<SampleEntity> _repository;

    public GetAllSampleEntitiesHandler(IRepository<SampleEntity> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<GetAllSampleEntitiesQueryResult>> Handle(
        GetAllSampleEntitiesQuery request,
        CancellationToken cancellationToken
    )
    {
        return (await _repository.GetAll()).Select(x => new GetAllSampleEntitiesQueryResult()
        {
            Id = x.Id,
            Description = x.Description,
            EventTime = x.RegistrationTime,
        });
    }
}
