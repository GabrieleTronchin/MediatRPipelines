namespace Sample.MediatRPipelines.Domain
{
    internal class AuthService : IAuthService
    {
        public AuthService() { }


        public bool OperationAlowed()
        {
            //TODO Add some custom logic
            return true;
        }
    }
}
