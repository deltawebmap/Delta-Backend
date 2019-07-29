using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer
{
    public static class ClientVersion
    {
        /// <summary>
        /// Version of data being returned. If any changes are made to the format that may break older versions, this is added to.
        /// </summary>
        public const int DATA_VERSION = 2;




        /// <summary>
        /// Version of this software
        /// </summary>
        public const int VERSION_MAJOR = 1;

        /// <summary>
        /// Version of this software
        /// </summary>
        public const int VERSION_MINOR = 3;
    }
}
