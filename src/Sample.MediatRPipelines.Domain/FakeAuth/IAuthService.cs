namespace Sample.MediatRPipelines.Domain.FakeAuth;

public interface IAuthService
{
    AuthResponse OperationAlowed();
}