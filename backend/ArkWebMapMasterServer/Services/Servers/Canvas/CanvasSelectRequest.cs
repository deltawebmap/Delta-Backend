using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static LibDeltaSystem.RPC.Payloads.Server.RPCPayload20003CanvasEvent;

namespace ArkWebMapMasterServer.Services.Servers.Canvas
{
    public class CanvasSelectRequest : CanvasRequestTemplate
    {
        public DbCanvas canvas;
        
        public CanvasSelectRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Get method
            var method = GetMethod();

            //Switch
            if (method == LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.GET)
                await WriteJSON(ConvertCanvas(canvas));
            else if (method == LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
                await OnPOST();
            else if (method == LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.DELETE)
                await OnDELETE();
            else if (method == LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.PUT)
                await OnPUT();
            else
                await WriteString("Unsupported Method", "text/plain", 400);
        }

        private async Task OnPOST()
        {
            //Check if this user has a tribe
            if (!await RequireTribe())
                return;

            //Rename the canvas; decode request
            CanvasCreateRequest request = await DecodePOSTBody<CanvasCreateRequest>();

            //Set
            await canvas.RenameCanvas(Program.connection, request.name, request.color);

            //Send RPC message
            SendRPCMessage(canvas, server, user, tribeId.Value, RPCPayload20003CanvasEvent_CanvasEventType.Modify);

            //Write response
            await WriteJSON(ConvertCanvas(canvas));
        }

        private async Task OnDELETE()
        {
            //Check if this user has a tribe
            if (!await RequireTribe())
                return;

            //Delete canvas
            await canvas.DeleteCanvas(Program.connection);

            //Send RPC message
            SendRPCMessage(canvas, server, user, tribeId.Value, RPCPayload20003CanvasEvent_CanvasEventType.Delete);

            //Write response
            await WriteJSON(ConvertCanvas(canvas));
        }

        private async Task OnPUT()
        {
            //Check if this user has a tribe
            if (!await RequireTribe())
                return;

            //We'll update the thumbnail; Decode the request
            UpdateThumbnailRequest request = await DecodePOSTBody<UpdateThumbnailRequest>();

            //Try to find
            DbUserContent uc = await Program.connection.GetUserContentByToken(request.token);
            if (uc == null)
                throw new StandardError("Token Not Valid", StandardErrorCode.InvalidInput);
            if (uc.application_id != CANVAS_THUMBNAIL_APPLICATION_ID)
                throw new StandardError("Specified User Content Application ID Mismatch", StandardErrorCode.InvalidInput);

            //Update
            await canvas.SetNewThumbnail(Program.connection, uc);

            //Send RPC message
            SendRPCMessage(canvas, server, user, tribeId.Value, RPCPayload20003CanvasEvent_CanvasEventType.Modify);

            //Write response
            await WriteJSON(ConvertCanvas(canvas));
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            //Run base
            if (!await base.SetArgs(args))
                return false;

            //Try to parse the canas ID
            if (!ObjectId.TryParse(args["CANVAS"], out ObjectId canvas_id))
            {
                await WriteString("Not a valid canvas ID", "text/plain", 400);
                return false;
            }

            //Get this server
            EndDebugCheckpoint("Get Canvas");
            canvas = await conn.LoadCanvasData(canvas_id);
            if (canvas == null)
            {
                await WriteString("Canvas not found", "text/plain", 404);
                return false;
            }

            //Check if this belongs to us
            if(tribeId.HasValue && tribeId.Value != canvas.tribe_id)
            {
                await WriteString("Canvas does not belong to you", "text/plain", 401);
                return false;
            }

            return true;
        }
    }
}
