using ArkWebMapGatewayClient.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient
{
    /// <summary>
    /// Override this in a class to use each type. https://docs.google.com/spreadsheets/d/1XvR03ie2ao5SkeaVDJlV5KY9Dv46XRBBQb1VhiAU-b8/edit?usp=sharing
    /// </summary>
    public class GatewayMessageHandler
    {
        public void HandleMsg(GatewayMessageOpcode opcode, string msg, object context)
        {
            switch(opcode)
            {
                case GatewayMessageOpcode.PingRequest: HandleMsgType<MessagePing>(Msg_PingRequest, msg, context); break;
                case GatewayMessageOpcode.PingResponse: HandleMsgType<MessagePing>(Msg_PingResponse, msg, context); break;
                case GatewayMessageOpcode.TribeMapInput: HandleMsgType<MessageMapDrawingInput>(Msg_TribeMapInput, msg, context); break;
                case GatewayMessageOpcode.TribeMapFrontendOutput: HandleMsgType<MessageMapDrawingOutput>(Msg_TribeMapFrontendOutput, msg, context); break;
                case GatewayMessageOpcode.SetSessionId: HandleMsgType<MessageSetSessionID>(Msg_SetSessionId, msg, context); break;
            }
        }

        private void HandleMsgType<T>(GatewayIncomingMessageCallback<T> code, string msg, object context)
        {
            T o = JsonConvert.DeserializeObject<T>(msg);
            code(o, context);
        }

        public virtual void Msg_PingRequest(MessagePing data, object context) { }
        public virtual void Msg_PingResponse(MessagePing data, object context) { }
        public virtual void Msg_TribeMapInput(MessageMapDrawingInput data, object context) { }
        public virtual void Msg_TribeMapFrontendOutput(MessageMapDrawingOutput data, object context) { }
        public virtual void Msg_SetSessionId(MessageSetSessionID data, object context) { }
    }

    public delegate void GatewayIncomingMessageCallback<T>(T data, object context);
}
