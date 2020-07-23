using ArkWebMapMasterServer.ServiceDefinitions.Auth;
using ArkWebMapMasterServer.ServiceDefinitions.Auth.NewAuth;
using ArkWebMapMasterServer.ServiceDefinitions.Misc;
using ArkWebMapMasterServer.ServiceDefinitions.Servers;
using ArkWebMapMasterServer.ServiceDefinitions.Servers.Admin;
using ArkWebMapMasterServer.ServiceDefinitions.Servers.Canvas;
using ArkWebMapMasterServer.ServiceDefinitions.User;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.MiscNet;
using LibDeltaSystem.WebFramework;
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
        public static DeltaConnection connection;

        public const int APP_VERISON_MAJOR = 0;
        public const int APP_VERISON_MINOR = 3;

        static void Main(string[] args)
        {
            connection = DeltaConnection.InitDeltaManagedApp(args, APP_VERISON_MAJOR, APP_VERISON_MINOR, new DeltaNetworkMasterServer());
            V2SetupServer().GetAwaiter().GetResult();
        }

        public static async Task V2SetupServer()
        {
            var server = new DeltaWebServer(connection, connection.GetUserPort(0));

            //Misc
            server.AddService(new MapListDefinition());

            //Auth
            server.AddService(new ValidateBetaKeyDefinition());
            server.AddService(new NewAuthBeginDefinition());
            server.AddService(new NewAuthAuthorizeDefinition());
            server.AddService(new NewAuthAppValidateDefinition());

            //Server
            server.AddService(new CanvasListDefinition());
            server.AddService(new CanvasSelectDefinition());
            server.AddService(new PutUserPrefsDefinition());
            server.AddService(new LeaveServerDefinition());
            server.AddService(new ServerTribesDefinition());
            server.AddService(new AdminServerPlayerListDefinition());
            server.AddService(new AdminServerSetSecureModeDefinition());
            server.AddService(new AdminServerSetPermissionsDefinition());
            server.AddService(new AdminServerDeleteDefinition());
            server.AddService(new AdminServerBanUserDefinition());
            server.AddService(new AdminServerSetIconDefinition());
            server.AddService(new AdminServerSetLockedStatusDefinition());
            server.AddService(new AdminServerSetNameDefinition());

            //User
            server.AddService(new UsersMeDefinition());
            server.AddService(new PutUserSettingsDefinition());
            server.AddService(new TokenDevalidateDefinition());
            server.AddService(new UserClustersDefinition());
            server.AddService(new UserOAuthApplicationsDefinition_Root());
            server.AddService(new UserOAuthApplicationsDefinition_Item());

            //Start
            await server.RunAsync();
        }
    }
}
