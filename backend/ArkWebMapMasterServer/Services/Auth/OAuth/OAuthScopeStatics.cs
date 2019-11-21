using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapMasterServer.Services.Auth.OAuth
{
    public static class OAuthScopeStatics
    {
        public static readonly OAuthScopeEntry[] scopes = new OAuthScopeEntry[]
        {
            new OAuthScopeEntry
            {
                id = "USER_INFO",
                name = "View your Steam ID and servers you are on.",
                is_dangerous = false
            },
            new OAuthScopeEntry
            {
                id = "VIEW_SERVER_INFO",
                name = "Read tribe data on all of your servers.",
                is_dangerous = false
            },
            new OAuthScopeEntry
            {
                id = "PUT_DINO_PREFS",
                name = "Modify dino settings, such as notes and color codes.",
                is_dangerous = false
            }
        };

        public static List<OAuthScopeEntry> GetOAuthScopes(string[] requestedScopes)
        {
            List<OAuthScopeEntry> responses = new List<OAuthScopeEntry>();
            foreach(var r in requestedScopes)
            {
                OAuthScopeEntry s = scopes.Where(x => x.id == r).FirstOrDefault();
                if (s == null)
                    continue;
                if (responses.Contains(s))
                    continue;
                responses.Add(s);
            }
            return responses;
        }

        public static string[] GetOAuthScopeIDs(List<OAuthScopeEntry> scopes, out bool is_dangerous)
        {
            string[] responseScopes = new string[scopes.Count];
            is_dangerous = false;
            for (int i = 0; i < responseScopes.Length; i++)
            {
                responseScopes[i] = scopes[i].id;
                if (scopes[i].is_dangerous && !is_dangerous)
                    is_dangerous = true;
            }
            return responseScopes;
        }

        public class OAuthScopeEntry
        {
            public string id;
            public string name;
            public bool is_dangerous;
        }
    }
}
