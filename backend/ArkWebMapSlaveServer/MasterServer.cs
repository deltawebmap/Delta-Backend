using ArkBridgeSharedEntities;
using ArkBridgeSharedEntities.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ArkWebMapSlaveServer
{
    public static class MasterServer
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
            string fullURL = ArkWebMapServer.remote_config.sub_server_config.endpoints.base_bridge_url + action;

            //Send the HTTP request with our special headers. We'll worry about authentication later.
            byte[] reply;
            using (HttpClient client = new HttpClient())
            {
                //Set our id
                content.Headers.Add("X-Ark-Slave-Server-ID", ArkWebMapServer.config.auth.id);

                //Generate an HMAC
                byte[] salt = HMACGen.GenerateSalt();
                string generatedHmac = HMACGen.GenerateHMAC(salt, ArkWebMapServer.creds);
                content.Headers.Add("X-Ark-Integrity", generatedHmac);
                content.Headers.Add("X-Ark-Salt", Convert.ToBase64String(salt));

                var response = client.PostAsync(fullURL, content).GetAwaiter().GetResult();
                var replyString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    //Parse error and throw it
                    var error = JsonConvert.DeserializeObject<StandardError>(replyString);
                    throw new StandardError($"Failed to send message to backend master server with code {((StandardErrorCode)error.error_code).ToString()} ({error.error_code}). Stack: {error.StackTrace}", StandardErrorCode.BridgeBackendServerError, error);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Continue normally
                    reply = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                }
                else
                {
                    //Unknown
                    throw new StandardError("Server reply was not valid.", StandardErrorCode.BridgeBackendServerNetFailed);
                }
            }
            return reply;
        }
    }
}
