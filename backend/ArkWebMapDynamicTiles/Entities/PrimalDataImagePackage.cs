using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapDynamicTiles.Entities
{
    public class PrimalDataImagePackage
    {
        public Dictionary<string, Dictionary<string, Image<Rgba32>>> images;
    }
}
