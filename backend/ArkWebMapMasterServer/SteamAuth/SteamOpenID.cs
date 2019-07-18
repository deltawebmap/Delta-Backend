using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using IdentityModel.OidcClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace ArkWebMapMasterServer.SteamAuth
{
    public class SteamOpenID
    {
        /// <summary>
        /// Holds "sessions" that have objects that are returned to the server when auth is complete. Maps those to an ID.
        /// </summary>
        public static Dictionary<string, object> return_values = new Dictionary<string, object>();

        /// <summary>
        /// Starts a session and returns a URL to redirect to.
        /// </summary>
        /// <param name="returner"></param>
        /// <returns></returns>
        public static string Begin(string mode)
        {
            //First, generate a state ID. This should be unique
            string stateId = Program.GenerateRandomString(24);
            while(return_values.ContainsKey(stateId))
                stateId = Program.GenerateRandomString(24);

            //Add
            return_values.Add(stateId, "");

            //Now, construct a URL to send the user to.
            string return_url = $"https://deltamap.net/api/auth/steam_auth_return/?state={stateId}&mode={mode}";
            string encoded_return_url = System.Web.HttpUtility.UrlEncode(return_url);
            string url = $"https://steamcommunity.com/openid/login?openid.return_to={encoded_return_url}&openid.mode=checkid_setup&openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.identity=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.claimed_id=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.ns.sreg=http%3A%2F%2Fopenid.net%2Fextensions%2Fsreg%2F1.1&openid.realm={encoded_return_url}";
            return url;
        }

        public static SteamValidationResponse Finish(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //We'll now validate this with Steam. Create the request back to Steam servers
            string validation_url = "https://steamcommunity.com/openid/login"+e.Request.QueryString.Value.Replace("openid.mode=id_res", "openid.mode=check_authentication");
            string validation_return;
            try
            {
                using (WebClient hc = new WebClient())
                    validation_return = hc.DownloadString(validation_url);
            } catch
            {
                throw new StandardError("Steam server returned an error.", StandardErrorCode.ExternalAuthError);
            }

            //Return validation is really gross. We're just going to use a find.
            bool validation_failed = validation_return.Contains("is_valid:false");
            bool validation_ok = validation_return.Contains("is_valid:true");
            if (!validation_ok && !validation_failed)
                throw new StandardError("Steam server did not return true/false when asked for validation.", StandardErrorCode.ExternalAuthError);

            //If return validation failed, throw an error
            if (validation_failed)
                throw new StandardError("Steam reported validation failure.", StandardErrorCode.AuthFailed);

            //Now, we have their ID and have validated it. Extract it from the URL.
            string steam_id = e.Request.Query["openid.claimed_id"].ToString().Substring("https://steamcommunity.com/openid/id/".Length);

            //Get the state ID
            string state_id = e.Request.Query["state"];
            if (!return_values.ContainsKey(state_id))
                throw new StandardError("Could not find state.", StandardErrorCode.AuthFailed);
            object state = return_values[state_id];

            //Also request this users' Steam profile.
            SteamProfile profile = RequestSteamUserData(steam_id);

            //Output
            return new SteamValidationResponse
            {
                ok = true,
                profile = profile,
                steam_id = steam_id
            };
        }

        public static SteamProfile RequestSteamUserData(string steam_id)
        {
            string profile_return;
            string profile_url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={Program.config.steam_api_key}&steamids={steam_id}";
            try
            {
                using (WebClient hc = new WebClient())
                    profile_return = hc.DownloadString(profile_url);
            }
            catch
            {
                throw new StandardError("Steam server returned an error while downloading profile.", StandardErrorCode.ExternalAuthError);
            }

            //Deserialize the profile
            SteamProfile_Full profile_full;
            try
            {
                profile_full = JsonConvert.DeserializeObject<SteamProfile_Full>(profile_return);
            }
            catch
            {
                throw new StandardError("Failed to deserialize the profile", StandardErrorCode.ExternalAuthError);
            }

            //Check to make sure we actuall have the the player
            if (profile_full.response.players.Count != 1)
                throw new StandardError("Steam profile response did not contain your player info.", StandardErrorCode.AuthFailed);

            return profile_full.response.players[0];
        }

        public enum SteamAuthMode
        {
            Web,
            AndroidClient
        }
    }
}
