using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapDynamicTiles.Entities
{
    public class PrimalDataImagesMetadata
    {
        public float version_minor;
        public float version_major;

        public Dictionary<string, Dictionary<string, string>> data;
    }
}
