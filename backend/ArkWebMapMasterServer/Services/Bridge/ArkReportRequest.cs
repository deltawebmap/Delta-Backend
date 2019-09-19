﻿using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArkBridgeSharedEntities.Entities;
using Newtonsoft.Json;
using System.Linq;
using ArkBridgeSharedEntities.Entities.Master;
using LibDeltaSystem.Db.System;

namespace ArkWebMapMasterServer.Services.Bridge
{
    public class ArkReportRequest
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s)
        {
            try
            {
                //Open report
                ArkSlaveReport report = Program.DecodePostBody<ArkSlaveReport>(e);

                //Set on server
                s.latest_server_map = report.map_name;
                s.latest_server_time = report.map_time;
                s.has_server_report = true;

                //Save
                s.Update();

                //Also update published server listing
                ArkPublishedServerListing listing = ServerPublishingManager.GetPublishedServer(s.id);
                if(listing != null)
                {
                    if (listing.saved_active_players != report.accounts.Count || listing.saved_total_players != report.accounts.Count)
                    {
                        //Dirty. Update
                        listing.saved_active_players = report.accounts.Count; //TODO: Actually count active players
                        listing.saved_total_players = report.accounts.Count;
                        ServerPublishingManager.SavePublishedServer(listing, s);
                    }
                }

                //Write OK
                return Program.QuickWriteJsonToDoc(e, new TrueFalseReply
                {
                    ok = true
                });
            } catch (Exception ex)
            {
                //Write OK
                return Program.QuickWriteJsonToDoc(e, new TrueFalseReply
                {
                    ok = false
                });
            }

            
        }
    }
}
