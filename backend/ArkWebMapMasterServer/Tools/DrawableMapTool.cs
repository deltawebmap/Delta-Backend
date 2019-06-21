using ArkWebMapGatewayClient.Messages.Entities;
using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapMasterServer.Tools
{
    public static class DrawableMapTool
    {
        public static LiteCollection<SavedMapPoint> GetPointsCollection()
        {
            return Program.map_db.GetCollection<SavedMapPoint>("points");
        }

        public static LiteCollection<SavedMapEntry> GetMapsCollection()
        {
            return Program.map_db.GetCollection<SavedMapEntry>("maps");
        }

        public static SavedMapEntry[] GetServerMaps(string server_id, int tribe_id)
        {
            return GetMapsCollection().Find(x => x.server_id == server_id && x.tribe_id == tribe_id).ToArray();
        }

        public static void AddMapPoints(string server_id, int tribe_id, int map_id, List<ArkTribeMapDrawingPoint> points)
        {
            //Create saved map point
            SavedMapPoint saved = new SavedMapPoint
            {
                map_id = map_id,
                points = points,
                server_id = server_id,
                tribe_id = tribe_id
            };

            //Add
            GetPointsCollection().Insert(saved);
        }

        public static List<ArkTribeMapDrawingPoint> GetMapPoints(string server_id, int tribe_id, int map_id)
        {
            //Find all
            var results = GetPointsCollection().Find(x => x.map_id == map_id && x.tribe_id == tribe_id && x.server_id == server_id);
            List<ArkTribeMapDrawingPoint> output = new List<ArkTribeMapDrawingPoint>();
            foreach (var r in results)
                output.AddRange(r.points);
            return output;
        }

        public static void ClearMapPoints(string server_id, int tribe_id, int map_id)
        {
            //Delete
            GetPointsCollection().Delete(x => x.map_id == map_id && x.tribe_id == tribe_id && x.server_id == server_id);
        }
    }
}
