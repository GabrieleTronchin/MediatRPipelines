using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Sample.MediatRPipelines.Domain.Primitives;

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
