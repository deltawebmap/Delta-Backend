using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class ArkServerInvite
    {
        public string inviter_uid { get; set; } //User ID of the person who invited this person
        public string server_uid { get; set; } //The ID of the server this invite is for

        public bool is_tribe { get; set; } //Invites can also be for tribes. If this is true, the following value is also set
        public string tribe_uid { get; set; } //The tribe ID that this person has been invited into.

        public long creation_time { get; set; } //Time this invite was created
        public long expiration_time { get; set; } //Time this invite exipres

        public int uses { get; set; }

        [JsonProperty("id")]
        public string _id { get; set; } //ID of the invite. Could also be called a token, as it's what you will have in the URL.

        public void Update()
        {
            ArkWebMapMasterServer.Servers.ArkServerInviteManager.GetCollection().Update(this);
        }

        public ArkServer GetServer()
        {
            return ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetSlaveServerById(server_uid);
        }

        public string GetUrl()
        {
            return $"https://ark.romanport.com/invites/{_id}";
        }
    }
}
