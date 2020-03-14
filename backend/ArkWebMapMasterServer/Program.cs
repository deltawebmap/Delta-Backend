using ArkWebMapMasterServer.ServiceDefinitions.Auth.AppAuth;
using ArkWebMapMasterServer.ServiceDefinitions.Auth.OAuth;
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
            server.AddService(new OAuthQueryDefinition());
            server.AddService(new OAuthAuthorizeDefinition());
            server.AddService(new OAuthVerifyDefinition());

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
    }
}
