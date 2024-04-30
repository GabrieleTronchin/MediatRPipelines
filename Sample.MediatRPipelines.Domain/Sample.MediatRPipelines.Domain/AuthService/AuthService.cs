using Bogus;

namespace Sample.MediatRPipelines.Domain.AuthService
{

    public class AuthResponse()
    {
        public bool IsSuccess { get; set; }

        public Exception? Exception { get; set; }

    }

    internal class AuthService : IAuthService
    {
        public AuthService() { }


        Exception[] exceptions = {
           new InvalidOperationException(),
           new InvalidDataException(),
           new ArgumentOutOfRangeException(),
           new ArgumentException(),
        };



        public AuthResponse OperationAlowed()
        {
            return new Faker<AuthResponse>().Rules((f, x) =>
            {
                x.IsSuccess = f.Random.Bool();

                if (!x.IsSuccess)
                {
                    x.Exception = f.PickRandom(exceptions);
                }
            }).Generate();
        }

    }




}
