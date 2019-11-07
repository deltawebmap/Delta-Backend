using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Linq;
using ArkWebMapMasterServer.NetEntities;

using LibDeltaSystem.Db.System;
using LibDeltaSystem;
using System.Threading.Tasks;

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

        public static async Task<DbUser> CreateUserWithSteam(string steamId, DbSteamCache profile)
        {
            //Generate
            DbUser user = new DbUser
            {
                user_settings = new DbUserSettings(),
                profile_image_url = profile.icon_url,
                steam_profile_url = profile.profile_url,
                screen_name = profile.name,
                steam_id = profile.steam_id,
                _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                conn = Program.connection
            };

            //Insert in the database
            await Program.connection.system_users.InsertOneAsync(user);

            return user;
        }
    }
}
