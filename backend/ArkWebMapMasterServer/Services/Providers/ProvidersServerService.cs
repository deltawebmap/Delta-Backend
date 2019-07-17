using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.Managers;
using ArkWebMapMasterServer.NetEntities.Managers;
using ArkWebMapMasterServer.PresistEntities.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Providers
{
    public static class ProvidersServerService
    {
        public static Task CreateMachine(Microsoft.AspNetCore.Http.HttpContext e, ArkManager user)
        {
            //Decode args
            CreateServerRequestArk request = Program.DecodePostBody<CreateServerRequestArk>(e);

            //Get machine
            ArkManagerMachine machine = ManageMachines.GetMachineById(user, request.machineId);

            //Create
            ArkManagerServer server = ManageServers.CreateArkServer(user, machine, request.name, request.serverName, request.clientId, new ArkManagerServerGame_ARK
            {
                map_name = request.mapName,
                map_path = request.mapPath
            });

            //Write it's info
            return Program.QuickWriteJsonToDoc(e, server);
        }

        public static Task SelectGet(Microsoft.AspNetCore.Http.HttpContext e, ArkManagerServer server, ArkManager user)
        {
            return Program.QuickWriteJsonToDoc(e, server);
        }

        public static Task SelectPost(Microsoft.AspNetCore.Http.HttpContext e, ArkManagerServer server, ArkManager user)
        {
            throw new StandardError("Not Implemented", StandardErrorCode.NotImplemented);
        }

        public static Task SelectDelete(Microsoft.AspNetCore.Http.HttpContext e, ArkManagerServer server, ArkManager user)
        {
            ManageServers.DeleteArkServer(server);
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
