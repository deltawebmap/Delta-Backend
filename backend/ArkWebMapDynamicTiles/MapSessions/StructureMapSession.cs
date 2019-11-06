using System;
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

            //Get tile
            DiskCachedTile ctile = GeneratorHolder.structures.GetTile(server_id, tile, mapInfo, tribe_id, structures);

            //Wait for and obtain a stream
            using(Stream s = await ctile.Open())
            {
                //Write headers
                e.Response.ContentType = "image/png";
                e.Response.Headers.Add("X-Delta-Tile-Cache-ID", ctile.id);
                await s.CopyToAsync(e.Response.Body);
            }
        }

        public override int GetMinDataVersion()
        {
            return 2;
        }

        public override int GetMaxZoom()
        {
            return 10;
        }
    }
}
