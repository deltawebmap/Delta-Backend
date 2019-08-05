using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LibDelta
{
    public static class WebTools
    {
        public static async Task<Stream> SendGet(string url, bool includeKey = true)
        {
            using (HttpClient hc = new HttpClient())
            {
                //Send
                if (includeKey)
                    hc.DefaultRequestHeaders.Add("X-Delta-System-Key", DeltaMapTools.ACCESS_KEY);
                hc.DefaultRequestHeaders.Add("X-Delta-System-Name", DeltaMapTools.USER_AGENT);
                var response = await hc.GetAsync(url);

                //Check
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Unexpected status code.");
                return await response.Content.ReadAsStreamAsync();
            }
        }

        public static async Task<T> SendGetJson<T>(string url, bool includeKey = true)
        {
            //Get
            Stream s = await SendGet(url, includeKey);
            return ReadStringAsJson<T>(ReadBytesAsString(ReadStreamAsBytes(s)));
        }

        public static async Task<Stream> SendUserGet(string url, string token, bool includeKey = true)
        {
            using (HttpClient hc = new HttpClient())
            {
                //Send
                if (includeKey)
                    hc.DefaultRequestHeaders.Add("X-Delta-System-Key", DeltaMapTools.ACCESS_KEY);
                hc.DefaultRequestHeaders.Add("X-Delta-System-Name", DeltaMapTools.USER_AGENT);
                hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await hc.GetAsync(url);

                //Check
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Unexpected status code.");
                return await response.Content.ReadAsStreamAsync();
            }
        }

        public static async Task<T> SendUserGetJson<T>(string url, string token, bool includeKey = true)
        {
            //Get
            Stream s = await SendUserGet(url, token, includeKey);
            return ReadStringAsJson<T>(ReadBytesAsString(ReadStreamAsBytes(s)));
        }

        public static async Task<Stream> SendPost(string url, HttpContent content, bool includeKey = true)
        {
            using (HttpClient hc = new HttpClient())
            {
                //Send
                if (includeKey)
                    hc.DefaultRequestHeaders.Add("X-Delta-System-Key", DeltaMapTools.ACCESS_KEY);
                hc.DefaultRequestHeaders.Add("X-Delta-System-Name", DeltaMapTools.USER_AGENT);
                var response = await hc.PostAsync(url, content);

                //Check
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Unexpected status code.");
                return await response.Content.ReadAsStreamAsync();
            }
        }

        public static async Task<T> SendPostJson<T, O>(string url, O content, bool includeKey = true)
        {
            //Get
            Stream s = await SendPost(url, new StringContent(JsonConvert.SerializeObject(content)), includeKey);
            return ReadStringAsJson<T>(ReadBytesAsString(ReadStreamAsBytes(s)));
        }

        //Tools
        private static byte[] ReadStreamAsBytes(Stream s)
        {
            byte[] buf = new byte[s.Length];
            s.Read(buf, 0, buf.Length);
            return buf;
        }

        private static string ReadBytesAsString(byte[] s)
        {
            return Encoding.UTF8.GetString(s);
        }

        private static T ReadStringAsJson<T>(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }
    }
}
