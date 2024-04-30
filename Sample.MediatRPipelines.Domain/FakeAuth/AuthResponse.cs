namespace Sample.MediatRPipelines.Domain.FakeAuth;

public class AuthResponse()
{
    public bool IsSuccess { get; set; }

    public Exception? Exception { get; set; }

}
