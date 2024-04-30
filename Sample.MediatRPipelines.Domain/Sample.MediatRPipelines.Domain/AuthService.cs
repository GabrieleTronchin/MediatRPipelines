using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
