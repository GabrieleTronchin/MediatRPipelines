namespace Sample.MediatRPipelines.Domain
{
    public interface IAuthService
    {
        AuthResponse OperationAlowed();
    }
}