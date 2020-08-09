using ArkWebMapMasterServer.ServiceTemplates;
using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.RPC.Payloads;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class PutDinoPrefsRequest : MasterTribeServiceTemplate
    {
        public PutDinoPrefsRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Deserialize
            RequestData request = await ReadPOSTContentChecked<RequestData>();
            if (request == null)
                return;

            //Parse ID
            if(!ulong.TryParse(request.dino_id, out ulong dinoId))
            {
                await WriteString("Invaid dino ID string.", "text/plain", 400);
                return;
            }

            //Validate
            if (request.prefs == null)
            {
                await WriteString("Prefs cannot be null.", "text/plain", 400);
                return;
            }

            //Find dino
            DbDino dino = await DbDino.GetDinosaurByID(conn, dinoId, server);
            if(dino == null)
            {
                await WriteString("Dino ID not found.", "text/plain", 400);
                return;
            }

            //Update
            await dino.UpdatePrefs(conn, server, request.prefs);

            //Return prefs
            await WriteJSON(dino.prefs);
        }

        class RequestData
        {
            public string dino_id;
            public SavedDinoTribePrefs prefs;
        }
    }
}
