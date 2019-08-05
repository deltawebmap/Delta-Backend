using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArkBridgeSharedEntities.Entities.Master;
using ArkSaveEditor.Entities;
using ArkSaveEditor.World.WorldTypes;
using ArkWebMapDynamicTiles.Entities;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using ArkSaveEditor.ArkEntries;

namespace ArkWebMapDynamicTiles.MapSessions
{
    public class StructureMapSession : MapSession
    {
        public ArkMapData mapInfo;
        public List<ArkStructure> structures;

        public override async Task OnCreate(HttpContext e, UsersMeReply_Server server, ContentMetadata commit)
        {
            //Get data
            mapInfo = commit.GetContent<ArkMapData>("map");
            structures = commit.GetContent<List<ArkStructure>>("structures");
        }

        public override async Task OnHttpRequest(HttpContext e, float x, float y, float z)
        {
            //Get tile info
            TileData tile = TileDataTool.GetTileData(x, y, z, mapInfo);

            //Compute and create the tile
            Image<Rgba32> image = await Compute(tile, mapInfo, structures, tribe_id);

            //Write
            e.Response.ContentType = "image/png";
            image.SaveAsPng(e.Response.Body, new SixLabors.ImageSharp.Formats.Png.PngEncoder
            {
                CompressionLevel = 9
            });
        }

        public override int GetMinDataVersion()
        {
            return 2;
        }

        public const bool DEBUG_SYMBOLS = false;

        private static async Task<Image<Rgba32>> Compute(TileData tile, ArkMapData mapinfo, List<ArkStructure> structures, int tribeId)
        {
            //Get range
            float tilePpm = 256 / tile.units_per_tile;

            //Create image
            Image<Rgba32> output = new Image<Rgba32>(256, 256);

            //Find all tiles in range
            List<QueuedTile> tilesInRange = new List<QueuedTile>();
            foreach (var t in structures)
            {
                //Check if the tribe matches
                if (!t.isInTribe || t.tribeId != tribeId)
                    continue;

                //Check if the image exists
                if (!Program.image_package.images["structure"].ContainsKey(t.displayMetadata.img + ".png"))
                    continue;

                //Get image and it's width and height in game units
                Image<Rgba32> img = Program.image_package.images["structure"][t.displayMetadata.img + ".png"];
                float ppm = t.displayMetadata.capturePixels / t.displayMetadata.captureSize;
                float img_game_width = img.Width * ppm;
                float img_game_height = img.Height * ppm;

                //Check if it is in range
                if (t.location.x > tile.game_max_x + (img_game_width) || t.location.x < tile.game_min_x - (img_game_width))
                    continue;
                if (t.location.y > tile.game_max_y + (img_game_height) || t.location.y < tile.game_min_y - (img_game_height))
                    continue;

                //Determine size
                float scaleFactor = tilePpm / ppm;
                int img_scale_x = (int)(img.Width * scaleFactor);
                int img_scale_y = (int)(img.Height * scaleFactor);

                //Check if this'll even be big enough to see
                if (img_scale_x < 5 || img_scale_y < 5)
                    continue;

                //Apply transformations
                Image<Rgba32> simg = img.Clone();
                simg.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new SixLabors.Primitives.Size(img_scale_x, img_scale_y),
                    Position = AnchorPositionMode.Center
                }).Rotate(t.location.yaw));

                //Determine location
                float loc_tile_x = (t.location.x - (img_game_width / 2) - tile.game_min_x) / tile.units_per_tile; //Top left of the image inside the tile
                float loc_tile_y = (t.location.y - (img_game_height / 2) - tile.game_min_y) / tile.units_per_tile; //Top left of the image inside the tile
                int copy_offset_x = (int)(loc_tile_x * 256);
                int copy_offset_y = (int)(loc_tile_y * 256);

                //Queue
                tilesInRange.Add(new QueuedTile
                {
                    img = simg,
                    copy_offset_x = copy_offset_x,
                    copy_offset_y = copy_offset_y,
                    z = t.location.z,
                    display_type = t.displayMetadata.displayType
                });
            }

            //Sort by Y level
            tilesInRange.Sort(new Comparison<QueuedTile>((x, y) =>
                x.z.CompareTo(y.z)
            ));
            tilesInRange.Sort(new Comparison<QueuedTile>((x, y) =>
                x.display_type.CompareTo(y.display_type)
            ));

            //Now, copy the images
            foreach (var q in tilesInRange)
            {
                //Copy
                for (int x = 0; x < q.img.Width; x++)
                {
                    for (int y = 0; y < q.img.Height; y++)
                    {
                        //Check if this is just alpha
                        if (q.img[x, y].A < 10)
                            continue;

                        //Check if in range
                        if (x + q.copy_offset_x < 0 || y + q.copy_offset_y < 0)
                            continue;
                        if (x + q.copy_offset_x >= output.Width || y + q.copy_offset_y >= output.Height)
                            continue;

                        //Copy
                        output[x + q.copy_offset_x, y + q.copy_offset_y] = q.img[x, y];
                    }
                }
            }

            //Add debug items
            if (DEBUG_SYMBOLS)
            {
                for (int x = 0; x < output.Width; x++)
                {
                    for (int y = 0; y < output.Height; y++)
                    {
                        if (y <= 1 || x <= 1 || y >= output.Height - 2 || x >= output.Width - 2)
                            output[x, y] = new Rgba32(255, 0, 0, 255);
                    }
                }
            }

            return output;
        }

        class QueuedTile
        {
            public int copy_offset_x;
            public int copy_offset_y;
            public float z;
            public Image<Rgba32> img;
            public StructureDisplayMetadata_DisplayType display_type;
        }
    }
}
