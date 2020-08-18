using LibDeltaSystem;
using LibDeltaSystem.Db;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class UsersMeRequest : UserAuthDeltaService
    {
        public UsersMeRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            return true;
        }

        public override async Task OnRequest()
        {
            //Set up our basic values
            UsersMeResponse response = new UsersMeResponse
            {
                screen_name = user.screen_name,
                profile_image_url = user.profile_image_url,
                id = user.id,
                steam_id = user.steam_id,
                user_settings = user.user_settings,
                server_token = user.server_creation_token,
                servers = new List<NetGuildUser>(),
                clusters = new List<NetCluster>()
            };

            //Fetch and convert our servers
            var profileServers = await user.GetGameServersAsync(Program.connection);
            List<string> clusterIds = new List<string>();
            List<ObjectId> serverIds = new List<ObjectId>();
            foreach (var s in profileServers)
            {
                //Get guild
                var guild = await NetGuildUser.GetNetGuild(Program.connection, s.Item1, user, s.Item2);

                //Add to lists
                if (guild.cluster_id != null && !clusterIds.Contains(guild.cluster_id))
                    clusterIds.Add(guild.cluster_id);
                serverIds.Add(s.Item1._id);
                response.servers.Add(guild);
            }

            //Add servers we admin, but we don't already have added
            var adminServers = await user.GetAdminedServersAsync(conn);
            foreach (var s in adminServers)
            {
                //Check if we already have this server
                if (serverIds.Contains(s._id))
                    continue;
                
                //Get guild. We can assume we do not have a profile
                var guild = await NetGuildUser.GetNetGuild(conn, s, user, null);

                //Add to lists
                if (guild.cluster_id != null && !clusterIds.Contains(guild.cluster_id))
                    clusterIds.Add(guild.cluster_id);
                serverIds.Add(s._id);
                response.servers.Add(guild);
            }

            //Add clusters
            foreach (var c in clusterIds)
            {
                //Get cluster data
                var cluster = await DbCluster.GetClusterById(Program.connection, MongoDB.Bson.ObjectId.Parse(c));

                //Add cluster data
                response.clusters.Add(NetCluster.GetCluster(cluster));
            }

            //Write response
            await WriteJSON(response);
        }

        class UsersMeResponse
        {
            public string screen_name;
            public string profile_image_url;
            public string id;
            public string steam_id;
            public string server_token;
            public DbUserSettings user_settings;
            public List<NetGuildUser> servers;
            public List<NetCluster> clusters;
        }
    }
}
