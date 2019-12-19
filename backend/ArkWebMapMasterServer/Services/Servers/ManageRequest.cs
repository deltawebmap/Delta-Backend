using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
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
            ManagementData m = new ManagementData
            {
                name = s.display_name,
                icon = s.image_url,
                is_user_locked = s.CheckLockFlag(1),
                permissions = s.GetPermissionFlagList(),
                settings = s.game_settings,
                cluster_id = s.cluster_id
            };
            await Program.QuickWriteJsonToDoc(e, m);
        }

        public static async Task OnPOSTRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s, DbUser u)
        {
            //Decode settings
            var settings = Program.DecodePostBody<ManagementData>(e);

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

        class ManagementData
        {
            public string name;
            public string icon; //Readonly
            public string icon_token; //Equal "%CLEAR_ICON" to clear. When sending, this is the icon for the server
            public bool is_user_locked;
            public bool[] permissions;
            public DbServerGameSettings settings; //Server settings, read only
            public string cluster_id;
        }
    }
}
