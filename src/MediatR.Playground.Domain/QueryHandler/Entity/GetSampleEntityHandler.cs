using MediatR.Playground.Model.Queries.Entity;
using MediatR.Playground.Persistence;
using MediatR.Playground.Persistence.Repository;

namespace MediatR.Playground.Domain.QueryHandler.Entity;

public class GetSampleEntityHandler
    : IRequestHandler<GetSampleEntityQuery, GetSampleEntityQueryResult>
{
    private readonly IRepository<SampleEntity> _repository;

    public GetSampleEntityHandler(IRepository<SampleEntity> repository)
    {
        _repository = repository;
    }

    public async Task<GetSampleEntityQueryResult> Handle(
        GetSampleEntityQuery request,
        CancellationToken cancellationToken
    )
    {
        if (request.Id == Guid.Empty)
            throw new ArgumentNullException(nameof(request.Id));

        var entity = await _repository.GetById(request.Id);

        if (entity == null)
            return new GetSampleEntityQueryResult();

        return new GetSampleEntityQueryResult()
        {
            Id = entity.Id,
            Description = entity.Description,
            EventTime = entity.RegistrationTime,
        };
    }
}
