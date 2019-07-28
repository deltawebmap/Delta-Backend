using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapAnalytics
{
    public class StandardError : Exception
    {
        public string errorMsg;
        public int errorCode;

        public StandardError(string msg, int code = 500)
        {
            errorMsg = msg;
            errorCode = code;
        }
    }
}
