using ArkHttpServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.HttpServices
{
    public static class EventService
    {
        public static List<HttpSessionEvent> events = new List<HttpSessionEvent>();

        public static Task OnEventRequest(Microsoft.AspNetCore.Http.HttpContext e, MasterServerArkUser user)
        {
            //Get DateTime from URL request
            DateTime firstTime;
            if(e.Request.Query.ContainsKey("t"))
            {
                firstTime = new DateTime(long.Parse(e.Request.Query["t"]));
            } else
            {
                //Required
                throw new Exception("Required query 't' not present.");
            }
            
            //Get next URL
            DateTime time = DateTime.UtcNow;
            string nextUrl = $"{ArkWebServer.api_prefix}/world/events?t={time.Ticks}";

            //Get events
            HttpSessionEvent[] activeEvents = events.Where(x => x.time > firstTime).ToArray();

            //Write reply
            return ArkWebServer.QuickWriteJsonToDoc(e, new HttpSessionEventsReply
            {
                events = activeEvents,
                next_url = nextUrl
            });
        }

        public static void AddEvent(HttpSessionEvent e)
        {
            //Set time
            e.time = DateTime.UtcNow;
            events.Add(e);
        }
    }
}
