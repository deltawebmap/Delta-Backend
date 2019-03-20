using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer
{
    public class StandardError : Exception
    {
        public string screen_error;
        public string screen_error_detailed;

        public string standard_error_string;
        public StandardErrorType standard_error;

        public StandardError(StandardErrorType type, string msg, string msg_more)
        {
            this.screen_error = msg;
            this.screen_error_detailed = msg_more;
            this.standard_error = type;
            this.standard_error_string = type.ToString();
        }
    }

    public enum StandardErrorType
    {
        NotFound,
        MissingArg,
        InvalidArg,
        ServerError,
        UncaughtException
    }
}
