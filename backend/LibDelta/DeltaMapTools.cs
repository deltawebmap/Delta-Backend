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

        public static void Init(string key, string name)
        {
            ACCESS_KEY = key;
            USER_AGENT = name;
        }

        
    }
}
