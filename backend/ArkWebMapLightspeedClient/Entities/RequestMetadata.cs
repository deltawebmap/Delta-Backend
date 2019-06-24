using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapLightspeedClient.Entities
{
    public class RequestMetadata
    {
        public int requestToken;
        public string method;
        public MasterServerArkUser auth;
        public int version;
        public string endpoint;
        public Dictionary<string, string> query;
    }
}
