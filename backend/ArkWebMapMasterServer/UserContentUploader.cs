using ArkWebMapMasterServer.NetEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ArkWebMapMasterServer
{
    public class UserContentUploader
    {
        public static UserContentTokenPayload FinishContentUpload(string token)
        {
            try
            {
                UserContentTokenPayload tokenPayload;
                using (WebClient wc = new WebClient())
                {
                    byte[] d = wc.DownloadData("https://user-content.romanport.com/upload_token?token=" + System.Web.HttpUtility.UrlEncode(token));
                    tokenPayload = JsonConvert.DeserializeObject<UserContentTokenPayload>(Encoding.UTF8.GetString(d));
                }
                return tokenPayload;
            } catch
            {
                return null;
            }
        }
    }
}
