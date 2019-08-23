using ArkHttpServer.Entities;
using ArkWebMapDynamicTiles.Entities;
using ArkWebMapDynamicTiles.MapSessions;
using LibDelta;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ArkWebMapDynamicTiles
{
    class Program
    {
        public static Random rand = new Random();
        public static PrimalDataImagePackage image_package;
        public static Timer kill_timer;
        public static ConfigFile config;

        public static Dictionary<string, PublicStructureSize> structure_size_map = new Dictionary<string, PublicStructureSize>();

        public const int HEARTBEAT_POLICY_MS = 60000;
        public const int HEARTBEAT_EXPIRE_TIME_ADD = 30000; //Time + HEARTBEAT_POLICY_MS that the session will be expired.

        public const string SESSION_ROOT = "https://dynamic-tiles.deltamap.net/";

        static void Main(string[] args)
        {
            //Open config file
            config = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(args[0]));

            //Init the lib
            DeltaMapTools.Init(config.system_api_key, "DYNAMIC_IMAGES_EDGE", new ArkWebMapGatewayClient.GatewayMessageHandler());

            //Get the db
            ContentTool.db = new LiteDB.LiteDatabase(config.database_path);

            //Import content
            image_package = ImportImages(config.image_content_path);

            //Start kill timer
            kill_timer = new Timer(1000);
            kill_timer.AutoReset = true;
            kill_timer.Elapsed += Kill_timer_Elapsed;
            kill_timer.Start();

            //Start web server
            MainAsync().GetAwaiter().GetResult();
        }

        private static void Kill_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SessionTool.PurgeSessions();
        }

        static PrimalDataImagePackage ImportImages(string pathname)
        {
            //Open stream and begin reading
            PrimalDataImagePackage package;
            structure_size_map = new Dictionary<string, PublicStructureSize>();
            using (FileStream fs = new FileStream(pathname, System.IO.FileMode.Open, FileAccess.Read))
            {
                using (ZipArchive za = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    //Read metadata
                    PrimalDataImagesMetadata meta = ZipTools.ReadEntryAsJson<PrimalDataImagesMetadata>("package_metadata.json", za);

                    //Open images
                    package = new PrimalDataImagePackage();
                    package.images = new Dictionary<string, Dictionary<string, Image<Rgba32>>>();
                    foreach (var type in meta.data)
                    {
                        Dictionary<string, Image<Rgba32>> imgs = new Dictionary<string, Image<Rgba32>>();
                        foreach (var i in type.Value)
                        {
                            //Read image
                            Image<Rgba32> source;
                            using (Stream s = za.GetEntry(i.Value).Open())
                                source = Image.Load(s);

                            //Add structure size map
                            structure_size_map.Add(i.Key, new PublicStructureSize
                            {
                                height = source.Height,
                                width = source.Width
                            });

                            //Resize to square
                            int size = Math.Max(source.Width, source.Height);
                            Image<Rgba32> img = new Image<Rgba32>(size, size);
                            int offsetX = (size - source.Width) / 2;
                            int offsetY = (size - source.Height) / 2;
                            for(int x = 0; x<source.Width; x++)
                            {
                                for(int y = 0; y<source.Height; y++)
                                {
                                    img[x + offsetX, y + offsetY] = source[x, y];
                                }
                            }

                            //add
                            imgs.Add(i.Key, img);
                        }
                        package.images.Add(type.Key, imgs);
                    }
                }
            }
            return package;
        }

        static async Task MainAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, config.web_port);
                })
                .UseStartup<Program>()
                .Build();

            await host.RunAsync();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(HttpHandler.OnHttpRequest);
        }

        public static async Task QuickWriteToDoc(Microsoft.AspNetCore.Http.HttpContext context, string content, string type = "text/html", int code = 200)
        {
            var response = context.Response;
            response.StatusCode = code;
            response.ContentType = type;

            //Load the template.
            string html = content;
            var data = Encoding.UTF8.GetBytes(html);
            response.ContentLength = data.Length;
            await response.Body.WriteAsync(data, 0, data.Length);
        }

        public static string GenerateRandomString(int length)
        {
            return GenerateRandomStringCustom(length, "1234567890ABCDEF".ToCharArray());
        }

        public static string GenerateRandomStringCustom(int length, char[] chars)
        {
            string output = "";
            for (int i = 0; i < length; i++)
            {
                output += chars[rand.Next(0, chars.Length)];
            }
            return output;
        }

        public static T DecodePostBody<T>(Microsoft.AspNetCore.Http.HttpContext context)
        {
            string buffer;
            using (StreamReader sr = new StreamReader(context.Request.Body))
                buffer = sr.ReadToEnd();

            //Deserialize
            return JsonConvert.DeserializeObject<T>(buffer);
        }
    }
}
