using ArkSaveEditor.Entities;
using ArkWebMapLightspeedClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Tools
{
    public static class TileDataTool
    {
        public static TileData GetTileData(LightspeedRequest e, ArkMapData map)
        {
            //Get the x, y, and z from the URL
            float x = float.Parse(e.query["x"]);
            float y = float.Parse(e.query["y"]);
            float z = float.Parse(e.query["z"]);
            TileData d = new TileData
            {
                tile_x = x,
                tile_y = y,
                tile_z = z
            };

            //Get units per tile
            d.tiles_per_axis = MathF.Pow(2, z);
            d.units_per_tile = map.captureSize / d.tiles_per_axis;

            //Calculate game pos
            CalculateZCoordsToGameUnits(map, d.units_per_tile, x, y, out d.game_min_x, out d.game_min_y);
            CalculateZCoordsToGameUnits(map, d.units_per_tile, x+1, y+1, out d.game_max_x, out d.game_max_y);

            return d;
        }

        private static void CalculateZCoordsToGameUnits(ArkMapData map, float units_per_tile, float x, float y, out float gx, out float gy)
        {
            float offset = map.captureSize / 2; //Because this is based in the upper left, while the game is based in the middle
            gx = (x * units_per_tile) - offset;
            gy = (y * units_per_tile) - offset;
        }
    }

    public class TileData
    {
        public float tile_x;
        public float tile_y;
        public float tile_z;

        public float units_per_tile;
        public float tiles_per_axis;

        public float game_min_x;
        public float game_min_y;

        public float game_max_x;
        public float game_max_y;
    }
}
