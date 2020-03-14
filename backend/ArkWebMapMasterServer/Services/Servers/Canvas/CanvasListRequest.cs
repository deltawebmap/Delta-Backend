using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.CommonNet;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static LibDeltaSystem.RPC.Payloads.Server.RPCPayload20003CanvasEvent;

namespace ArkWebMapMasterServer.Services.Servers.Canvas
{
    public class CanvasListRequest : CanvasRequestTemplate
    {
        public CanvasListRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Get method
            var method = GetMethod();

            //Switch
            if (method == LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.GET)
                await OnGET();
            else if (method == LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
                await OnPOST();
            else
                await WriteString("Unsupported Method", "text/plain", 400);
        }

        private async Task OnGET()
        {
            //Find all canvases beloning to this server
            List<DbCanvas> canvases = await server.GetServerCanvases(Program.connection, tribeId);

            //Now, convert all
            CanvasListResponse response = new CanvasListResponse
            {
                canvases = new NetCanvas[canvases.Count],
                ws_reconnect_policy = 10000,
                ws_url = Program.config.endpoint_canvas
            };
            for (int i = 0; i < canvases.Count; i++)
                response.canvases[i] = ConvertCanvas(canvases[i]);

            //Write response
            await Program.QuickWriteJsonToDoc(e, response);
        }

        private async Task OnPOST()
        {
            //Require we have a tribe
            if (!await RequireTribe())
                return;
            
            //We'll create a canvas. Decode the body
            CanvasCreateRequest request = Program.DecodePostBody<CanvasCreateRequest>(e);

            //Verify
            if (request.color == null || request.name == null)
            {
                await Program.QuickWriteToDoc(e, "Missing required data.", "text/plain", 400);
                return;
            }

            //Create
            DbCanvas c = new DbCanvas
            {
                color = request.color,
                last_edited = DateTime.UtcNow,
                last_editor = user._id,
                last_saved = DateTime.UtcNow,
                name = request.name,
                server_id = server._id,
                users = new List<ObjectId>(),
                version = 1,
                tribe_id = tribeId.Value,
                _id = ObjectId.GenerateNewId()
            };

            //Add
            await Program.connection.system_canvases.InsertOneAsync(c);

            //Send RPC message
            SendRPCMessage(c, server, user, tribeId.Value, RPCPayload20003CanvasEvent_CanvasEventType.Create);

            //Write response
            await Program.QuickWriteJsonToDoc(e, ConvertCanvas(c));
        }
    }
}
