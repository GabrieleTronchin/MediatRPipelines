using MediatR;
using Sample.MediatRPipelines.Domain.Primitives;

namespace Sample.MediatRPipelines.Domain.PipelineBehavior
{
    public class CommandAuthorizationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
    {
        private readonly IAuthService _authService;

        public CommandAuthorizationBehavior(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_authService.OperationAlowed())
                throw new NotImplementedException();

            return await next();

        }
    }
}
