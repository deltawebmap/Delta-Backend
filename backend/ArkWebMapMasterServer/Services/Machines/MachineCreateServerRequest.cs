using ArkBridgeSharedEntities.Entities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LibDeltaSystem.Tools;

namespace ArkWebMapMasterServer.Services.Machines
{
    public static class MachineCreateServerRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbMachine machine)
        {
            //Deocde payload
            var payload = Program.DecodePostBody<MachineCreateServerPayload>(e);

            //Make sure all required elements are here
            if (payload == null)
                throw new StandardError("No payload provided.", StandardErrorCode.MissingRequiredArg);
            if(payload.load_settings == null || payload.name == null)
                throw new StandardError("Missing required payload data.", StandardErrorCode.MissingRequiredArg);

            //Make sure our name is a correct length
            if(payload.name.Length > 24 || payload.name.Length < 2)
                throw new StandardError("name is too long or too short.", StandardErrorCode.InvalidInput);

            //Get the icon to use.
            string icon = DbServer.StaticGetPlaceholderIcon(payload.name);
            bool isIconCustom = false;
            if(payload.icon_token != null)
            {
                var r = UserContentUploader.FinishContentUpload(payload.icon_token);
                if (r == null)
                    throw new StandardError("icon_token is not valid.", StandardErrorCode.InvalidInput);
                icon = r.url;
                isIconCustom = true;
            }

            //Generate a token to use
            string token = SecureStringTool.GenerateSecureString(82);
            while (!await SecureStringTool.CheckStringUniquenessAsync<DbServer>(token, Program.connection.system_servers))
                token = SecureStringTool.GenerateSecureString(82);

            //Now, we'll create a server.
            DbServer server = new DbServer
            {
                display_name = payload.name,
                _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                image_url = icon,
                owner_uid = machine.owner_id,
                token = token,
                is_managed = false,
                is_published = false,
                has_custom_image = isIconCustom,
                revision_id = 0,
                conn = Program.connection,
                load_settings = payload.load_settings,
                machine_uid = machine.id
            };

            //Insert
            Program.connection.system_servers.InsertOne(server);

            //Notify the machine to update
            Program.gateway.SendMessageToSubserverWithId(new ArkWebMapGatewayClient.Messages.SubserverClient.MessageMachineUpdateServerList
            {
                opcode = ArkWebMapGatewayClient.GatewayMessageOpcode.OnMachineUpdateServerList,
                headers = new Dictionary<string, string>()
            }, machine.id);

            //Write server info
            MachineCreateServerResponse response = new MachineCreateServerResponse
            {
                id = server.id,
                token = server.token,
                name = server.display_name,
                icon_url = server.image_url
            };
            await Program.QuickWriteJsonToDoc(e, response);
        }

        /// <summary>
        /// Data sent to this.
        /// </summary>
        class MachineCreateServerPayload
        {
            /// <summary>
            /// The settings used to load the server map. Not null.
            /// </summary>
            public DbServer_LoadSettings load_settings;

            /// <summary>
            /// The name of the server. Not null.
            /// </summary>
            public string name;

            /// <summary>
            /// The token for the icon. Optional.
            /// </summary>
            public string icon_token;
        }

        class MachineCreateServerResponse
        {
            /// <summary>
            /// ID of this server
            /// </summary>
            public string id;

            /// <summary>
            /// Server token
            /// </summary>
            public string token;

            /// <summary>
            /// Name
            /// </summary>
            public string name;

            /// <summary>
            /// Icon URL
            /// </summary>
            public string icon_url;
        }
    }
}
