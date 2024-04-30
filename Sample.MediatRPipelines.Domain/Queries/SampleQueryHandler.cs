using MediatR;
using Sample.MediatRPipelines.Persistence;
using Sample.MediatRPipelines.Persistence.Repository;

namespace Sample.MediatRPipelines.Domain.Commands.SampleRequest;

public class SampleQueryHandler : IRequestHandler<SampleEntityQuery, IEnumerable<SampleEntityQueryComplete>>
{
    private readonly IRepository<SampleEntity> _repository;

    public SampleQueryHandler(IRepository<SampleEntity> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SampleEntityQueryComplete>> Handle(SampleEntityQuery request, CancellationToken cancellationToken)
    {
        return (await _repository.GetAll())
               .Select(x => new SampleEntityQueryComplete()
               {
                   Id = x.Id,
                   Description = x.Description,
                   EventTime = x.RegistrationTime
               });
    }

}