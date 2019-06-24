using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes;
using ArkWebMapLightspeedClient;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.HttpServices
{
    public class PopulationService
    {
        public const int POPULATION_DETAIL = 16;
        public const int TILE_SIZE = 512;

        public static async Task OnHttpRequest(LightspeedRequest e, ArkWorld world)
        {
            //Process a tile request. Get the locations
            int zoom = int.Parse(e.query["zoom"]);
            int x = int.Parse(e.query["x"]);
            int y = int.Parse(e.query["y"]);
            string filtered_classname = e.query["filter"];

            //Respect permissions
            if (!ArkWebServer.CheckPermission("allowHeatmapDinoFilter"))
                filtered_classname = "";

            //Check if in bounds
            Stream img;
            if(x < 0 || y < 0 || x > MathF.Pow(2, zoom) || y > MathF.Pow(2, zoom))
            {
                //Write blank tile
                Image<Rgba32> imgData = new Image<Rgba32>(TILE_SIZE, TILE_SIZE);
                img = new MemoryStream();
                imgData.SaveAsPng(img);
                img.Position = 0;
            } else
            {
                //Get population data.
                List<ArkDinosaur>[,] population = GetFilteredPopulationData(filtered_classname, world);

                //Convert each tile to colors
                Rgba32[,] colorData = ConvertPopulationToColorData(POPULATION_DETAIL, population);

                //Convert to tile
                img = Tools.TileTool.ProduceTile(zoom, x, y, colorData, POPULATION_DETAIL, TILE_SIZE);
            }

            //Write
            byte[] data = new byte[img.Length];
            img.Read(data, 0, data.Length);
            await e.DoRespond(200, "image/png", data);
        }

        private static List<ArkDinosaur>[,] GetFilteredPopulationData(string filter, ArkWorld world)
        {
            List<ArkDinosaur>[,] population_source = world.cached_population;
            if (population_source == null)
                population_source = world.GetPopulation(POPULATION_DETAIL);

            List<ArkDinosaur>[,] population = new List<ArkDinosaur>[POPULATION_DETAIL, POPULATION_DETAIL];

            //Filter population data.
            for (int x = 0; x < POPULATION_DETAIL; x++)
            {
                for (int y = 0; y < POPULATION_DETAIL; y++)
                {
                    //Loop through population list and remove
                    if (population_source[x, y] == null)
                        population_source[x, y] = new List<ArkDinosaur>();
                    population[x, y] = new List<ArkDinosaur>();
                    for (int i = 0; i< population_source[x,y].Count; i++)
                    {
                        if(population_source[x, y][i].classnameString == filter || filter == "")
                        {
                            population[x, y].Add(population_source[x, y][i]);
                        }
                    }
                }
            }

            return population;
        }

        private static Rgba32[,] ConvertPopulationToColorData(int size, List<ArkDinosaur>[,] population)
        {
            //Loop through population and find the min and max
            int min = int.MaxValue;
            int max = int.MinValue;

            for(int x = 0; x<size; x++)
            {
                for(int y = 0; y<size; y++)
                {
                    int count = 0;
                    if(population[x,y] != null)
                        count = population[x, y].Count;
                    min = Math.Min(count, min);
                    max = Math.Max(count, max);
                }
            }

            //Loop through and create color data
            Rgba32[,] colorData = new Rgba32[size, size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int count = 0;
                    if (population[x, y] != null)
                        count = population[x, y].Count;
                    float percent = ((float)(count - min) / (float)(max - min));
                    colorData[x, y] = new Rgba32(1, 0, 0, percent);
                }
            }

            return colorData;
        }
    }
}
