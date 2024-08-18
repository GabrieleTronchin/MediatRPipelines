namespace FakeAuth.Service;

public class AuthResponse()
{
    public bool IsSuccess { get; set; }

    public Exception? Exception { get; set; }
}
