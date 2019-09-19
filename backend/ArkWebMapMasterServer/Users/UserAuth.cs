using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Linq;
using ArkWebMapMasterServer.NetEntities;
using ArkBridgeSharedEntities.Entities;
using LibDeltaSystem.Db.System;
using LibDeltaSystem;

namespace ArkWebMapMasterServer.Users
{
    /// <summary>
    /// This covers authenticating users, but not anything else. Not even tokens.
    /// </summary>
    public static class UserAuth
    {
        public static DbUser GetUserById(string id)
        {
            return Program.connection.GetUserByIdAsync(id).GetAwaiter().GetResult();
        }

        public static DbUser GetUserByAuthName(string id)
        {
            return Program.connection.GetUserBySteamIdAsync(id).GetAwaiter().GetResult();
        }

        public static DbUser CreateUserWithSteam(string steamId, SteamProfile profile)
        {
            //Generate
            DbUser user = new DbUser
            {
                user_settings = new DbUserSettings(),
                profile_image_url = profile.avatarfull,
                steam_profile_url = profile.profileurl,
                screen_name = profile.personaname,
                steam_id = profile.steamid,
                _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                conn = Program.connection
            };

            //Insert in the database
            Program.connection.system_users.InsertOne(user);

            return user;
        }
    }
}
