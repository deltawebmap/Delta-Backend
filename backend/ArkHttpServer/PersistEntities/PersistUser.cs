using ArkHttpServer.Entities;
using ArkWebMapLightspeedClient.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.PersistEntities
{
    public class PersistUser
    {
        /// <summary>
        /// Ark Web Map Master internal ID
        /// </summary>
        public string awm_id { get; set; }

        /// <summary>
        /// Steam ID
        /// </summary>
        public string steam_id { get; set; }

        /// <summary>
        /// The name of this person from Steam
        /// </summary>
        public string steam_name { get; set; }

        /// <summary>
        /// The icon of this user from Steam
        /// </summary>
        public string steam_icon { get; set; }

        public static PersistUser ConvertUser(MasterServerArkUser u)
        {
            return new PersistUser
            {
                awm_id = u.id,
                steam_icon = u.profile_image_url,
                steam_id = u.steam_id,
                steam_name = u.screen_name
            };
        }
    }
}
