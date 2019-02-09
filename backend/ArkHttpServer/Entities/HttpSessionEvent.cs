using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class HttpSessionEventsReply
    {
        public HttpSessionEvent[] events;
        public string next_url;
    }

    public class HttpSessionEvent
    {
        public object data;
        public HttpSessionEventType type;
        public DateTime time;

        public HttpSessionEvent(object data, HttpSessionEventType type)
        {
            this.data = data;
            this.type = type;
            time = DateTime.UtcNow;
        }
    }

    public enum HttpSessionEventType
    {
        MapUpdate = 0, //Issued when the Ark map file is updated.
        TestEvent = 1, //Testing
    }
}
