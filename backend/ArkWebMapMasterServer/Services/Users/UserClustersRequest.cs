using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class UserClustersRequest : UserAuthDeltaService
    {
        public UserClustersRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            return true;
        }

        public override async Task OnRequest()
        {
            //Get method
            var method = GetMethod();
            if (method == LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.GET)
                await OnGETRequest(e, user);
            else if (method == LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
                await OnPOSTRequest(e, user);
            else
                throw new StandardError("This method was not expected.", StandardErrorCode.BadMethod);
        }

        public async Task OnGETRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser u)
        {
            //Get clusters
            var clusters = await DbCluster.GetClustersForUser(Program.connection, u._id);

            //Convert all
            List<ClusterResponseData> response = new List<ClusterResponseData>();
            foreach(var c in clusters)
            {
                response.Add(new ClusterResponseData
                {
                    id = c.id,
                    name = c.name
                });
            }

            await WriteJSON(response);
        }

        public async Task OnPOSTRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser u)
        {
            //Decode data
            var request = await DecodePOSTBody<ClusterCreateData>();

            //Check
            if (request.name == null)
                throw new StandardError("Missing name field.", StandardErrorCode.InvalidInput);
            if (request.name.Length > 24 || request.name.Length < 2)
                throw new StandardError("Name field is too long or too short.", StandardErrorCode.InvalidInput);

            //Add cluster
            var cluster = new DbCluster
            {
                _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                name = request.name,
                owner = u._id
            };
            await Program.connection.system_clusters.InsertOneAsync(cluster);

            //Write the response
            await WriteJSON(new ClusterResponseData
            {
                id = cluster.id,
                name = cluster.name
            });
        }

        class ClusterCreateData
        {
            public string name;
        }

        class ClusterResponseData
        {
            public string id;
            public string name;
        }
    }
}
