using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities
{
    public class StandardError : Exception
    {
        /// <summary>
        /// Error displayed to the user
        /// </summary>
        public string screen_error;

        /// <summary>
        /// Error code
        /// </summary>
        public StandardErrorCode error_code;

        /// <summary>
        /// String version of the error code
        /// </summary>
        public string error_code_string;

        public Exception inner_error_exception;

        public StandardError(string screen_error, StandardErrorCode error_code, Exception inner = null)
        {
            this.screen_error = screen_error;
            this.error_code = error_code;
            this.inner_error_exception = inner;
        }
    }

    public enum StandardErrorCode
    {
        NotFound = 0,
        NotImplemented = 1,
        UncaughtException = 2,
        InternalSigninError = 3,
        MissingRequiredArg = 4,
        AuthRequired = 5,
        SlaveAuthFailed = 6,
        NotPermitted = 7,
        ExternalAuthError = 8,
        AuthFailed = 9,
        InvalidInput = 10,
        BridgeIntegrityCheckFailed = 11,
        BridgeBackendServerError = 12, //Error code 500
        BridgeBackendServerNetFailed = 13, //Any other error on the bridge
    }
}
