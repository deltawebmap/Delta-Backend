using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities.Managers
{
    /// <summary>
    /// Managers are usually owned my hosting provider companies. They have control over a server's hardware, but also have a client that rents from them.
    /// </summary>
    public class ArkManager
    {
        /// <summary>
        /// The ID of this user
        /// </summary>
        public string _id { get; set; }

        /// <summary>
        /// Holds data such as image and name
        /// </summary>
        public ArkManagerProfile profile { get; set; }

        /// <summary>
        /// E-Mail of this provider. Used for authentication
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// Hashed password
        /// </summary>
        public byte[] password { get; set; }

        /// <summary>
        /// The salt for verifying a password
        /// </summary>
        public byte[] password_salt { get; set; }

        /// <summary>
        /// The date this manager was created.
        /// </summary>
        public long creationDate { get; set; }

        /// <summary>
        /// A shared API token that will never expire (unless requested)
        /// </summary>
        public string api_token { get; set; }
    }
}
