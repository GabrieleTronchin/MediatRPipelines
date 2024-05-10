using MediatR;

namespace Sample.MediatRPipelines.Domain.Queries.StreamEntityWithFilter
{
    public class SampleStreamEntityWithPipeFilterQuery : IStreamRequest<SampleStreamEntityWithPipeFilterQueryResult> { }

    public record SampleStreamEntityWithPipeFilterQueryResult
    {
        public Guid Id { get; set; }

        public DateTime EventTime { get; set; }

        public string Description { get; set; }
    }
}
