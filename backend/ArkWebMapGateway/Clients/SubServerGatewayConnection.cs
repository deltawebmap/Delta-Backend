using ArkWebMapGateway.ClientHandlers;
using ArkWebMapGatewayClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapGateway.Clients
{
    public class SubServerGatewayConnection : GatewayConnection
    {
        public SubServerGatewayHandler handler;

        public string serverId;
        public string serverName;
        public string serverIcon;

        public static async Task<SubServerGatewayConnection> HandleIncomingConnection(Microsoft.AspNetCore.Http.HttpContext e, string version)
        {
            //Get data
            string[] serverQueryData = e.Request.Query["auth_token"].ToString().Split('#');
            string serverId = serverQueryData[0];
            string serverCreds = serverQueryData[1];

            //Authenticate
            ServerValidationResponsePayload details;
            using (HttpClient wc = new HttpClient())
            {
                string request = JsonConvert.SerializeObject(new ServerValidationRequestPayload
                {
                    server_creds = serverCreds,
                    server_id = serverId
                });
                var response = await wc.PostAsync("https://deltamap.net/api/server_validation", new StringContent(request));
                if (!response.IsSuccessStatusCode)
                {
                    details = null;
                } else
                {
                    details = JsonConvert.DeserializeObject<ServerValidationResponsePayload>(await response.Content.ReadAsStringAsync());
                }
            }

            //If we failed, stop
            if(details == null)
            {
                await Program.QuickWriteToDoc(e, "Not Authenticated.", "text/plain", 401);
                return null;
            }

            //Start
            SubServerGatewayConnection conn = new SubServerGatewayConnection();
            conn.serverIcon = details.icon_url;
            conn.serverName = details.server_name;
            conn.serverId = details.server_id;

            //Set handler
            conn.handler = new SubServerGatewayHandler(conn);
            conn.OnSetHandler(conn.handler);
            await conn.Run(e, () =>
            {
                //Ready
            });

            return conn;
        }

        public override Task<bool> OnMsg(string msg)
        {
            //Deserialize as base type to get the opcode
            GatewayMessageBase b = JsonConvert.DeserializeObject<GatewayMessageBase>(msg);

            //Now, let it be handled like normal.
            //handler.HandleMsg(b.opcode, msg, this); //TODO

            //Return OK
            return Task.FromResult<bool>(true);
        }

        class ServerValidationRequestPayload
        {
            public string server_id;
            public string server_creds; //Base 64 encoded creds for the server
        }

        class ServerValidationResponsePayload
        {
            public string server_id;
            public string server_name;
            public string server_owner_id;
            public bool has_icon;
            public string icon_url;
        }
    }
}
