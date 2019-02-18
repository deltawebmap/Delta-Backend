using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    /// <summary>
    /// Returns the Ark server in a reply safe to show anyone.
    /// </summary>
    public class ArkServerReply
    {
        public string display_name;
        public string image_url;
        public string owner_uid;
        public string id;
        public bool online;
        public double ping;

        public ArkServerReply(ArkServer s, ArkUser u, int timeoutMs = 2000)
        {
            //Set the simple stuff
            display_name = s.display_name;
            image_url = s.image_url;
            owner_uid = s.owner_uid;
            id = s._id;

            //Ping
            try
            {
                DateTime start = DateTime.UtcNow;
                s.SendRequest<PingRequest>("/bridge/ping", new PingRequest
                {
                    content = "user ping"
                }, RequestHttpMethod.get, u, timeoutMs);
                ping = (DateTime.UtcNow - start).TotalMilliseconds;
                online = true;
            } catch
            {
                ping = -1;
                online = false;
            }
        }
    }
}
