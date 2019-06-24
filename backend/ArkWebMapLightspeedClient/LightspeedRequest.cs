using ArkWebMapLightspeedClient.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapLightspeedClient
{
    public class LightspeedRequest
    {
        public int token;
        public string method;
        public MasterServerArkUser auth;
        public string endpoint;
        public Dictionary<string, string> query;
        public byte[] body;

        public AWMLightspeedClient client;

        public async Task DoRespondJson<T>(T data, int status = 200, string contentType = "application/json")
        {
            string content = JsonConvert.SerializeObject(data);
            await DoRespondString(content, contentType, status);
        }

        public async Task DoRespondString(string strData, string contentType, int status)
        {
            byte[] data = Encoding.UTF8.GetBytes(strData);
            await DoRespond(status, contentType, data);
        }

        public async Task DoRespond(int status, string contentType, byte[] data)
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
