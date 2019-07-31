using ArkBridgeSharedEntities.Entities;
using ArkHttpServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Tools
{
    public static class SteamUserData
    {
        public static SteamProfile GetSteamProfile(string id)
        {
            //Get collection
            var collec = ArkWebServer.db.GetCollection<CachedSteamUser>("cached_steam_users");

            //Find, if it exists
            var result = collec.FindById(id);

            //Check if it is valid
            if(result != null)
            {
                //Check 
                if (result.expire_time <= DateTime.UtcNow.Ticks)
                    return result.payload;

                //Failed. Remove
                collec.Delete(id);
            }

            //We'll need to fetch it.
            var data = SteamIdLookup.MassFetchPlayerData(new List<string>
            {
                id
            }).GetAwaiter().GetResult()[0];

            //Verify
            if (data == null)
                throw new Exception("No Steam ID found.");

            //Insert to DB
            collec.Insert(new CachedSteamUser
            {
                expire_time = DateTime.UtcNow.AddDays(1).Ticks,
                payload = data,
                _id = id
            });

            //Respond with data
            return data;
        }
    }
}
