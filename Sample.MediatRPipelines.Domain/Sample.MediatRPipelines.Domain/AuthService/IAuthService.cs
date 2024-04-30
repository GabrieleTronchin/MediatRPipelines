namespace Sample.MediatRPipelines.Domain.AuthService
{
    public interface IAuthService
    {
        AuthResponse OperationAlowed();
    }
}