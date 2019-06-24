using ArkWebMapLightspeedClient.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapLightspeedClient
{
    public class LightspeedRequest
    {
        public int token;
        public string method;
        public MasterServerArkUser auth;
        public string endpoint;

        public AWMLightspeedClient client;

        public void DoRespond(int status, string contentType, byte[] data)
        {
            //Encode, then send
            byte[] contentTypeBytes = Encoding.UTF8.GetBytes(contentType);
            byte[] content = new byte[4 + 4 + 4 + 4 + data.Length + contentTypeBytes.Length];
            BinaryIntEncoder.Int32ToBytes(token).CopyTo(content, 0);
            BinaryIntEncoder.Int32ToBytes(status).CopyTo(content, 4);
            BinaryIntEncoder.Int32ToBytes(data.Length).CopyTo(content, 8);
            BinaryIntEncoder.Int32ToBytes(contentTypeBytes.Length).CopyTo(content, 12);
            contentTypeBytes.CopyTo(content, 16);
            data.CopyTo(content, 16 + contentTypeBytes.Length);

            //Send
            client.SendMessage(content);
        }
    }
}
