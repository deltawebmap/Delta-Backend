using ArkWebMapMasterServer.NetEntities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static ArkWebMapMasterServer.Services.Misc.ServerValidation;

namespace LibDelta
{
    public static class DeltaAuth
    {
        public static async Task<UsersMeReply> AuthenticateUser(string token)
        {
            //Check if it's cached
            if (cached_tokens.TryGetValue(token, out UsersMeReply user))
                return user;

            //We'll need to add it
            try
            {
                user = await WebTools.SendUserGetJson<UsersMeReply>(DeltaMapTools.API_ROOT + "users/@me/", token);

                //Add to the cache
                cached_tokens.TryAdd(token, user);

                //Respond with this
                return user;
            } catch
            {
                //Auth failed.
                return null;
            }
        }

        public static async Task<ServerValidationResponsePayload> AuthenticateServer(string id, string b64Creds)
        {
            //Create body
            ServerValidationRequestPayload request = new ServerValidationRequestPayload
            {
                server_creds = b64Creds,
                server_id = id
            };

            //Submit
            return await WebTools.SendPostJson<ServerValidationResponsePayload, ServerValidationRequestPayload>(DeltaMapTools.API_ROOT+"server_validation", request);
        }

        private static ConcurrentDictionary<string, UsersMeReply> cached_tokens = new ConcurrentDictionary<string, UsersMeReply>();
    }
}
