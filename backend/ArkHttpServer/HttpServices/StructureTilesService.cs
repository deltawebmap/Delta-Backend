using ArkHttpServer.Tools;
using ArkSaveEditor.ArkEntries;
using ArkSaveEditor.Entities.LowLevel.DotArk;
using ArkSaveEditor.World;
using ArkWebMapLightspeedClient;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.HttpServices
{
    public static class StructureTilesService
    {
        public const bool DEBUG_SYMBOLS = false;

        public static async Task OnHttpRequest(LightspeedRequest e, ArkWorld world, int tribeId)
        {
            //Get range
            TileData tile = TileDataTool.GetTileData(e, world.mapinfo);
            float tilePpm = 256 / tile.units_per_tile;

            //Create image
            Image<Rgba32> output = new Image<Rgba32>(256, 256);

            //Find all tiles in range
            List<QueuedTile> tilesInRange = new List<QueuedTile>();
            foreach(var t in world.structures)
            {
                //Check if the tribe matches
                if (!t.isInTribe || t.tribeId != tribeId)
                    continue;

                //Check if the image exists
                if (!ArkWebServer.image_package.images["structure"].ContainsKey(t.displayMetadata.img + ".png"))
                    continue;

                //Get image and it's width and height in game units
                Image<Rgba32> img = ArkWebServer.image_package.images["structure"][t.displayMetadata.img+".png"];
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

            //Save image
            byte[] buf;
            using(MemoryStream ms = new MemoryStream())
            {
                output.SaveAsPng(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder
                {
                    CompressionLevel = 9
                });
                buf = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(buf, 0, buf.Length);
            }
            await e.DoRespond(200, "image/png", buf);
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
