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
using ArkBridgeSharedEntities.Entities;

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

        public List<MinifiedBasicArkPlayerCharacter> player_characters;

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

            //Get tribemembers
            var tribeMembers = world.playerCharacters.Where(x => x.tribeId == tribeId);
            List<string> idsToDownload = new List<string>();
            player_characters = new List<MinifiedBasicArkPlayerCharacter>();
            foreach (var t in tribeMembers)
                player_characters.Add(new MinifiedBasicArkPlayerCharacter(t, world, ref idsToDownload));

            //Mass request all the steam IDs for this
            List<SteamProfile> tribeProfiles = Tools.SteamIdLookup.MassFetchPlayerData(idsToDownload).GetAwaiter().GetResult();
            foreach(var p in tribeProfiles)
            {
                foreach(var t in player_characters)
                {
                    if (t.profile.steamPlayerId == p.steamid)
                        t.steamProfile = p;
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

            adjusted_map_pos = w.mapinfo.ConvertFromGamePositionToNormalized(new Vector2(dino.location.x, dino.location.y));
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
        public string id;
        public string tamedName;
        public string displayClassname;
        public int level;

        public MinifiedBasicArkDino(ArkDinosaur dino, ArkWorld w)
        {
            //Convert this dino to this.
            coord_pos = w.ConvertFromWorldToGameCoords(dino.location);

            classname = dino.classnameString;
            imgUrl = $"{ArkWebServer.config.resources_url}/dinos/icons/lq/{classname}.png";
            id = dino.dinosaurId.ToString();
            apiUrl = $"{ArkWebServer.api_prefix}/world/dinos/{id}";
            tamedName = dino.tamedName;
            displayClassname = dino.classnameString;
            if (dino.dino_entry != null)
                displayClassname = dino.dino_entry.screen_name;
            level = dino.level;


            //Create the adjusted map pos
            adjusted_map_pos = w.mapinfo.ConvertFromGamePositionToNormalized(new Vector2(dino.location.x, dino.location.y));
        }
    }

    public class MinifiedBasicArkPlayerCharacter
    {
        public Vector2 coord_pos;
        public Vector2 adjusted_map_pos; //Position for the web map

        public ArkPlayerProfile profile;
        public SteamProfile steamProfile; //This will be set outside of our constructor
        public bool is_alive;

        public MinifiedBasicArkPlayerCharacter(ArkPlayer player, ArkWorld w, ref List<string> idsToFetch)
        {
            //Get pos
            coord_pos = w.ConvertFromWorldToGameCoords(player.location);
            adjusted_map_pos = w.mapinfo.ConvertFromGamePositionToNormalized(new Vector2(player.location.x, player.location.y));

            //Add to Steam request queue
            if (!idsToFetch.Contains(player.steamId))
                idsToFetch.Add(player.steamId);

            //Copy
            profile = player.GetPlayerProfile();
            is_alive = player.isAlive;
        }
    }
}
