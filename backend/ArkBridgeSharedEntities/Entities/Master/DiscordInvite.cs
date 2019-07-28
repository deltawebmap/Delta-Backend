using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities.Master
{
    public class Guild
    {
        public string splash { get; set; }
        public List<string> features { get; set; }
        public string name { get; set; }
        public int verification_level { get; set; }
        public string icon { get; set; }
        public string banner { get; set; }
        public string id { get; set; }
    }

    public class Inviter
    {
        public string username { get; set; }
        public string discriminator { get; set; }
        public string id { get; set; }
        public string avatar { get; set; }
    }

    public class Channel
    {
        public int type { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class DiscordInvite
    {
        public string code { get; set; }
        public Guild guild { get; set; }
        public int approximate_member_count { get; set; }
        public int approximate_presence_count { get; set; }
        public Inviter inviter { get; set; }
        public Channel channel { get; set; }
    }
}
