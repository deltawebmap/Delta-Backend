using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapDynamicTiles.Entities
{
    public class DiskCachedTile
    {
        public string server_id;
        public int tribe_id;
        public DateTime created;
        public DateTime expires;

        public float x;
        public float y;
        public float z;

        public Task compute; //Task for computing the tile
        public string filename;
        public string id;

        public async Task<Stream> Open()
        {
            //Wait for compute to finish
            await compute;

            //Open a stream on this
            return new FileStream(filename, FileMode.Open, FileAccess.Read);
        }
    }
}
