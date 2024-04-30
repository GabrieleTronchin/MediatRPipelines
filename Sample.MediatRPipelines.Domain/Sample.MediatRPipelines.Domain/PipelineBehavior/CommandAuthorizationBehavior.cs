using MediatR;
using Sample.MediatRPipelines.Domain.Primitives;

namespace Sample.MediatRPipelines.Domain.PipelineBehavior
{
    public class CommandAuthorizationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
    {

        public CommandAuthorizationBehavior()
        {
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
