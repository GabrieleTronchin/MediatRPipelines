namespace MediatR.Playground.Model.Queries.StreamEntity
{
    public class SampleStreamEntityQuery : IStreamRequest<SampleStreamEntityQueryResult> { }

    public record SampleStreamEntityQueryResult
    {
        public Guid Id { get; set; }

        public DateTime EventTime { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
