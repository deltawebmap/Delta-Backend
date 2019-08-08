using ArkHttpServer.NetEntities.Library;
using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes;
using ArkWebMapLightspeedClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.HttpServices
{
    public static class LibraryService
    {
        public static async Task OnHttpRequest(LightspeedRequest e, ArkWorld world, int tribeId)
        {
            //Convert all dinos
            List<ArkLibraryDino> dinos = GetDinos(world, tribeId);

            //Encode
            ArkLibrary l = new ArkLibrary
            {
                dinos = dinos
            };

            //Write
            await e.DoRespondJson(l);
        }

        private static List<ArkLibraryDino> GetDinos(ArkWorld w, int tribeId)
        {
            List<ArkLibraryDino> lib = new List<ArkLibraryDino>();
            ArkDinosaurStats statMin = MakeDefaultStats(float.MaxValue);
            ArkDinosaurStats statMax = MakeDefaultStats(float.MinValue);

            //Find min/max stats
            foreach (var d in w.dinos)
            {
                //Check if this is in the correct tribe
                if (!d.isInTribe || d.tribeId != tribeId)
                    continue;

                //Update max
                statMax.health = MathF.Max(statMax.health, d.baseLevelupsApplied.health + d.tamedLevelupsApplied.health);
                statMax.stamina = MathF.Max(statMax.stamina, d.baseLevelupsApplied.stamina + d.tamedLevelupsApplied.stamina);
                statMax.food = MathF.Max(statMax.food, d.baseLevelupsApplied.food + d.tamedLevelupsApplied.food);
                statMax.oxygen = MathF.Max(statMax.oxygen, d.baseLevelupsApplied.oxygen + d.tamedLevelupsApplied.oxygen);
                statMax.inventoryWeight = MathF.Max(statMax.inventoryWeight, d.baseLevelupsApplied.inventoryWeight + d.tamedLevelupsApplied.inventoryWeight);
                statMax.movementSpeedMult = MathF.Max(statMax.movementSpeedMult, d.baseLevelupsApplied.movementSpeedMult + d.tamedLevelupsApplied.movementSpeedMult);
                statMax.meleeDamageMult = MathF.Max(statMax.meleeDamageMult, d.baseLevelupsApplied.meleeDamageMult + d.tamedLevelupsApplied.meleeDamageMult);

                //Update min
                statMin.health = MathF.Min(statMin.health, d.baseLevelupsApplied.health + d.tamedLevelupsApplied.health);
                statMin.stamina = MathF.Min(statMin.stamina, d.baseLevelupsApplied.stamina + d.tamedLevelupsApplied.stamina);
                statMin.food = MathF.Min(statMin.food, d.baseLevelupsApplied.food + d.tamedLevelupsApplied.food);
                statMin.oxygen = MathF.Min(statMin.oxygen, d.baseLevelupsApplied.oxygen + d.tamedLevelupsApplied.oxygen);
                statMin.inventoryWeight = MathF.Min(statMin.inventoryWeight, d.baseLevelupsApplied.inventoryWeight + d.tamedLevelupsApplied.inventoryWeight);
                statMin.movementSpeedMult = MathF.Min(statMin.movementSpeedMult, d.baseLevelupsApplied.movementSpeedMult + d.tamedLevelupsApplied.movementSpeedMult);
                statMin.meleeDamageMult = MathF.Min(statMin.meleeDamageMult, d.baseLevelupsApplied.meleeDamageMult + d.tamedLevelupsApplied.meleeDamageMult);
            }

            //Convert
            foreach (var d in w.dinos)
            {
                //Check if this is in the correct tribe
                if (!d.isInTribe || d.tribeId != tribeId)
                    continue;

                //Skip if this has no entry
                if (d.dino_entry == null)
                    continue;

                //Get max stats
                ArkDinosaurStats max = d.GetMaxStats();

                //Start conversion
                ArkLibraryDino ld = new ArkLibraryDino
                {
                    species_img = d.dino_entry.icon.image_thumb_url,
                    species_name = d.dino_entry.screen_name,
                    is_female = d.isFemale,
                    tamed_name = d.tamedName,
                    tamingEffectiveness = -1,
                    imprintingBonus = d.imprintQuality,
                    is_bred = d.isBaby,
                    level_total = d.level,
                    level_tamed = d.level - d.baseLevel,
                    level_wild = d.baseLevel,
                    colors = d.colors_hex,
                    id = d.dinosaurId.ToString(),
                    more_url = $"{ArkWebServer.api_prefix}/world/dinos/{d.dinosaurId.ToString()}",
                    export_url = $"{ArkWebServer.api_prefix}/world/dinos/{d.dinosaurId.ToString()}?export=true",

                    health = GetDinoStat(max.health, d.baseLevelupsApplied.health, d.tamedLevelupsApplied.health, statMax.health, statMin.health),
                    stamina = GetDinoStat(max.stamina, d.baseLevelupsApplied.stamina, d.tamedLevelupsApplied.stamina, statMax.stamina, statMin.stamina),
                    food = GetDinoStat(max.food, d.baseLevelupsApplied.food, d.tamedLevelupsApplied.food, statMax.food, statMin.food),
                    oxygen = GetDinoStat(max.oxygen, d.baseLevelupsApplied.oxygen, d.tamedLevelupsApplied.oxygen, statMax.oxygen, statMin.oxygen),
                    weight = GetDinoStat(max.inventoryWeight, d.baseLevelupsApplied.inventoryWeight, d.tamedLevelupsApplied.inventoryWeight, statMax.inventoryWeight, statMin.inventoryWeight),
                    speed = GetDinoStat(max.movementSpeedMult, d.baseLevelupsApplied.movementSpeedMult, d.tamedLevelupsApplied.movementSpeedMult, statMax.movementSpeedMult, statMin.movementSpeedMult),
                    damage = GetDinoStat(max.meleeDamageMult, d.baseLevelupsApplied.meleeDamageMult, d.tamedLevelupsApplied.meleeDamageMult, statMax.meleeDamageMult, statMin.meleeDamageMult),
                };

                lib.Add(ld);
            }
            return lib;
        }

        private static ArkDinosaurStats MakeDefaultStats(float defaultValue)
        {
            return new ArkDinosaurStats
            {
                food = defaultValue,
                health = defaultValue,
                inventoryWeight = defaultValue,
                meleeDamageMult = defaultValue,
                movementSpeedMult = defaultValue,
                oxygen = defaultValue,
                stamina = defaultValue,
                unknown1 = defaultValue,
                water = defaultValue
            };
        }

        private static ArkLibraryDinoStat GetDinoStat(float value, float appliedBase, float appliedTamed, float statMax, float statMin)
        {
            //Find where this value fits in the range
            float r = (appliedBase + appliedTamed - statMin) / (statMax - statMin);

            //Produce output
            return new ArkLibraryDinoStat
            {
                levelups_tamed = appliedTamed,
                levelups_wild = appliedBase,
                score = r,
                value = value
            };
        }
    }
}
