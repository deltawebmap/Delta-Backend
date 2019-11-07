using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer
{
    public static class ApiTools
    {
        /// <summary>
        /// Gets a token from the Bearer token in the header
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetBearerToken(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Get the token
            string token = null;
            if (e.Request.Headers.ContainsKey("Authorization"))
            {
                //Read Authorization header
                token = e.Request.Headers["Authorization"];
                if (token.StartsWith("Bearer "))
                    token = token.Substring("Bearer ".Length);
            }
            return token;
        }
        
        /// <summary>
        /// Authenticates a Bearer token
        /// </summary>
        /// <param name="e"></param>
        /// <param name="required"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<DbUser> AuthenticateUser(string token, bool required)
        {
            //Authenticate
            if (token == null && required)
                throw new StandardError("No Auth Token Provided.", StandardErrorCode.AuthRequired);
            DbUser user = await Program.connection.AuthenticateUserToken(token);
            if (user == null && required)
                throw new StandardError("You're not signed in.", StandardErrorCode.AuthRequired);
            return user;
        }

        /// <summary>
        /// Authenticates a Bearer token
        /// </summary>
        /// <param name="e"></param>
        /// <param name="required"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<DbUser> AuthenticateUser(Microsoft.AspNetCore.Http.HttpContext e, bool required)
        {
            string token = GetBearerToken(e);
            return await AuthenticateUser(token, required);
        }
    }
}
