using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class EditServerListingPayload
    {
        //Either of these values could be null. They'll just be ignored
        public string name;
        public string iconToken;
    }
}
