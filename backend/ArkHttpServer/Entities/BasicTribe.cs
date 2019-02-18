using ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties;
using ArkSaveEditor.World;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ArkSaveEditor.World.WorldTypes;
using ArkSaveEditor.Entities.LowLevel.DotArk;
using ArkSaveEditor.Entities;
using ArkSaveEditor.ArkEntries;

namespace ArkHttpServer.Entities
{
    /// <summary>
    /// A basic world that won't take up too much bandwidth to send over http.
    /// </summary>
    public class BasicTribe
    {
        public float gameTime;
        public List<MinifiedBasicArkDino> dinos;
        public int tribeId;

        public List<string> baby_dino_urls;
        public List<ArkDinoReply> baby_dinos;

        public string[] dino_ids;

        public BasicTribe(ArkWorld world, int tribeId)
        {
            gameTime = world.gameTime;
            this.tribeId = tribeId;

            //Narrow down our search by tribe name, if needed.
            ArkDinosaur[] searchDinos = world.dinos.Where(x => x.isTamed == true && x.tribeId == tribeId).ToArray();
            dinos = new List<MinifiedBasicArkDino>();
            for (int i = 0; i < searchDinos.Length; i++)
            {
                dinos.Add(new MinifiedBasicArkDino(searchDinos[i], world));

            }

            //Find new and find missing
            /*dino_ids = new_dino_list.ToArray();
           if(last_dino_list != null)
            {
                diff_dinos_missing = last_dino_list.Except(new_dino_list).ToArray();
                diff_dinos_added = new_dino_list.Except(last_dino_list).ToArray();
                diff_dinos_unchanged = new_dino_list.Intersect(last_dino_list).ToArray();
            } else
            {
                //Set diff dinos added to them all
                diff_dinos_added = new_dino_list.ToArray();
                diff_dinos_unchanged = new_dino_list.ToArray();
            }*/

            //Get baby dinos
            baby_dino_urls = new List<string>();
            baby_dinos = new List<ArkDinoReply>();
            foreach(var d in searchDinos)
            {
                if(d.isBaby == true && d.babyAge < 1f)
                {
                    baby_dino_urls.Add(new BasicArkDino(d, world).apiUrl);
                    baby_dinos.Add(new ArkDinoReply(d, world)); //Full dino
                }
            }
        }
    }

    public class BasicArkDino
    {
        public DotArkLocationData pos;

        public Vector2 coord_pos;
        public Vector2 normalized_pos; //0 if on left, 1 if right, ect
        public Vector2 adjusted_map_pos; //Position for the web map

        public int[] colors;
        public string classname;
        public string imgUrl;
        public string apiUrl;
        public ulong id;
        public bool isFemale;
        public string tamedName;
        public string tamerName;
        public bool isTamed;

        public ArkDinoEntry entry;

        public BasicArkDino(ArkDinosaur dino, ArkWorld w)
        {
            //Convert this dino to this.
            pos = dino.location;
            coord_pos = w.ConvertFromWorldToGameCoords(dino.location);
            normalized_pos = w.ConvertFromWorldToNormalizedPos(dino.location);

            classname = dino.classnameString;
            imgUrl = $"{ArkWebServer.config.resources_url}/dinos/icons/lq/{classname}.png";
            id = dino.dinosaurId;
            apiUrl = $"{ArkWebServer.api_prefix}/world/dinos/{id}";
            isFemale = dino.isFemale;
            tamedName = dino.tamedName;
            tamerName = dino.tamerName;
            isTamed = dino.isTamed;

            //Convert the colors to an integer so it is serialized correctly.
            colors = new int[dino.colors.Length];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = dino.colors[i];

            //Create the adjusted map pos
            adjusted_map_pos = normalized_pos.Clone();

            adjusted_map_pos = w.mapinfo.transformOffsets.Apply(adjusted_map_pos);
            entry = ArkSaveEditor.ArkImports.GetDinoDataByClassname(classname);
        }
    }

    /// <summary>
    /// To be used ONLY by the /tribes endpoint.
    /// </summary>
    public class MinifiedBasicArkDino
    {
        public Vector2 coord_pos;
        public Vector2 adjusted_map_pos; //Position for the web map

        public string classname;
        public string imgUrl;
        public string apiUrl;
        public ulong id;
        public string tamedName;

        public MinifiedBasicArkDino(ArkDinosaur dino, ArkWorld w)
        {
            //Convert this dino to this.
            coord_pos = w.ConvertFromWorldToGameCoords(dino.location);
            var normalized_pos = w.ConvertFromWorldToNormalizedPos(dino.location);

            classname = dino.classnameString;
            imgUrl = $"{ArkWebServer.config.resources_url}/dinos/icons/lq/{classname}.png";
            id = dino.dinosaurId;
            apiUrl = $"{ArkWebServer.api_prefix}/world/dinos/{id}";
            tamedName = dino.tamedName;


            //Create the adjusted map pos
            adjusted_map_pos = normalized_pos.Clone();

            adjusted_map_pos = w.mapinfo.transformOffsets.Apply(adjusted_map_pos);
        }
    }
}
