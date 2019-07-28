using ArkBridgeSharedEntities.Entities.Master;
using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.ServerDiscovery
{
    public static class SearchEndpoint
    {
        public const int PAGE_SIZE = 20;

        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser user)
        {
            //Read payload
            SearchRequest payload = Program.DecodePostBody<SearchRequest>(e);

            //Build a search query
            List<Query> queryParams = new List<Query>();
            queryParams.Add(Query.Where("is_published", x => x.AsBoolean == true));

            if (payload.server_name != null)
                queryParams.Add(Query.Where("display_name", x => x.AsString.ToLower().Contains(payload.server_name.ToLower())));
            if (payload.map_name != null)
                queryParams.Add(Query.Where("map_name", x => x.AsString == payload.map_name));
            if (payload.location != null)
                queryParams.Add(Query.Where("location", x => x.AsInt32 == (int)payload.location));
            if (payload.language != null)
                queryParams.Add(Query.Where("language", x => x.AsString == payload.language));

            //Push all of these into one and search
            Query q = queryParams[0];
            if(queryParams.Count > 1) 
                q = Query.And(queryParams.ToArray());
            var results = Program.db.GetCollection<ArkPublishedServerListing>(ServerPublishingManager.COLLECTION_NAME).Find(q, payload.page * PAGE_SIZE, PAGE_SIZE);

            //Write
            SearchReply reply = new SearchReply
            {
                token = payload.token,
                results = results.ToArray()
            };
            return Program.QuickWriteJsonToDoc(e, reply);
        }

        class SearchReply
        {
            public int token;
            public ArkPublishedServerListing[] results;
        }

        class SearchRequest
        {
            public string server_name;
            public string map_name;

            public int page;
            public int token;

            public ArkPublishedServerLocation? location;
            public string language;

            public bool? is_modded;
            public bool? is_pvp;
            public bool? is_small_tribes;
            public bool? is_shop;
            public bool? is_cluster;

            public SearchRequestRange taming_speed;
            public SearchRequestRange xp_mult;
            public SearchRequestRange gather_mult;
            public SearchRequestRange maturation_mult;
            public SearchRequestRange breeding_mult;
        }

        class SearchRequestRange
        {
            public float min;
            public float max;
        }
    }
}
