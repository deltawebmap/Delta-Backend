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
        public Dictionary<string, BasicArkDino> dinos;

        public List<string> baby_dino_urls;

        public string[] diff_dinos_missing; //Dinos missing since last request
        public string[] diff_dinos_added; //New dinos
        public string[] diff_dinos_unchanged;
        public string[] dino_ids;

        public BasicTribe(ArkWorld world, string tribeName, string sessionid, List<string> last_dino_list)
        {
            gameTime = world.gameTime;

            //Narrow down our search by tribe name, if needed.
            ArkDinosaur[] searchDinos = world.dinos.Where(x => x.isTamed == true && x.tamerName == tribeName).ToArray();
            dinos = new Dictionary<string, BasicArkDino>();
            for (int i = 0; i < searchDinos.Length; i++)
            {
                if(dinos.ContainsKey(searchDinos[i].dinosaurId.ToString()))
                {
                    //Uh oh. This could be an issue. Check if it matches this dino
                    var d = dinos[searchDinos[i].dinosaurId.ToString()];
                    if (d.tamedName != searchDinos[i].tamedName)
                        throw new Exception("Dino exists with the same ID.");
                } else
                {
                    dinos.Add(searchDinos[i].dinosaurId.ToString(), new BasicArkDino(searchDinos[i], world, sessionid));
                }
                
            }

            //Diff dino list
            //Convert to new dino list
            List<string> new_dino_list = new List<string>();
            foreach (var d in dinos.Keys)
                new_dino_list.Add(d.ToString());

            //Find new and find missing
            dino_ids = new_dino_list.ToArray();
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
            }

            //Get baby dinos
            baby_dino_urls = new List<string>();
            foreach(var d in world.dinos)
            {
                if(d.isBaby == true && d.babyAge < 1f && d.tamerName == tribeName)
                {
                    baby_dino_urls.Add(new BasicArkDino(d, world, sessionid).apiUrl);
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

        public BasicArkDino(ArkDinosaur dino, ArkWorld w, string sessionId)
        {
            //Convert this dino to this.
            pos = dino.location;
            coord_pos = w.ConvertFromWorldToGameCoords(dino.location);
            normalized_pos = w.ConvertFromWorldToNormalizedPos(dino.location);

            classname = dino.classnameString;
            imgUrl = $"{Program.config.resources_url}/dinos/icons/lq/{classname}.png";
            id = dino.dinosaurId;
            apiUrl = $"{Program.config.api_url}/world/{sessionId}/dinos/{id}";
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

            //Testing
            /*Random rand = new Random();
            switch(rand.Next(0, 5))
            {
                case 0:
                    adjusted_map_pos = new Vector2(0, 0);
                    break;
                case 1:
                    adjusted_map_pos = new Vector2(1, 1);
                    break;
                case 2:
                    adjusted_map_pos = new Vector2(0, 1);
                    break;
                case 3:
                    adjusted_map_pos = new Vector2(1, 0);
                    break;
                case 4:
                    adjusted_map_pos = new Vector2(0.5f, 0.5f);
                    break;
            }*/

            adjusted_map_pos = w.mapinfo.transformOffsets.Apply(adjusted_map_pos);
            entry = ArkSaveEditor.ArkImports.GetDinoDataByClassname(classname);
        }
    }
}
