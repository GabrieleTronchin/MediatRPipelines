using Bogus;

namespace Sample.MediatRPipelines.Domain.FakeAuth;

public class AuthService : IAuthService
{
    Exception[] exceptions =
    {
        new InvalidOperationException(),
        new InvalidDataException(),
        new ArgumentOutOfRangeException(),
        new ArgumentException(),
    };

    public AuthResponse OperationAlowed()
    {
        return new Faker<AuthResponse>()
            .Rules(
                (f, x) =>
                {
                    x.IsSuccess = f.Random.Bool();

                    if (!x.IsSuccess)
                    {
                        x.Exception = f.PickRandom(exceptions);
                    }
                }
            )
            .Generate();
    }
}
