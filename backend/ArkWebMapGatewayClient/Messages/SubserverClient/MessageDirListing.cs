using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages.SubserverClient
{
    public class MessageDirListing : GatewayMessageBase
    {
        public string pathname;
        public string token;
        public string callback_url;
    }
}
