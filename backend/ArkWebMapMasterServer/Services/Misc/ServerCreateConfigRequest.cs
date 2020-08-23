using LibDeltaSystem;
using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public class ServerCreateConfigRequest : BasicDeltaService
    {
        public ServerCreateConfigRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Get dino icon templates
            List<ResponseData_DinoIcons> icons = new List<ResponseData_DinoIcons>();
            await GetDinoIconTemplates(icons, "left", new string[]
            {
                "Baryonyx_Character_BP",
                "BionicPara_Character_BP",
                "BionicGigant_Character_BP",
                "Bigfoot_Character_BP",
                "Compy_Character_BP_Child_GNS",
                "Deinonychus_Character_BP",
                "Enforcer_Character_BP",
                "Griffin_Character_BP",
                "Procoptodon_Character_BP",
                "Therizino_Character_BP",
                "Spino_Character_BP"
            });
            await GetDinoIconTemplates(icons, "right", new string[]
            {
                "Allo_Character_BP",
                "Argent_Character_BP",
                "Raptor_Character_BP",
                "Basilisk_Character_BP",
                "Rex_Character_BP",
                "BionicQuetz_Character_BP",
                "BionicStego_Character_BP",
                "CaveWolf_Character_BP",
                "CrystalWyvern_Character_BP_Base",
                "Equus_Character_BP",
                "Galli_Character_BP",
                "IceKaiju_Character_BP",
                "Kapro_Character_BP_Race",
                "Microraptor_Character_BP",
                "Pachy_Character_BP",
                "Phoenix_Character_BP",
                "Ptero_Character_BP",
                "Sarco_Character_BP",
                "Snow_Rhino_Character_BP",
                "Yutyrannus_Character_BP"
            });

            //Get maps
            var maps = await conn.GetARKMaps();
            List<string> mapNames = new List<string>();
            foreach (var m in maps)
            {
                if(m.Key != "DEFAULT")
                    mapNames.Add(m.Value.displayName);
            }

            //Load static config
            ResponseData_Config config = conn.GetUserConfigDefault<ResponseData_Config>("master_guildconfig.json", new ResponseData_Config());

            //Write
            await WriteJSON(new ResponseData
            {
                config = config,
                dino_icon_templates = icons,
                supported_maps = mapNames
            });
        }

        private async Task GetDinoIconTemplates(List<ResponseData_DinoIcons> output, string align, string[] searchClassnames)
        {
            //Search
            var filterBuilder = Builders<DbArkEntry<DinosaurEntry>>.Filter;
            var results = await conn.arkentries_dinos.FindAsync(filterBuilder.In("data.classname", searchClassnames));
            var resultList = await results.ToListAsync();

            //Convert
            foreach(var r in resultList)
            {
                output.Add(new ResponseData_DinoIcons
                {
                    icon_align = align,
                    icon_url_full = r.data.icon.image_url,
                    icon_url_thumb = r.data.icon.image_thumb_url
                });
            }
        }

        class ResponseData
        {
            public List<ResponseData_DinoIcons> dino_icon_templates;
            public ResponseData_Config config;
            public List<string> supported_maps;
        }

        class ResponseData_DinoIcons
        {
            public string icon_url_thumb;
            public string icon_url_full;
            public string icon_align;
        }

        class ResponseData_Config
        {
            public string steam_mod_id;
            public ResponseData_Config_Host[] supported_hosts;
        }

        class ResponseData_Config_Host
        {
            public string id;
            public string display_name;
            public string type;
            public string tutorial_video_id;
        }
    }
}
