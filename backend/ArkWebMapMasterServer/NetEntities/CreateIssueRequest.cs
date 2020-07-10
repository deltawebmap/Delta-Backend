using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class CreateIssueRequest
    {
        public string title;
        public string topic;
        public string server_id;

        public string client_info;
        public string client_name;

        public string screenshot_token;
        public string[] attachment_tokens;
        public string[] attachment_names;

        public string body_description;
        public string body_expected;
    }

    public class CreateIssueResponse
    {
        public string url;
    }
}
