using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers.Admin
{
    public class AdminServerSetIconRequest : ArkServerAdminDeltaService
    {
        public AdminServerSetIconRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnAuthenticatedRequest()
        {
            //Upload image
            string url = await LibDeltaSystem.Tools.UserContentTool.UploadUserContentResizeImage(e.Request.Body, 128, 128);

            //Update
            await server.ExplicitUpdateAsync(conn, Builders<DbServer>.Update.Set("image_url", url).Set("has_custom_image", true));
            server.image_url = url;
            server.has_custom_image = true;

            //Notify via RPC
            await server.NotifyPublicDetailsChanged(conn);

            //Return server
            await WriteJSON(await NetGuildUser.GetNetGuild(conn, server, user));
        }
    }
}
