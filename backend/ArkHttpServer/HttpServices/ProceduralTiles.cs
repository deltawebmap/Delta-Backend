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
    public class ProceduralTiles
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Get values
            int zoom = int.Parse(e.Request.Query["zoom"]);
            int x = int.Parse(e.Request.Query["x"]);
            int y = int.Parse(e.Request.Query["y"]);
            string map = e.Request.Query["map"];

            //Check if in bounds
            int imageSideCount = (int)MathF.Pow(2, zoom);
            if (x < 0 || x > imageSideCount || y < 0 || y > imageSideCount)
            {
                //404
                return Program.QuickWriteToDoc(e, "Tile is out of bounds.", "text/plain", 404);
            }

            //Set reply headers
            e.Response.ContentType = "image/png";

            //Compute
            CreateProceduralTile(zoom, x, y, null, e.Response.Body);
            return null;
        }

        public static void CreateProceduralTile(int zoom, int x, int y, Image<Rgba32> source, Stream output)
        {
            //Produce tile from source
            //Calculate offsets
            int total_image_size = source.Width;
            int imageSideCount = (int)MathF.Pow(2, zoom);
            int part_size = (int)MathF.Ceiling((float)total_image_size / (float)imageSideCount);

            //Compute part
            Image<Rgba32> part = new Image<Rgba32>(part_size, part_size);
            CopyImage(ref part, source, 0, 0, x * part_size, y * part_size, part_size, part_size);
            part.Mutate(o => o.Resize(512, 512));

            //Save
            part.SaveAsPng(output);
        }

        static void CopyImage(ref Image<Rgba32> dest, Image<Rgba32> source, int desPosX, int desPosY, int sourcePosX, int sourcePosY, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x + desPosX < dest.Width && y + desPosY < dest.Height && /* Source */ x + sourcePosX < source.Width && y + sourcePosY < source.Height)
                        dest[x + desPosX, y + desPosY] = source[x + sourcePosX, y + sourcePosY];
                }
            }
        }
    }
}
