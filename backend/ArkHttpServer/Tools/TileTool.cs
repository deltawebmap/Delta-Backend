using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArkHttpServer.Tools
{
    /// <summary>
    /// Produces map tiles based on data.
    /// </summary>
    public static class TileTool
    {
        /// <summary>
        /// Produce a tile with data being a square.
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="xOffsetTile"></param>
        /// <param name="yOffsetTile"></param>
        /// <param name="data"></param>
        /// <param name="dataSize"></param>
        /// <returns></returns>
        public static Stream ProduceTile(int zoom, int xOffsetTile, int yOffsetTile, Rgba32[,] data, int dataSize, int tileSize = 512)
        {
            //First, convert the coords we got into world percentages.

            //Calculate the number of tiles on the axis.
            float mapAxisTileCount = MathF.Pow(2, zoom);

            //Create a tile
            Image<Rgba32> image = new Image<Rgba32>(tileSize, tileSize);

            //Loop through pixels
            for(int imgX = 0; imgX<tileSize; imgX++)
            {
                for(int imgY = 0; imgY<tileSize; imgY++)
                {
                    //Get normalized location
                    float normX = (xOffsetTile + ((float)imgX / tileSize)) / mapAxisTileCount;
                    float normY = (yOffsetTile + ((float)imgY / tileSize)) / mapAxisTileCount;

                    //Using this, find an index inside of the data.
                    int indexX = (int)MathF.Round((float)dataSize * normX);
                    int indexY = (int)MathF.Round((float)dataSize * normY);

                    //Keep in bounds
                    if (indexX >= dataSize)
                        indexX = dataSize - 1;
                    if (indexY >= dataSize)
                        indexY = dataSize - 1;

                    //Copy
                    image[imgX, imgY] = data[indexX, indexY];
                }
            }

            //Output image
            MemoryStream ms = new MemoryStream();
            image.SaveAsPng(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
