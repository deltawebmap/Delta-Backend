using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class ServerSetupWizard_BeginReply
    {
        public string display_id;
        public bool ok;
        public string request_url;
        public DbServer server;
    }

    public class ServerSetupWizard_BeginReplyHeadless : ServerSetupWizard_BeginReply
    {
        public string headless_config_url;
    }
}
