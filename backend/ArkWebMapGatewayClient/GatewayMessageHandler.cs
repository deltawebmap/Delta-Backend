using ArkWebMapGatewayClient.Messages;
using ArkWebMapGatewayClient.Messages.SubserverClient;
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
                case GatewayMessageOpcode.OnDrawableMapChange: HandleMsgType<MessageOnDrawableMapChange>(Msg_OnDrawableMapChange, msg, context); break;
                case GatewayMessageOpcode.SetSessionId: HandleMsgType<MessageSetSessionID>(Msg_SetSessionId, msg, context); break;
                case GatewayMessageOpcode.OnServerListUpdate: HandleMsgType<MessageOnServerListUpdate>(Msg_OnServerListUpdate, msg, context); break;
                case GatewayMessageOpcode.MessageDirListing: HandleMsgType<MessageDirListing>(Msg_MessageDirListing, msg, context); break;
                case GatewayMessageOpcode.OnMachineUpdateServerList: HandleMsgType<MessageMachineUpdateServerList>(Msg_OnMachineUpdateServerList, msg, context); break;
                case GatewayMessageOpcode.OnMirrorDinoUpdate: HandleMsgType<MessageOnMirrorDinoUpdate>(Msg_OnMirrorDinoUpdate, msg, context); break;
            }
        }

        private void HandleMsgType<T>(GatewayIncomingMessageCallback<T> code, string msg, object context)
        {
            T o = JsonConvert.DeserializeObject<T>(msg);
            code(o, context);
        }

        public virtual void Msg_OnDrawableMapChange(MessageOnDrawableMapChange data, object context) { }
        public virtual void Msg_SetSessionId(MessageSetSessionID data, object context) { }
        public virtual void Msg_OnServerListUpdate(MessageOnServerListUpdate data, object context) { }
        public virtual void Msg_MessageDirListing(MessageDirListing data, object context) { }
        public virtual void Msg_OnMachineUpdateServerList(MessageMachineUpdateServerList data, object context) { }
        public virtual void Msg_OnMirrorDinoUpdate(MessageOnMirrorDinoUpdate data, object context) { }
    }

    public delegate void GatewayIncomingMessageCallback<T>(T data, object context);
}
