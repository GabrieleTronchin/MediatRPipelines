﻿using MediatR;

namespace Sample.MediatRPipelines.Domain.Queries
{
    public class SampleStreamEntityQuery : IStreamRequest<SampleStreamEntityQueryComplete> { }

    public record SampleStreamEntityQueryComplete
    {
        public Guid Id { get; set; }

        public DateTime EventTime { get; set; }

        public string Description { get; set; }
    }
}
