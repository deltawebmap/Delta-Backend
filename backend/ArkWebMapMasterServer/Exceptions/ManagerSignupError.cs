using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.Exceptions
{
    public class ManagerSignupError : Exception
    {
        public string error;
        public string element;

        public ManagerSignupError(string msg, string element)
        {
            this.error = msg;
            this.element = element;
        }
    }
}
