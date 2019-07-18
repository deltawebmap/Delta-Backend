using ArkBridgeSharedEntities.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ArkWebMapGateway
{
    public static class ServerDataHolder
    {
        private static Dictionary<string, List<ArkSlaveReport_PlayerAccount>> serverTribes = new Dictionary<string, List<ArkSlaveReport_PlayerAccount>>();

        public static List<ArkSlaveReport_PlayerAccount> GetServerMembers(string id)
        {
            //Check if we have it saved
            if (serverTribes.ContainsKey(id))
                return serverTribes[id];

            //We'll have to manually fetch it from the server.
            using (WebClient wc = new WebClient())
            {
                string s = wc.DownloadString("https://deltamap.net/api/servers/"+id+"/users");
                List<ArkSlaveReport_PlayerAccount> data = JsonConvert.DeserializeObject<List<ArkSlaveReport_PlayerAccount>>(s);
                lock(serverTribes)
                {
                    if(!serverTribes.ContainsKey(id))
                    {
                        serverTribes.Add(id, data);
                    }
                }
                return data;
            }
        }
    }
}
