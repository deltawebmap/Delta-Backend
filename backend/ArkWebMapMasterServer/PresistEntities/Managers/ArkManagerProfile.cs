using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities.Managers
{
    public class ArkManagerProfile
    {
        /// <summary>
        /// The image for this hosting provider.
        /// </summary>
        public string wide_image_url { get; set; }

        /// <summary>
        /// The name of this provider
        /// </summary>
        public string name { get; set; }
    }
}
