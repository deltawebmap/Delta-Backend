using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities.Managers
{
    public class ArkManagerToken
    {
        /// <summary>
        /// The actual token text
        /// </summary>
        public string _id { get; set; }

        /// <summary>
        /// The linked manager ID
        /// </summary>
        public string managerId { get; set; }

        /// <summary>
        /// The time this token expires. Usually a few hours after it is created.
        /// </summary>
        public long expiryDate { get; set; }
    }
}
