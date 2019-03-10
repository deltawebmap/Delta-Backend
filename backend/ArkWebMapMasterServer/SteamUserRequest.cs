using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer
{
    public static class SteamUserRequest
    {
        const long CACHE_MAX_AGE_SECONDS = 3600; //One hour

        private static LiteCollection<CachedSteamUser> GetCollection()
        {
            return Program.db.GetCollection<CachedSteamUser>("cached_steam_accounts");
        }

        private static CachedSteamUser TryGetSteamUserFromCache(string id)
        {
            //Returns a Steam user from cache if it exists and is valid.
            var collec = GetCollection();
            var user = collec.FindOne(x => x._id == id);
            if (user == null)
                return null;
            else
            {
                //If this is too old, remove it and return null
                TimeSpan age = DateTime.UtcNow - new DateTime(user.cacheDate);
                if(age.TotalSeconds > CACHE_MAX_AGE_SECONDS)
                {
                    //Devalidate and remove
                    collec.Delete(user._id);
                    return null;
                } else
                {
                    return user;
                }
            }
        }

        public static SteamProfile GetSteamProfile(string id)
        {
            //Try and get it from the cache
            CachedSteamUser cacheUser = TryGetSteamUserFromCache(id);
            if (cacheUser != null)
                return cacheUser.payload;

            //Fetch data
            var steamProfile = SteamAuth.SteamOpenID.RequestSteamUserData(id);

            //Add to cache
            GetCollection().Insert(new CachedSteamUser
            {
                _id = steamProfile.steamid,
                payload = steamProfile,
                cacheDate = DateTime.UtcNow.Ticks
            });

            return steamProfile;
        }
    }
}
