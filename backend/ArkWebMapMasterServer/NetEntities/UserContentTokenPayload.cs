using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class UserContentTokenPayload
    {
        public string _id { get; set; } //The ID of the image on the disk

        public string mimeType { get; set; }
        public string applicationId { get; set; }
        public long uploadTime { get; set; }
        public string filename { get; set; }
        public string deletionToken { get; set; }
        public string url { get; set; }
    }
}
