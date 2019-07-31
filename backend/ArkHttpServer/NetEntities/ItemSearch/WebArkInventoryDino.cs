using ArkSaveEditor.ArkEntries;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.NetEntities.ItemSearch
{
    public class WebArkInventoryDino : WebArkInventoryHolder
    {
        public string id;
        public string displayName;
        public string displayClassName;
        public string img;
        public int level;

        public static WebArkInventoryDino Convert(ArkDinosaur dino)
        {
            if(dino.dino_entry == null)
            {
                return new WebArkInventoryDino
                {
                    displayClassName = dino.classnameString,
                    displayName = dino.tamedName,
                    id = dino.dinosaurId.ToString(),
                    img = "https://icon-assets.deltamap.net/legacy/img_failed.png",
                    level = dino.level
                };
            } else
            {
                return new WebArkInventoryDino
                {
                    displayClassName = dino.dino_entry.screen_name,
                    displayName = dino.tamedName,
                    id = dino.dinosaurId.ToString(),
                    img = dino.dino_entry.icon.image_thumb_url,
                    level = dino.level
                };
            }
        }
    }
}
