using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Requests;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.SteamAuth;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Bridge
{
    public static class MassRequestSteamDataRequest
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s)
        {
            //Decode body
            MassFetchSteamDataPayload payload = Program.DecodePostBody<MassFetchSteamDataPayload>(e);

            //Loop through and return each user
            List<SteamProfile> profiles = new List<SteamProfile>();
            foreach(string id in payload.ids)
            {
                try
                {
                    profiles.Add(SteamUserRequest.GetSteamProfile(id));
                } catch
                {
                    //Ignore.
                }
            }

            //Write this
            return Program.QuickWriteJsonToDoc(e, profiles);
        }
    }
}
