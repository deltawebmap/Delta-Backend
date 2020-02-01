using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public static class ManageRequest
    {
        public const string ICON_APPLICATION_ID = "t3VXa599Na64w7vd";

        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s, DbUser u)
        {
            //Check scope
            await Program.CheckTokenScope(u, null);

            //Validate that this user owns this server
            if (s.owner_uid != u.id)
            {
                throw new StandardError("You do not own this server and are not allowed to perform this action.", StandardErrorCode.NotPermitted);
            }

            //Get method
            var method = Program.FindRequestMethod(e);
            if (method == RequestHttpMethod.get)
                await OnGETRequest(e, s, u);
            else if (method == RequestHttpMethod.post)
                await OnPOSTRequest(e, s, u);
            else
                throw new StandardError("This method was not expected.", StandardErrorCode.BadMethod);
        }

        public static async Task OnGETRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s, DbUser u)
        {
            //Get cluster, if any
            DbCluster cluster = null;
            if (s.cluster_id != null)
                cluster = await DbCluster.GetClusterById(Program.connection, MongoDB.Bson.ObjectId.Parse(s.cluster_id));

            //Get map entry
            string mapName = null;
            var mapEntry = await Program.connection.GetARKMapByInternalName(s.latest_server_map);
            if (mapEntry != null)
                mapName = mapEntry.displayName;

            //Get server status
            string status = "ONLINE";
            string message = "Online";
            if (s.CheckLockFlag(1))
            {
                status = "ALERT";
                message = "Locked";
            }
            if(s.CheckLockFlag(0))
            {
                status = "ALERT";
                message = "Still Initializing...";
            }
            if (s.CheckLockFlag(2))
            {
                status = "OFFLINE";
                message = "Locked by Admin";
            }
            if (mapEntry == null)
            {
                status = "OFFLINE";
                message = "Incompatible Map";
            }

            //Check last connect time
            DateTime lastConnectTime = new DateTime(s.last_client_connect_time);
            if ((DateTime.UtcNow - lastConnectTime).TotalMinutes > 2.5f)
            {
                status = "OFFLINE";
                message = $"Offline Since {lastConnectTime.ToShortDateString()}";
            }

            //Look up all mods used and convert
            Dictionary<string, DbSteamModCache> mods = await s.GetAllServerMods(Program.connection, true);
            List<WebArkMod> modList = new List<WebArkMod>();
            foreach(var mod in mods.Values)
            {
                if (mod == null)
                    modList.Add(null);
                else
                    modList.Add(mod.GetWebVersion());
            }

            //Look up all admins
            List<ManagementData_User> admins = new List<ManagementData_User>();
            admins.Add(await ManagementData_User.GetUser(u));
            foreach (var id in s.admins)
                admins.Add(await ManagementData_User.GetUser( await DbUser.GetUserByID( Program.connection, id )));

            ManagementDataResponse m = new ManagementDataResponse
            {
                name = s.display_name,
                icon = s.image_url,
                is_user_locked = s.CheckLockFlag(1),
                permissions = s.GetPermissionFlagList(),
                settings = s.game_settings,
                cluster_id = s.cluster_id,
                cluster = cluster,
                status = status,
                alert = message,
                mods = modList,
                admins = admins,
                map_id = s.latest_server_map,
                map_name = mapName
            };
            await Program.QuickWriteJsonToDoc(e, m);
        }

        public static async Task OnPOSTRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s, DbUser u)
        {
            //Decode settings
            var settings = Program.DecodePostBody<ManagementDataRequest>(e);

            //Set name
            if (settings.name != null)
                s.display_name = settings.name;

            //Set icon
            if (settings.icon_token == "%CLEAR_ICON") {
                s.image_url = DbServer.StaticGetPlaceholderIcon(s.display_name);
                s.has_custom_image = false;
            } else if (settings.icon_token != null)
            {
                var d = await Program.connection.GetUserContentByToken(settings.icon_token);
                if (d == null)
                    throw new StandardError("Couldn't get content token!", StandardErrorCode.InvalidInput);
                if (d.application_id != ICON_APPLICATION_ID)
                    throw new StandardError("Content application ID did not match.", StandardErrorCode.InvalidInput);
                if (d.uploader != u._id)
                    throw new StandardError("Content uploader ID did not match.", StandardErrorCode.InvalidInput);
                s.image_url = d.url;
                s.has_custom_image = true;
            }

            //Set user lock
            s.SetLockFlag(1, settings.is_user_locked);

            //Set permissions
            s.SetPermissionFlags(settings.permissions);

            //Set cluster ID, if it is set
            if (settings.cluster_id != null && await DbCluster.GetClusterById(Program.connection, MongoDB.Bson.ObjectId.Parse(settings.cluster_id)) != null)
                s.cluster_id = settings.cluster_id;

            //Save
            await s.UpdateAsync();

            //Write response
            await OnGETRequest(e, s, u);
        }

        class ManagementData_User
        {
            public string icon;
            public string name;
            public string steamId;
            public string id;
            public string steamUrl;

            public static async Task<ManagementData_User> GetUser(DbUser user)
            {
                //Lookup Steam info
                var steamInfo = await Program.connection.GetSteamProfileById(user.steam_id);
                if (steamInfo == null)
                    return null;

                return new ManagementData_User
                {
                    name = steamInfo.name,
                    icon = steamInfo.icon_url,
                    steamId = steamInfo.steam_id,
                    steamUrl = steamInfo.profile_url,
                    id = user.id
                };
            }
        }

        class ManagementDataResponse : ManagementData
        {
            public string icon; //Readonly
            public DbServerGameSettings settings; //Server settings, read only
            public DbCluster cluster;
            public string status; //Can be "ONLINE", "OFFLINE", or "ALERT"
            public string alert; //Must only be set if status == "ALERT"
            public List<WebArkMod> mods;
            public List<ManagementData_User> admins;
            public string map_name; //The last map name used
            public string map_id; //The last map id used
        }

        class ManagementDataRequest : ManagementData
        {
            public string icon_token; //Equal "%CLEAR_ICON" to clear. When sending, this is the icon for the server
        }

        class ManagementData
        {
            public string name;
            public bool is_user_locked;
            public bool[] permissions;
            public string cluster_id;
        }
    }
}
