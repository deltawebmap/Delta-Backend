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
    public static class ProvidersMachineService
    {
        public static Task CreateMachine(Microsoft.AspNetCore.Http.HttpContext e, ArkManager user)
        {
            //Decode args
            CreateMachineRequest request = Program.DecodePostBody<CreateMachineRequest>(e);

            //Create
            ArkManagerMachine machine = ManageMachines.CreateMachine(user, request.name, request.location);

            //Write it's info
            return Program.QuickWriteJsonToDoc(e, machine);
        }

        public static Task SelectGet(Microsoft.AspNetCore.Http.HttpContext e, ArkManagerMachine machine, ArkManager user)
        {
            return Program.QuickWriteJsonToDoc(e, machine);
        }

        public static Task SelectPost(Microsoft.AspNetCore.Http.HttpContext e, ArkManagerMachine machine, ArkManager user)
        {
            throw new StandardError("Not Implemented", StandardErrorCode.NotImplemented);
        }

        public static Task SelectDelete(Microsoft.AspNetCore.Http.HttpContext e, ArkManagerMachine machine, ArkManager user)
        {
            throw new StandardError("Not Implemented", StandardErrorCode.NotImplemented);
        }
    }
}
