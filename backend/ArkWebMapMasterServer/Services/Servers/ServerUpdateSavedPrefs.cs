using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.RPC.Payloads;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public static class ServerUpdateSavedPrefs
    {
        public static async Task OnUserPrefsRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s, DbUser u)
        {
            //Deserialize
            SavedUserServerPrefs prefs = Program.DecodePostBody<SavedUserServerPrefs>(e);

            //Update in db
            var filterBuilder = Builders<DbSavedUserServerPrefs>.Filter;
            var filter = filterBuilder.Eq("server_id", s.id) & filterBuilder.Eq("user_id", u.id);
            await Program.connection.system_saved_user_server_prefs.ReplaceOneAsync(filter, new DbSavedUserServerPrefs
            {
                user_id = u.id,
                server_id = s.id,
                payload = prefs
            }, new UpdateOptions
            {
                IsUpsert = true
            });

            //Return prefs
            await Program.QuickWriteJsonToDoc(e, prefs);
        }

        public static async Task OnTribeDinoPrefsRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s, DbUser u, int tribeId, string next)
        {
            //Get the dinosaur ID
            ulong id = ulong.Parse(next);

            //Decode data from the POST
            SavedDinoTribePrefs prefs = Program.DecodePostBody<SavedDinoTribePrefs>(e);

            //Now, create an entry to insert into the db
            var filterBuilder = Builders<DbSavedDinoTribePrefs>.Filter;
            var filter = filterBuilder.Eq("server_id", s.id) & filterBuilder.Eq("dino_id", id) & filterBuilder.Eq("tribe_id", tribeId);
            await Program.connection.system_saved_dino_tribe_prefs.ReplaceOneAsync(filter, new DbSavedDinoTribePrefs
            {
                tribe_id = tribeId,
                dino_id = id,
                server_id = s.id,
                payload = prefs
            }, new UpdateOptions
            {
                IsUpsert = true
            });

            //Notify on RPC
            Program.connection.GetRPC().SendRPCMessageToTribe(LibDeltaSystem.RPC.RPCOpcode.DinoPrefsChanged, new RPCPayloadDinoPrefsUpdate
            {
                dino_id = id.ToString(),
                prefs = prefs,
                user_id = u.id
            }, s, tribeId);

            //Return prefs
            await Program.QuickWriteJsonToDoc(e, prefs);
        }
    }
}
