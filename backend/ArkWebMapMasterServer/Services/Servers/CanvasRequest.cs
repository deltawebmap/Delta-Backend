using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.RPC.Payloads;
using LibDeltaSystem.RPC.Payloads.Server;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static LibDeltaSystem.RPC.Payloads.Server.RPCPayload20003CanvasEvent;

namespace ArkWebMapMasterServer.Services.Servers
{
    public static class CanvasRequest
    {
        public const string CANVAS_THUMBNAIL_APPLICATION_ID = "Bmazv5PRjg6loBWn";

        public static async Task OnListRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s, DbUser u, int tribe_id)
        {
            //Get request method
            var method = Program.FindRequestMethod(e);

            //If this is a GET, list
            if(method == RequestHttpMethod.get)
            {
                //Find all canvases beloning to this server
                List<DbCanvas> canvases = await s.GetServerCanvases(Program.connection);

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
                return;
            } else if (method == RequestHttpMethod.post)
            {
                //Check scope
                await Program.CheckTokenScope(u, null);
                
                //We'll create a canvas. Decode the body
                CanvasCreateRequest request = Program.DecodePostBody<CanvasCreateRequest>(e);

                //Verify
                if(request.color == null || request.name == null)
                {
                    await Program.QuickWriteToDoc(e, "Missing required data.", "text/plain", 400);
                    return;
                }

                //Create
                DbCanvas c = new DbCanvas
                {
                    color = request.color,
                    last_edited = DateTime.UtcNow,
                    last_editor = u._id,
                    last_saved = DateTime.UtcNow,
                    name = request.name,
                    server_id = s.id,
                    users = new List<ObjectId>(),
                    version = 1,
                    _id = ObjectId.GenerateNewId()
                };

                //Add
                await Program.connection.system_canvases.InsertOneAsync(c);

                //Send RPC message
                SendRPCMessage(c, s, u, tribe_id, RPCPayload20003CanvasEvent_CanvasEventType.Create);

                //Write response
                await Program.QuickWriteJsonToDoc(e, ConvertCanvas(c));
            } else
            {
                throw new StandardError("Unexpected method.", StandardErrorCode.BadMethod);
            }
        }

        public static async Task OnCanvasRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s, DbUser u, int tribe_id, string next)
        {
            //Find canvas
            if(!ObjectId.TryParse(next, out ObjectId id))
            {
                throw new StandardError("Canvas Not Found", StandardErrorCode.NotFound);
            }
            DbCanvas c = await Program.connection.LoadCanvasData(id);
            if (c == null)
            {
                throw new StandardError("Canvas Not Found", StandardErrorCode.NotFound);
            }

            //Get request method
            var method = Program.FindRequestMethod(e);

            //Act from the method
            if (method == RequestHttpMethod.get)
            {
                //Write response
                await Program.QuickWriteJsonToDoc(e, ConvertCanvas(c));
            }
            else if (method == RequestHttpMethod.post)
            {
                //Check scope
                await Program.CheckTokenScope(u, null);

                //Rename the canvas; decode request
                CanvasCreateRequest request = Program.DecodePostBody<CanvasCreateRequest>(e);

                //Set
                await c.RenameCanvas(Program.connection, request.name, request.color);

                //Send RPC message
                SendRPCMessage(c, s, u, tribe_id, RPCPayload20003CanvasEvent_CanvasEventType.Modify);

                //Write response
                await Program.QuickWriteJsonToDoc(e, ConvertCanvas(c));
            }
            else if (method == RequestHttpMethod.delete)
            {
                //Check scope
                await Program.CheckTokenScope(u, null);

                //Delete canvas
                await c.DeleteCanvas(Program.connection);

                //Send RPC message
                SendRPCMessage(c, s, u, tribe_id, RPCPayload20003CanvasEvent_CanvasEventType.Delete);

                //Write response
                await Program.QuickWriteJsonToDoc(e, ConvertCanvas(c));
            }
            else if (method == RequestHttpMethod.put)
            {
                //Check scope
                await Program.CheckTokenScope(u, null);

                //We'll update the thumbnail; Decode the request
                UpdateThumbnailRequest request = Program.DecodePostBody<UpdateThumbnailRequest>(e);

                //Try to find
                DbUserContent uc = await Program.connection.GetUserContentByToken(request.token);
                if (uc == null)
                    throw new StandardError("Token Not Valid", StandardErrorCode.InvalidInput);
                if (uc.application_id != CANVAS_THUMBNAIL_APPLICATION_ID)
                    throw new StandardError("Specified User Content Application ID Mismatch", StandardErrorCode.InvalidInput);

                //Update
                await c.SetNewThumbnail(Program.connection, uc);

                //Send RPC message
                SendRPCMessage(c, s, u, tribe_id, RPCPayload20003CanvasEvent_CanvasEventType.Modify);

                //Write response
                await Program.QuickWriteJsonToDoc(e, ConvertCanvas(c));
            }
            else
            {
                throw new StandardError("Unexpected method.", StandardErrorCode.BadMethod);
            }
        }

        private static void SendRPCMessage(DbCanvas c, DbServer server, DbUser u, int tribe_id, RPCPayload20003CanvasEvent_CanvasEventType change)
        {
            //Create message
            RPCPayload20003CanvasEvent msg = new RPCPayload20003CanvasEvent
            {
                action = change,
                data = ConvertCanvas(c),
                user = NetMiniUser.ConvertUser(u)
            };

            //Send RPC message
            Program.connection.GetRPC().SendRPCMsgToServerTribe(LibDeltaSystem.RPC.RPCOpcode.RPCServer20002CanvasEvent, msg, server, tribe_id);
        }

        private static NetCanvas ConvertCanvas(DbCanvas c)
        {
            return new NetCanvas
            {
                color = c.color,
                href = Program.connection.config.hosts.master + "/api" + "/servers/" + c.server_id + "/canvas/" + c.id,
                id = c.id,
                name = c.name,
                thumbnail = c.thumbnail_url
            };
        }

        class CanvasListResponse
        {
            public NetCanvas[] canvases;
            public string ws_url;
            public int ws_reconnect_policy;
        }

        class CanvasCreateRequest
        {
            public string name;
            public string color;
        }

        class UpdateThumbnailRequest
        {
            public string token;
        }
    }
}
