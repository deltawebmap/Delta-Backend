using ArkWebMapMasterServer.ServiceTemplates;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class ManageRequest : MasterTribeServiceTemplate
    {
        public const string ICON_APPLICATION_ID = "t3VXa599Na64w7vd";

        public ManageRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Make sure we are admin
            if (!await RequireServerAdmin())
                return;

            //Get method
            var method = Program.FindRequestMethod(e);
            if (method == RequestHttpMethod.get)
                await OnGETRequest();
            else if (method == RequestHttpMethod.post)
                await OnPOSTRequest();
            else
                throw new StandardError("This method was not expected.", StandardErrorCode.BadMethod);
        }

        public async Task OnGETRequest()
        {
            //Get cluster, if any
            DbCluster cluster = null;
            if (server.cluster_id != null)
                cluster = await DbCluster.GetClusterById(Program.connection, MongoDB.Bson.ObjectId.Parse(server.cluster_id));

            //Get map entry
            string mapName = null;
            var mapEntry = await Program.connection.GetARKMapByInternalName(server.latest_server_map);
            if (mapEntry != null)
                mapName = mapEntry.displayName;

            //Get server status
            string status = "ONLINE";
            string message = "Online";
            if (server.CheckLockFlag(1))
            {
                status = "ALERT";
                message = "Locked";
            }
            if(server.CheckLockFlag(0))
            {
                status = "ALERT";
                message = "Still Initializing...";
            }
            if (server.CheckLockFlag(2))
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
            DateTime lastConnectTime = new DateTime(server.last_client_connect_time);
            if ((DateTime.UtcNow - lastConnectTime).TotalMinutes > 2.5f)
            {
                status = "OFFLINE";
                message = $"Offline Since {lastConnectTime.ToShortDateString()}";
            }

            //Look up all mods used and convert
            Dictionary<string, DbSteamModCache> mods = await server.GetAllServerMods(Program.connection, true);
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
            admins.Add(await ManagementData_User.GetUser(user));
            foreach (var id in server.admins)
                admins.Add(await ManagementData_User.GetUser( await DbUser.GetUserByID( Program.connection, id )));

            ManagementDataResponse m = new ManagementDataResponse
            {
                name = server.display_name,
                icon = server.image_url,
                is_user_locked = server.CheckLockFlag(1),
                permissions = server.GetPermissionFlagList(),
                settings = server.game_settings,
                cluster_id = server.cluster_id,
                cluster = cluster,
                status = status,
                alert = message,
                mods = modList,
                admins = admins,
                map_id = server.latest_server_map,
                map_name = mapName
            };
            await Program.QuickWriteJsonToDoc(e, m);
        }

        public async Task OnPOSTRequest()
        {
            //Decode settings
            var settings = Program.DecodePostBody<ManagementDataRequest>(e);

            //Set name
            if (settings.name != null)
                server.display_name = settings.name;

            //Set icon
            if (settings.icon_token == "%CLEAR_ICON") {
                server.image_url = DbServer.StaticGetPlaceholderIcon(Program.connection, server.display_name);
                server.has_custom_image = false;
            } else if (settings.icon_token != null)
            {
                var d = await Program.connection.GetUserContentByToken(settings.icon_token);
                if (d == null)
                    throw new StandardError("Couldn't get content token!", StandardErrorCode.InvalidInput);
                if (d.application_id != ICON_APPLICATION_ID)
                    throw new StandardError("Content application ID did not match.", StandardErrorCode.InvalidInput);
                if (d.uploader != user._id)
                    throw new StandardError("Content uploader ID did not match.", StandardErrorCode.InvalidInput);
                server.image_url = d.url;
                server.has_custom_image = true;
            }

            //Set user lock
            server.SetLockFlag(1, settings.is_user_locked);

            //Set permissions
            server.SetPermissionFlags(settings.permissions);

            //Set cluster ID, if it is set
            if (settings.cluster_id != null && await DbCluster.GetClusterById(Program.connection, MongoDB.Bson.ObjectId.Parse(settings.cluster_id)) != null)
                server.cluster_id = settings.cluster_id;

            //Save
            var updateBuilder = Builders<DbServer>.Update;
            var update = updateBuilder.Set("lock_flags", server.lock_flags)
                .Set("permission_flags", server.permission_flags)
                .Set("cluster_id", server.cluster_id)
                .Set("has_custom_image", server.has_custom_image)
                .Set("image_url", server.image_url)
                .Set("display_name", server.display_name);
            await server.UpdateAsync(Program.connection, update);

            //Write response
            await OnGETRequest();
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
