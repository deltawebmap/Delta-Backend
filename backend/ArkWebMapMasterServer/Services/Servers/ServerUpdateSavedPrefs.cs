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
    public class ServerUpdateSavedPrefs : MasterTribeServiceTemplate
    {
        public ServerUpdateSavedPrefs(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Ensure method
            if(GetMethod() != LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
            {
                await WriteString("Unsupported Method", "text/plain", 400);
                return;
            }
            
            //Deserialize
            SavedUserServerPrefs prefs = await DecodePOSTBody<SavedUserServerPrefs>();

            //Update in db
            var filterBuilder = Builders<DbSavedUserServerPrefs>.Filter;
            var filter = filterBuilder.Eq("server_id", server.id) & filterBuilder.Eq("user_id", user.id);
            await Program.connection.system_saved_user_server_prefs.ReplaceOneAsync(filter, new DbSavedUserServerPrefs
            {
                user_id = user.id,
                server_id = server.id,
                payload = prefs
            }, new UpdateOptions
            {
                IsUpsert = true
            });

            //Return prefs
            await WriteJSON(prefs);
        }
    }
}
