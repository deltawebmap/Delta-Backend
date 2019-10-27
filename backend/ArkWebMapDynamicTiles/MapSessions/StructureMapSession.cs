﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArkSaveEditor.Entities;
using ArkSaveEditor.World.WorldTypes;
using ArkWebMapDynamicTiles.Entities;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using ArkSaveEditor.ArkEntries;
using System.Linq;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.Content;
using MongoDB.Driver;

namespace ArkWebMapDynamicTiles.MapSessions
{
    public class StructureMapSession : MapSession
    {
        public ArkMapData mapInfo;
        public List<DbStructure> structures;

        public const int MIN_RESIZED_SIZE = 3;

        public override async Task OnCreate(HttpContext e, DbServer server, int tribeId)
        {
            //Get map data
            mapInfo = Program.ark_maps[server.latest_server_map];

            //Get map structures
            var filterBuilder = Builders<DbStructure>.Filter;
            var filter = filterBuilder.Eq("tribe_id", tribeId) & filterBuilder.Eq("server_id", server.id);
            var results = await Program.connection.content_structures.FindAsync(filter);
            structures = await results.ToListAsync();
        }

        public override async Task OnHttpRequest(HttpContext e, float x, float y, float z)
        {
            //Get tile info
            TileData tile = TileDataTool.GetTileData(x, y, z, mapInfo);

            //Compute and create the tile
            Image<Rgba32> image = await Compute(tile, mapInfo, structures, tribe_id);

            //Resize
            if(resolution != 256)
            {
                image.Mutate(i => i.Resize(256, 256));
            }

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

        public override int GetMaxZoom()
        {
            return 10;
        }

        public const bool DEBUG_SYMBOLS = false;

        static List<CachedStructure> structure_resize_cache = new List<CachedStructure>();

        private async Task<Image<Rgba32>> Compute(TileData tile, ArkMapData mapinfo, List<DbStructure> structures, int tribeId)
        {
            //Get range
            float tilePpm = resolution / tile.units_per_tile;

            //Create image
            Image<Rgba32> output = new Image<Rgba32>(resolution, resolution);

            //Find all tiles in range
            List<QueuedTile> tilesInRange = new List<QueuedTile>();
            foreach (var t in structures)
            {
                //Check if the tribe matches
                if (t.tribe_id != tribeId)
                    continue;

                //Get classname
                string classname = t.classname;
                if (classname.EndsWith("_C"))
                    classname = classname.Substring(0, classname.Length - 2);

                //Get structure metadata
                var displayMetadata = Program.structure_metadata.Where(x => x.names.Contains(classname)).FirstOrDefault();
                if (displayMetadata == null)
                    continue;

                //Check if the image exists
                if (!Program.image_package.images["structure"].ContainsKey(displayMetadata.img + ".png"))
                    continue;

                //Get image and it's width and height in game units
                Image<Rgba32> img = Program.image_package.images["structure"][displayMetadata.img + ".png"];
                float ppm = displayMetadata.capturePixels / displayMetadata.captureSize;
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
                if (img_scale_x < MIN_RESIZED_SIZE || img_scale_y < MIN_RESIZED_SIZE)
                    continue;

                //Apply transformations, or load from cache
                var cacheHits = structure_resize_cache.Where(x => x.size_x == img_scale_x && x.size_y == img_scale_y && x.img == displayMetadata.img && x.rot == t.location.yaw).ToArray();
                Image<Rgba32> simg;
                if(cacheHits.Length == 0)
                {
                    //Create object
                    var c = new CachedStructure
                    {
                        time = DateTime.UtcNow,
                        uses = 1,
                        size_x = img_scale_x,
                        size_y = img_scale_y,
                        rot = t.location.yaw,
                        img = displayMetadata.img
                    };

                    //Start the creation, then add it
                    c.image = c.Create();
                    structure_resize_cache.Add(c);

                    //Now, wait for it to finish
                    simg = c.image.GetAwaiter().GetResult();
                } else
                {
                    //Load from cache
                    cacheHits[0].uses++;
                    cacheHits[0].time = DateTime.UtcNow;
                    simg = cacheHits[0].image.GetAwaiter().GetResult();
                }

                //Determine location
                float loc_tile_x = (t.location.x - (img_game_width / 2) - tile.game_min_x) / tile.units_per_tile; //Top left of the image inside the tile
                float loc_tile_y = (t.location.y - (img_game_height / 2) - tile.game_min_y) / tile.units_per_tile; //Top left of the image inside the tile
                int copy_offset_x = (int)(loc_tile_x * resolution);
                int copy_offset_y = (int)(loc_tile_y * resolution);

                //Queue
                tilesInRange.Add(new QueuedTile
                {
                    img = simg,
                    copy_offset_x = copy_offset_x,
                    copy_offset_y = copy_offset_y,
                    z = t.location.z,
                    display_type = displayMetadata.displayType
                });
            }

            //Sort by Y level
            tilesInRange.Sort(new Comparison<QueuedTile>((x, y) =>
            {
                if (x.display_type == StructureDisplayMetadata_DisplayType.AlwaysTop || y.display_type == StructureDisplayMetadata_DisplayType.AlwaysTop)
                    return x.display_type.CompareTo(y.display_type);
                return x.z.CompareTo(y.z);
            }));
            /*tilesInRange.Sort(new Comparison<QueuedTile>((x, y) =>
                x.display_type.CompareTo(y.display_type)
            ));*/

            //Now, copy the images
            foreach (var q in tilesInRange)
            {
                //Copy
                for (int x = 0; x < q.img.Width; x++)
                {
                    for (int y = 0; y < q.img.Height; y++)
                    {
                        //Check if this is just alpha
                        if (q.img[x, y].A == 0)
                            continue;

                        //Check if in range
                        if (x + q.copy_offset_x < 0 || y + q.copy_offset_y < 0)
                            continue;
                        if (x + q.copy_offset_x >= output.Width || y + q.copy_offset_y >= output.Height)
                            continue;

                        //Mix color
                        Rgba32 sourcePixel = output[x + q.copy_offset_x, y + q.copy_offset_y];
                        Rgba32 edgePixel = q.img[x, y];
                        float edgeMult = ((float)edgePixel.A) / 255;
                        float sourceMult = 1 - edgeMult;
                        Rgba32 mixedColor = new Rgba32(
                            ((((float)edgePixel.R) / 255) * edgeMult) + ((((float)sourcePixel.R) / 255) * sourceMult),
                            ((((float)edgePixel.G) / 255) * edgeMult) + ((((float)sourcePixel.G) / 255) * sourceMult),
                            ((((float)edgePixel.B) / 255) * edgeMult) + ((((float)sourcePixel.B) / 255) * sourceMult),
                            ((((float)edgePixel.A) / 255) * edgeMult) + ((((float)sourcePixel.A) / 255) * sourceMult)
                        );
                        output[x + q.copy_offset_x, y + q.copy_offset_y] = mixedColor;
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

        class CachedStructure
        {
            public int size_x;
            public int size_y;
            public float rot;
            public string img;
            public DateTime time;
            public int uses;

            public Task<Image<Rgba32>> image;

            public async Task<Image<Rgba32>> Create()
            {
                //Create
                Image<Rgba32> source = Program.image_package.images["structure"][img + ".png"];
                var simg = source.Clone();
                simg.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new SixLabors.Primitives.Size(size_x, size_y),
                    Position = AnchorPositionMode.Center
                }).Rotate(rot));

                //Return this
                return simg;
            }
        }
    }
}
