using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway
{
    public static class ServerNameCache
    {
        public static Dictionary<string, string> serverNames = new Dictionary<string, string>();

        public static string GetServerName(string name)
        {
            if (serverNames.ContainsKey(name))
                return serverNames[name];
            return "N/A";
        }

        public static void UpdateOrInsertServer(string id, string name)
        {
            lock(serverNames)
            {
                if (serverNames.ContainsKey(id))
                {
                    serverNames[id] = name;
                }
                else
                {
                    serverNames.Add(id, name);
                }
            }
        }
    }
}
