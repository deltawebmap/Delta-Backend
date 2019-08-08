using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.NetEntities.Library
{
    public class ArkLibraryDino
    {
        public string species_name;
        public string species_img;

        public bool is_female;
        public string tamed_name;

        public ArkLibraryDinoStat health;
        public ArkLibraryDinoStat stamina;
        public ArkLibraryDinoStat oxygen;
        public ArkLibraryDinoStat food;
        public ArkLibraryDinoStat weight;
        public ArkLibraryDinoStat damage;
        public ArkLibraryDinoStat speed;

        public float tamingEffectiveness;
        public float imprintingBonus;
        public bool is_bred;

        public int level_total;
        public int level_wild;
        public int level_tamed;

        public string[] colors;

        public string id;

        public string more_url;
        public string export_url;
    }
}
