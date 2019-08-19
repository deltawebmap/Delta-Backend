using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class GitHubIssueCreateRequest
    {
        public string title;
        public string body;
        public string[] labels;
    }

    public class GitHubIssueCreateResponse
    {
        public string html_url;
    }
}
