using ArkSaveEditor.Entities;
using ArkWebMapDynamicTiles.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapDynamicTiles.Generators
{
    public abstract class BaseGenerator
    {
        private List<DiskCachedTile> cache;

        public BaseGenerator()
        {
            cache = new List<DiskCachedTile>();
        }

        /// <summary>
        /// Gets a cached tile, if it exists
        /// </summary>
        /// <param name="server_id"></param>
        /// <param name="tribe_id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private DiskCachedTile TryGetCacheTile(string server_id, int tribe_id, float x, float y, float z)
        {
            DiskCachedTile tile;
            lock (cache)
            {
                tile = cache.Where(a => a.server_id == server_id && a.tribe_id == tribe_id && a.x == x && a.y == y && a.z == z && a.expires > DateTime.UtcNow).FirstOrDefault();
            }
            return tile;
        }

        public DiskCachedTile GetTile(string server_id, TileData tile, ArkMapData mapinfo, int tribe_id, object data)
        {
            //First, check to see if we already have this tile
            DiskCachedTile response = TryGetCacheTile(server_id, tribe_id, tile.tile_x, tile.tile_y, tile.tile_z);
            if (response != null)
                return response;

            //Create a new filename to use
            string filename = Program.GenerateRandomString(42);
            while(File.Exists(Program.config.cache_path + filename))
                filename = Program.GenerateRandomString(42);

            //Create the file
            FileStream stream = new FileStream(Program.config.cache_path + filename, FileMode.Create);

            //Start compute in a new task
            Task compute = CreateTile(server_id, tile, mapinfo, tribe_id, stream, data);

            //Create a cache object
            response = new DiskCachedTile
            {
                compute = compute,
                expires = DateTime.UtcNow.AddMinutes(10),
                created = DateTime.UtcNow,
                server_id = server_id,
                tribe_id = tribe_id,
                x = tile.tile_x,
                y = tile.tile_y,
                z = tile.tile_z,
                id = filename,
                filename = Program.config.cache_path + filename
            };

            //Add to list
            cache.Add(response);

            return response;
        }

        public abstract Task CreateTile(string server_id, TileData tile, ArkMapData mapinfo, int tribe_id, Stream output, object data);
    }
}
