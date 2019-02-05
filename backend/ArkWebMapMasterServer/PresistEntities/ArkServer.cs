using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class ArkServer
    {
        /// <summary>
        /// Name shown in the UI
        /// </summary>
        public string display_name { get; set; }

        /// <summary>
        /// URL to a server icon.
        /// </summary>
        public string image_url { get; set; }

        /// <summary>
        /// ID of the owner of the server
        /// </summary>
        public string owner_uid { get; set; }

        /// <summary>
        /// Creds checked to verify the connection between the slave server.
        /// </summary>
        public byte[] server_creds { get; set; }

        /// <summary>
        /// ID of the server
        /// </summary>
        [JsonProperty("id")]
        public string _id { get; set; }

        /// <summary>
        /// The location to connect to.
        /// </summary>
        public string latest_proxy_url { get; set; }

        public void Update()
        {
            ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetCollection().Update(this);
        }

        public T SendRequest<T>(string action, object data, RequestHttpMethod method, ArkUser user)
        {
            HttpResponseMessage response = OpenHttpRequest(new StringContent(JsonConvert.SerializeObject(data)), action, method.ToString(), user);
            var replyString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                //Parse error and throw it
                throw JsonConvert.DeserializeObject<StandardError>(replyString);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Continue normally
                return JsonConvert.DeserializeObject<T>(replyString);
            }
            else
            {
                //Unknown
                throw new Exception("Server reply was not valid.");
            }
        }

        public HttpResponseMessage OpenHttpRequest(HttpContent content, string action, string method, ArkUser user)
        {
            //Add headers and send.
            content.Headers.Add("X-Ark-User-Auth", JsonConvert.SerializeObject(user));

            //Todo: Create validation headers
            string fullURL = $"http://{latest_proxy_url}{action}";
            HttpResponseMessage reply;
            using (HttpClient client = new HttpClient())
            {
                reply = client.SendAsync(new HttpRequestMessage
                {
                    Content = content, 
                    Method = new HttpMethod(method),
                    RequestUri = new Uri(fullURL),
                }).GetAwaiter().GetResult();
            }
            return reply;
        }
    }
}
