using ArkBridgeSharedEntities.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ArkHttpServer
{
    public static class MasterServerSender
    {
        public static T SendRequestToMaster<T>(string action, object request)
        {
            byte[] data = SendRequestToMasterGetBytes(action, new StringContent(JsonConvert.SerializeObject(request)));
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        }

        public static object SendRequestToMaster(string action, object request, Type type)
        {
            byte[] data = SendRequestToMasterGetBytes(action, new StringContent(JsonConvert.SerializeObject(request)));
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), type);
        }

        public static byte[] SendRequestToMasterGetBytes(string action, HttpContent content)
        {
            //Get full URL
            string fullURL = ArkWebServer.remote_config.sub_server_config.endpoints.base_bridge_url + action;

            //Send the HTTP request with our special headers. We'll worry about authentication later.
            byte[] reply;
            using (HttpClient client = new HttpClient())
            {
                //Set our id
                content.Headers.Add("X-Ark-Slave-Server-ID", ArkWebServer.config.connection.id);
                content.Headers.Add("X-Ark-Slave-Server-Creds", ArkWebServer.config.connection.creds);

                var response = client.PostAsync(fullURL, content).GetAwaiter().GetResult();
                var replyString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Continue normally
                    reply = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                }
                else
                {
                    //Unknown
                    Console.WriteLine("Failed to send a system message to the master server. Error code -5.");
                    throw new Exception();
                }
            }
            return reply;
        }
    }
}
