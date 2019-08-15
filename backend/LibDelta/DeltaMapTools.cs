using ArkWebMapGatewayClient;
using System;
using System.Net;
using System.Threading.Tasks;

namespace LibDelta
{
    public class DeltaMapTools
    {
        public const string API_ROOT = "https://deltamap.net/api/";

        public static string ACCESS_KEY;
        public static string USER_AGENT;

        public static AWMGatewayClient gateway;

        public static void Init(string key, string name, GatewayMessageHandler handler)
        {
            ACCESS_KEY = key;
            USER_AGENT = name;
            gateway = AWMGatewayClient.CreateClient(GatewayClientType.System, USER_AGENT, "LibDelta", 1, 1, false, handler, ACCESS_KEY);
        }

        
    }
}
