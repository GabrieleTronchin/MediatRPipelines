using Bogus;
using Microsoft.Extensions.Configuration;

namespace FakeAuth.Service;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    Exception[] exceptions =
    {
        new InvalidOperationException(),
        new InvalidDataException(),
        new ArgumentOutOfRangeException(),
        new ArgumentException(),
    };

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthResponse OperationAlowed()
    {
        if (_configuration.GetValue<bool>("FakeAuth:AlwaysAuthorize"))
        {
            return new AuthResponse { IsSuccess = true };
        }

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
