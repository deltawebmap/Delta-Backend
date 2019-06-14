using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapLightspeed
{
    class ServerValidationRequestPayload
    {
        public string server_id;
        public string server_creds; //Base 64 encoded creds for the server
    }

    public class ServerValidationResponsePayload
    {
        public string server_id;
        public string server_name;
        public string server_owner_id;
        public bool has_icon;
        public string icon_url;
    }
}
