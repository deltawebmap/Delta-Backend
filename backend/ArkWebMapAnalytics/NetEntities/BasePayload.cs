using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapAnalytics.NetEntities
{
    public class BasePayload
    {
        public string access_token; //Token
        public string client_name; //The name of the client, such as "web", or "android"
        public string client_version; //The version of the client
        public string client_view; //The page we're on
        public string client_details; //User agent
    }
}
