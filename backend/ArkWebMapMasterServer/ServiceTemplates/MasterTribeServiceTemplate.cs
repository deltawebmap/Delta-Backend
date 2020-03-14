using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.ServiceTemplates
{
    /// <summary>
    /// A service template used for the master server. Provides tribe and server info for server endpoints
    /// </summary>
    public abstract class MasterTribeServiceTemplate : CheckedTribeServerDeltaService
    {
        public MasterTribeServiceTemplate(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override bool CheckTribeID(int? tribeId)
        {
            if (tribeId.HasValue)
                return true; //Always ok; this is an authenticated tribe
            return server.CheckIsUserAdmin(user);
        }

        /// <summary>
        /// Requires that we have a tribe and returns null if we don't, writing an error
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RequireTribe()
        {
            if (tribeId.HasValue)
                return true;
            await WriteString("Valid Tribe Required", "text/plain", 400);
            return false;
        }

        /// <summary>
        /// Requires that we are an admin of the server
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RequireServerAdmin()
        {
            if (server.CheckIsUserAdmin(user))
                return true;
            await WriteString("Admin Permissions Required", "text/plain", 401);
            return false;
        }
    }
}
