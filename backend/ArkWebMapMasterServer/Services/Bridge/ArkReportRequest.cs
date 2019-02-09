using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArkBridgeSharedEntities.Entities;

namespace ArkWebMapMasterServer.Services.Bridge
{
    public class ArkReportRequest
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s)
        {
            //Open report
            ArkSlaveReport report = Program.DecodePostBody<ArkSlaveReport>(e);

            //Set on server
            s.latest_server_local_accounts = report.accounts;
            s.latest_server_map = report.map_name;
            s.latest_server_time = report.map_time;
            s.latest_server_report_downloaded = DateTime.UtcNow.Ticks;
            s.has_server_report = true;

            //Save
            s.Update();

            //Write OK
            return Program.QuickWriteJsonToDoc(e, new TrueFalseReply
            {
                ok = true
            });
        }
    }
}
