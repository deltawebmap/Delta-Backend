using ArkHttpServer.Entities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Bridge
{
    public static class MirrorReportService
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s)
        {
            //Decode post body
            ArkWebMapMirrorTokens data = Program.DecodePostBody<ArkWebMapMirrorTokens>(e);

            //Create output
            ArkMirrorToken output;
            if(data == null)
            {
                output = new ArkMirrorToken
                {
                    _id = s._id,
                    time = DateTime.UtcNow.Ticks,
                    token = null,
                    hasToken = false,
                    dinoTokenMap = null
                };
            } else
            {
                output = new ArkMirrorToken
                {
                    dinoTokenMap = data.dinoTokenMap,
                    hasToken = true,
                    token = data.token,
                    time = DateTime.UtcNow.Ticks,
                    _id = s._id
                };
            }

            //Insert or replace
            var collection = MirrorTokenTool.GetCollection();
            if (collection.FindOne(x => x._id == s._id) == null)
                collection.Insert(output);
            else
                collection.Update(output);

            //Return ok
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
