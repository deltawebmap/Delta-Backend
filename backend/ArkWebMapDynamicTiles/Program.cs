using ArkHttpServer.Entities;
using ArkWebMapDynamicTiles.Entities;
using LibDelta;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapDynamicTiles
{
    class Program
    {
        public static Random rand = new Random();
        public static PrimalDataImagePackage image_package;

        static void Main(string[] args)
        {
            //Init the lib
            DeltaMapTools.Init(SystemKeys.ObtainKey(), "DYNAMIC_IMAGES_EDGE");

            //Get the db
            ContentTool.db = new LiteDB.LiteDatabase("content.db");

            //Import content
            image_package = ImportImages("images.pdip");

            //Start web server
            MainAsync().GetAwaiter().GetResult();
        }

        static PrimalDataImagePackage ImportImages(string pathname)
        {
            //Open stream and begin reading
            PrimalDataImagePackage package;
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
                            Image<Rgba32> img;
                            using (Stream s = za.GetEntry(i.Value).Open())
                                img = Image.Load(s);
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
                    options.Listen(addr, 43295);
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
