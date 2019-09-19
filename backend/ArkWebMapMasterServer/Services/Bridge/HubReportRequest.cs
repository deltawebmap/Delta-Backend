using ArkBridgeSharedEntities.Entities.BasicTribeLog;
using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Bridge
{
    public static class HubReportRequest
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s)
        {
            //Decode the POST body
            BasicTribeLogSubmission submission = Program.DecodePostBody<BasicTribeLogSubmission>(e);

            //Add each submission to DB
            var collec = Tools.TribeHubTool.GetCollection();
            foreach (var sub in submission.entries)
            {
                sub.serverId = s.id;
                collec.Insert(sub);
            }

            //Return OK
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
