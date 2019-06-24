using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapLightspeedClient
{
    public class LightspeedConfigFile
    {
        public string server_endpoint;
        public int reconnect_delay_seconds;
        public int buffer_size;
        public string client_endpoint_prefix;
    }
}
