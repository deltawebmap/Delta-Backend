using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public static class ServiceStatus
    {
        static readonly DeltaService[] services = new DeltaService[]
        {
            new DeltaService
            {
                url = "https://deltamap.net/api/",
                name = "Master Server",
                description = "Handles users, servers, and other important data.",
                down_message = "Without this service, you will not be able to access the app.",
                important = true,
                id = 0
            },
            new DeltaService
            {
                url = "https://gateway.deltamap.net:43297/",
                name = "Gateway",
                description = "Sends important events between all services.",
                down_message = "Without this service, some actions won't appear without a page refresh.",
                important = true,
                id = 1
            },
            new DeltaService
            {
                url = "https://lightspeed.deltamap.net/online",
                name = "Lightspeed Proxy",
                description = "Relays server data from Ark servers to you.",
                down_message = "Without this service, only offline data can be accessed.",
                important = true,
                id = 2
            },
            new DeltaService
            {
                url = "https://dynamic-tiles.deltamap.net/",
                name = "Dynamic Tiles",
                description = "Provides structure images and other map tiles that can be changed.",
                down_message = "Without this service, structure images will not load.",
                important = false,
                id = 3
            },
            new DeltaService
            {
                url = "https://config.deltamap.net/",
                name = "Remote Config",
                description = "Provides configuration files to all parts of the service.",
                down_message = "Without this service, new sessions cannot be created.",
                important = true,
                id = 4
            },
            new DeltaService
            {
                url = "https://tile-assets.deltamap.net/",
                name = "Tile Assets CDN",
                description = "Distributes static map tiles.",
                down_message = "Without this service, the Ark map background will not load.",
                important = true,
                id = 5
            },
            new DeltaService
            {
                url = "https://icon-assets.deltamap.net/",
                name = "Icon Assets CDN",
                description = "Distributes static icon assets, such as dino images, item icons, and other Ark assets.",
                down_message = "Without this service, no dino images or item icons will appear.",
                important = true,
                id = 6
            },
            new DeltaService
            {
                url = "https://web-analytics.deltamap.net/",
                name = "Web Analytics",
                description = "Reports analytics from clients about usage.",
                down_message = "You will not notice this service going down.",
                important = false,
                id = 7
            },
            new DeltaService
            {
                url = "https://offline-content.deltamap.net/",
                name = "Offline Content",
                description = "Used to upload/download offline server data. Offline data is used when the Delta Web Map client is closed.",
                down_message = "Without this service, no servers that aren't currently running will download data.",
                important = true,
                id = 8
            },
        };

        /// <summary>
        /// Pings services to see if they're alive. Does some weird streaming.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //This does some weird chunking
            e.Response.ContentType = "application/json";

            //First, send all of the data to show it.
            await SendData(e, services);
            DateTime startTime = DateTime.UtcNow;

            //Keep testing connections until this is closed.
            while(!e.RequestAborted.IsCancellationRequested && (DateTime.UtcNow - startTime).TotalMinutes < 1)
            {
                //Do all
                await TestAll(e, startTime);
                await Task.Delay(400);
            }
        }

        private static async Task TestAll(Microsoft.AspNetCore.Http.HttpContext e, DateTime startTime)
        {
            //Loop through and ping all services
            Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[services.Length];
            bool[] finished = new bool[services.Length];
            int completed = 0;
            DateTime[] serviceStarts = new DateTime[services.Length];
            for (int i = 0; i < services.Length; i += 1)
            {
                HttpClient hc = new HttpClient();
                hc.Timeout = TimeSpan.FromSeconds(5);
                serviceStarts[i] = DateTime.UtcNow;
                tasks[i] = hc.GetAsync(services[i].url);
                finished[i] = false;
            }

            //A little janky, but that's ok
            while (completed < services.Length)
            {
                //Loop through and find tasks that are newly finished
                for (int i = 0; i < tasks.Length; i += 1)
                {
                    //Check
                    if (finished[i] == true || !tasks[i].IsCompleted)
                        continue;

                    //This is a newly finished service. Write it.
                    Task<HttpResponseMessage> data = tasks[i];
                    bool ok = data.IsCompletedSuccessfully;
                    if (ok)
                    {
                        ok = data.Result.StatusCode == System.Net.HttpStatusCode.OK || data.Result.StatusCode == System.Net.HttpStatusCode.NotFound || data.Result.StatusCode == System.Net.HttpStatusCode.InternalServerError || data.Result.StatusCode == System.Net.HttpStatusCode.Forbidden || data.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized || data.Result.StatusCode == System.Net.HttpStatusCode.BadRequest; //We accept server errors or 404s, because may of the services return that, even if they work.
                    }

                    //Set status
                    completed++;
                    finished[i] = true;

                    //Write response
                    await SendData(e, new DeltaServiceStatus
                    {
                        id = services[i].id,
                        message = "",
                        ok = ok,
                        delta_time = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                        time = DateTime.UtcNow,
                        ping = (int)(DateTime.UtcNow - serviceStarts[i]).TotalMilliseconds
                    });
                }

                //Wait a sec
                await Task.Delay(5);
            }
        }

        private static async Task SendData<T>(Microsoft.AspNetCore.Http.HttpContext e, T data)
        {
            string j = JsonConvert.SerializeObject(data);
            j = j.Length.ToString().PadLeft(8, '0') + j;
            byte[] b = Encoding.UTF8.GetBytes(j);
            await e.Response.Body.WriteAsync(b, 0, b.Length);
            await e.Response.Body.FlushAsync();
        }

        class DeltaService
        {
            public string url;
            public string name;
            public string description;
            public string down_message;
            public bool important;
            public int id;
        }

        class DeltaServiceStatus
        {
            public bool ok;
            public int id;
            public string message;

            public DateTime time;
            public int delta_time; //Time, in ms, since the page was first loaded
            public int ping; //Time, in ms, that the request took
        }
    }
}
