using LibDeltaSystem;
using LibDeltaSystem.Tools;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.ServiceTemplates
{
    public abstract class MasterArkRpcService : ArkServerDeltaService
    {
        public MasterArkRpcService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        /// <summary>
        /// Builds the RPC event data and returns if it's OK to send or not. Expected to create your own error if it fails
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="persist"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public abstract Task<RpcCommand?> BuildArkRpcEvent();

        public override async Task OnRequest()
        {
            //Get the parameters
            var p = await BuildArkRpcEvent();
            if(p == null)
            {
                return;
            }

            //Send the ArkRPC event
            //This does create a race condition, but the time it takes ARK to process this will be much longer than it takes for the client to download this ID
            ObjectId rpcId = await SyncCommandTool._SendCommand(conn, p.Value.opcode, p.Value.persist, server._id, user._id, p.Value.payload);

            //Create response
            await WriteJSON(new ResponseData
            {
                rpc_id = rpcId
            });
        }

        public struct RpcCommand
        {
            public int opcode;
            public bool persist;
            public object payload;
        }

        class ResponseData
        {
            public ObjectId rpc_id;
        }
    }
}
