using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapDynamicTiles.Entities
{
    public class ContentMetadata
    {
        public string _id { get; set; } //Server id

        public Dictionary<string, string> content { get; set; } //Content tokens
        public int version { get; set; } //Version
        public string revision { get; set; } //Changes with each upload
        public long time { get; set; } //Time

        public T GetContent<T>(string key)
        {
            //Get token
            string token = content[key];

            //Get
            return ContentTool.GetContent<T>(token);
        }
    }
}
