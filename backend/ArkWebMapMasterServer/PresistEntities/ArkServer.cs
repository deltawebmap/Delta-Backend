using ArkBridgeSharedEntities;
using ArkBridgeSharedEntities.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// The latest local accounts the server has. These are Ark accounts, not our accounts.
        /// </summary>
        public List<ArkSlaveReport_PlayerAccount> latest_server_local_accounts { get; set; }

        /// <summary>
        /// The latest map the server was on.
        /// </summary>
        public string latest_server_map { get; set; }

        /// <summary>
        /// Latest time of the Ark server
        /// </summary>
        public float latest_server_time { get; set; }

        /// <summary>
        /// The time the last server report was downloaded.
        /// </summary>
        public long latest_server_report_downloaded { get; set; }

        /// <summary>
        /// If we have the above four values
        /// </summary>
        public bool has_server_report { get; set; }

        /// <summary>
        /// If the server was deleted, this is set to true.
        /// </summary>
        public bool is_deleted { get; set; }

        public void Update()
        {
            ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetCollection().Update(this);
        }

        public string GetPlaceholderIcon()
        {
            return StaticGetPlaceholderIcon(display_name);
        }

        public static string StaticGetPlaceholderIcon(string display_name)
        {
            //Find letters
            string[] words = display_name.Split(' ');
            char[] charset = "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
            string output = "";
            for(int i = 0; i<words.Length; i++)
            {
                if (output.Length >= 2)
                    break;
                if(words[i].Length > 1)
                {
                    char c = words[i][0];
                    if(charset.Contains(c))
                    {
                        string sc = new string(new char[] { c });
                        if (output.Length == 0)
                            sc = sc.ToUpper();
                        else
                            sc = sc.ToLower();
                        output += sc;
                    }
                }
            }

            //Now, return URL
            return "https://ark.romanport.com/resources/placeholder_server_images/" + output + ".png";
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
            //Generate salt and create URLs
            string fullURL = $"http://{latest_proxy_url}{action}";
            Uri fullURI = new Uri(fullURL);
            byte[] salt = HMACGen.GenerateSalt();

            //Generate HMAC
            string hmac = HMACGen.GenerateHMAC(salt, server_creds);

            //Add headers and send.
            content.Headers.Add("X-Ark-User-Auth", JsonConvert.SerializeObject(user));
            content.Headers.Add("X-Ark-Salt", Convert.ToBase64String(salt));
            content.Headers.Add("X-Ark-Integrity", hmac);
            content.Headers.Add("X-Ark-Source-IP", "0.0.0.0");

            
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
