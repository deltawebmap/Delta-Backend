using ArkWebMapMasterServer.ServiceDefinitions.Auth.AppAuth;
using ArkWebMapMasterServer.ServiceDefinitions.Misc;
using ArkWebMapMasterServer.ServiceDefinitions.Servers;
using ArkWebMapMasterServer.ServiceDefinitions.Servers.Canvas;
using ArkWebMapMasterServer.ServiceDefinitions.User;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.MiscNet;
using LibDeltaSystem.WebFramework;
using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer
{
    class Program
    {
        public static MasterServerConfig config;
        public static DeltaConnection connection;

        static void Main(string[] args)
        {
            Console.WriteLine("Loading config...");
            config = JsonConvert.DeserializeObject<MasterServerConfig>(File.ReadAllText(args[0]));

            Console.WriteLine("Connecting to MongoDB...");
            connection = new DeltaConnection(config.database_config_path, "master", 0, 0);
            connection.Connect().GetAwaiter().GetResult();

            Console.WriteLine("Starting Server...");
            V2SetupServer().GetAwaiter().GetResult();
        }

        public static async Task V2SetupServer()
        {
            var server = new DeltaWebServer(connection, config.listen_port);

            //Misc
            server.AddService(new MapListDefinition());

            //Auth
            server.AddService(new AppAuthBeginDefinition());
            server.AddService(new AppAuthEndDefinition());
            server.AddService(new AppAuthTokenDefinition());

            //Server
            server.AddService(new CanvasListDefinition());
            server.AddService(new CanvasSelectDefinition());
            server.AddService(new ServerManageDefinition());
            server.AddService(new PutUserPrefsDefinition());

            //User
            server.AddService(new UsersMeDefinition());
            server.AddService(new PutUserSettingsDefinition());
            server.AddService(new TokenDevalidateDefinition());
            server.AddService(new UserClustersDefinition());
            server.AddService(new UserOAuthApplicationsDefinition_Root());
            server.AddService(new UserOAuthApplicationsDefinition_Item());
            server.AddService(new IssueCreatorDefinition());

            //Start
            await server.RunAsync();
        }

        public static Task QuickWriteToDoc(Microsoft.AspNetCore.Http.HttpContext context, string content, string type = "text/html", int code = 200)
        {
            var response = context.Response;
            response.StatusCode = code;
            response.ContentType = type;

            //Load the template.
            string html = content;
            var data = Encoding.UTF8.GetBytes(html);
            response.ContentLength = data.Length;
            return response.Body.WriteAsync(data, 0, data.Length);
        }

        public static string GetPostBodyString(Microsoft.AspNetCore.Http.HttpContext context)
        {
            string buffer;
            using (StreamReader sr = new StreamReader(context.Request.Body))
                buffer = sr.ReadToEnd();

            return buffer;
        }

        public static T DecodePostBody<T>(Microsoft.AspNetCore.Http.HttpContext context)
        {
            string buffer = GetPostBodyString(context);

            //Deserialize
            return JsonConvert.DeserializeObject<T>(buffer);
        }

        public static Task QuickWriteStatusToDoc(Microsoft.AspNetCore.Http.HttpContext e, bool ok, int code = 200)
        {
            return QuickWriteJsonToDoc(e, new OkStatusResponse
            {
                ok = ok
            }, code);
        }

        public static Task QuickWriteJsonToDoc<T>(Microsoft.AspNetCore.Http.HttpContext context, T data, int code = 200)
        {
            return QuickWriteToDoc(context, JsonConvert.SerializeObject(data, Formatting.Indented), "application/json", code);
        }

        public static RequestHttpMethod FindRequestMethod(Microsoft.AspNetCore.Http.HttpContext context)
        {
            return Enum.Parse<RequestHttpMethod>(context.Request.Method.ToLower());
        }
    }

    public enum RequestHttpMethod
    {
        get,
        post,
        put,
        delete,
        options
    }
}
