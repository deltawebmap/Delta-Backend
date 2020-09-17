using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public class FetchPlatformProfileService : UserAuthDeltaService
    {
        public FetchPlatformProfileService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Read data
            RequestData request = await DecodePOSTBody<RequestData>();

            //Decode all tokens
            List<string> requestSteamIds = new List<string>();
            List<ResponseData_Item> responses = new List<ResponseData_Item>();
            foreach(var t in request.tokens)
            {
                //Read token data
                var tokenData = conn.ReadSteamIdTokenString(t);

                //If read correctly, queue steam ID
                if(tokenData != null)
                {
                    if (!requestSteamIds.Contains(tokenData.steam_id))
                        requestSteamIds.Add(tokenData.steam_id);
                }

                //Add to responses
                responses.Add(new ResponseData_Item
                {
                    ok = false,
                    token = t,
                    profile = null,
                    _tokenData = tokenData
                });
            }
            
            //Lookup profiles
            var profiles = await conn.BulkGetSteamProfiles(requestSteamIds);

            //Figure out where to put profiles
            foreach(var p in responses)
            {
                //Ignore if this token failed
                if (p._tokenData == null)
                    continue;

                //Find steam profile
                if(profiles.ContainsKey(p._tokenData.steam_id))
                {
                    p.profile = profiles[p._tokenData.steam_id];
                    p.ok = true;
                }
            }

            //Write response
            await WriteJSON(new ResponseData
            {
                profiles = responses
            });
        }

        class RequestData
        {
            public string[] tokens;
        }

        class ResponseData
        {
            public List<ResponseData_Item> profiles;
        }

        class ResponseData_Item
        {
            public string token;
            public bool ok;
            public DbSteamCache profile;

            [JsonIgnoreAttribute]
            public SteamIdToken _tokenData;
        }
    }
}
