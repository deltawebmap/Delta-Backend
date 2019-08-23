using ArkWebMapAnalytics.NetEntities;
using ArkWebMapAnalytics.PersistEntities;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapAnalytics.Services
{
    public static class DestroyService
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Authenticate
            string token = e.Request.Query["access_token"];
            string uid = UserAuthenticator.GetUserIdFromToken(token);

            //Check if failed
            if (uid == null)
                throw new StandardError("Must be authenticated.", 401);

            //Check if POST
            if (e.Request.Method.ToUpper() != "POST")
                throw new StandardError("Must be a POST request.", 400);

            //Find all
            var collec = Program.db.GetCollection<ActionEntry>("actions");
            var data = collec.Find(x => x.user_id == uid).ToArray();
            foreach (var d in data)
                collec.Delete(d._id);

            //Return OK
            await Program.QuickWriteToDoc(e, "OK", "text/plain");
        }
    }
}
