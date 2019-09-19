using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Servers;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Clusters
{
    public static class ClustersHttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Authenticate user
            DbUser user = Users.UsersHttpHandler.AuthenticateUser(e, true, out string userToken);

            //Handle
            return ArrayActionsHandler.OnHttpRequest(e, path, user, ArkClusterTool.GetCollection(), CreateMachine, SelectGet, SelectPost, SelectDelete);
        }

        public static Task CreateMachine(Microsoft.AspNetCore.Http.HttpContext e, DbUser user)
        {
            //Create a new cluster. Get the args
            ClusterEditArgs edit = Program.DecodePostBody<ClusterEditArgs>(e);

            //Check name length
            if(edit.name.Length > 24 || edit.name.Length < 2)
                throw new StandardError("Name must be between 2-24 characters long.", StandardErrorCode.InvalidInput);

            //Generate a new ID
            string id = Program.GenerateRandomString(24);
            while (ArkClusterTool.GetClusterById(id) != null)
                id = Program.GenerateRandomString(24);

            //Create object and add it
            ArkCluster cluster = new ArkCluster
            {
                name = edit.name,
                owner_id = user.id,
                _id = id
            };
            ArkClusterTool.GetCollection().Insert(cluster);

            //Write
            return Program.QuickWriteJsonToDoc(e, cluster);
        }

        public static Task SelectGet(Microsoft.AspNetCore.Http.HttpContext e, ArkCluster machine, DbUser user)
        {
            return Program.QuickWriteJsonToDoc(e, machine);
        }

        public static Task SelectPost(Microsoft.AspNetCore.Http.HttpContext e, ArkCluster machine, DbUser user)
        {
            //Check if we own this cluster. If we do, rename it and save
            ClusterEditArgs edit = Program.DecodePostBody<ClusterEditArgs>(e);
            machine.name = edit.name;
            ArkClusterTool.GetCollection().Update(machine);

            //TODO: Send gateway message
            return Program.QuickWriteJsonToDoc(e, machine);
        }

        public static Task SelectDelete(Microsoft.AspNetCore.Http.HttpContext e, ArkCluster machine, DbUser user)
        {
            throw new StandardError("Not Implemented", StandardErrorCode.NotImplemented);
        }
    }
}
