using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class IAuthMethod
    {
        /// <summary>
        /// The user name or Google ID, ect. This is what this will be found with. Everything else will be inhereted.
        /// </summary>
        public string uid { get; set; }
    }
}
