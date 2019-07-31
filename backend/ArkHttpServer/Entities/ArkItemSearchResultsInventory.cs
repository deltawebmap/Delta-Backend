using ArkSaveEditor.World.WorldTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class ArkItemSearchResultsInventory
    {
        public ArkItemSearchResultsInventoryType type;
        public int count;
        public string id;

        [JsonIgnore]
        public ArkCharacter character;
    }

    public enum ArkItemSearchResultsInventoryType
    {
        Dino = 0,
        Player = 1
    }
}
