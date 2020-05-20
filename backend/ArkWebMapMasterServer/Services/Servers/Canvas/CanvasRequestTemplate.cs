using ArkWebMapMasterServer.ServiceTemplates;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.RPC.Payloads.Server;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using static LibDeltaSystem.RPC.Payloads.Server.RPCPayload20003CanvasEvent;

namespace ArkWebMapMasterServer.Services.Servers.Canvas
{
    public abstract class CanvasRequestTemplate : MasterTribeServiceTemplate
    {
        public const string CANVAS_THUMBNAIL_APPLICATION_ID = "Bmazv5PRjg6loBWn";

        public CanvasRequestTemplate(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        internal static void SendRPCMessage(DbCanvas c, DbServer server, DbUser u, int tribe_id, RPCPayload20003CanvasEvent_CanvasEventType change)
        {
            //Create message
            RPCPayload20003CanvasEvent msg = new RPCPayload20003CanvasEvent
            {
                action = change,
                data = ConvertCanvas(c),
                user = NetMiniUser.ConvertUser(u)
            };

            //Send RPC message
            Program.connection.GetRPC().SendRPCMsgToServerTribe(LibDeltaSystem.RPC.RPCOpcode.RPCServer20003CanvasEvent, msg, server, tribe_id);
        }

        internal static NetCanvas ConvertCanvas(DbCanvas c)
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

        internal class CanvasListResponse
        {
            public NetCanvas[] canvases;
            public string ws_url;
            public int ws_reconnect_policy;
        }

        internal class CanvasCreateRequest
        {
            public string name;
            public string color;
        }

        internal class UpdateThumbnailRequest
        {
            public string token;
        }
    }
}
