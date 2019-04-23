using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.PersistEntities
{
    public class TribeNote
    {
        /// <summary>
        /// ID of this note
        /// </summary>
        [JsonProperty("id")]
        public string _id { get; set; }

        /// <summary>
        /// The ID of the tribe this will be sent to.
        /// </summary>
        public int tribeId { get; set; }

        /// <summary>
        /// The user of the person who initially created this card.
        /// </summary>
        public PersistUser creator { get; set; }

        /// <summary>
        /// The date in which this event was created
        /// </summary>
        public DateTime creationDate { get; set; }

        /// <summary>
        /// The date in which this event was last updated.
        /// </summary>
        public DateTime lastUpdateDate { get; set; }

        /// <summary>
        /// The last user who updated this note.
        /// </summary>
        public PersistUser lastUpdater { get; set; }

        /// <summary>
        /// If this has a location on the map
        /// </summary>
        public bool hasLocation { get; set; }

        /// <summary>
        /// If it has a location, this is it.
        /// </summary>
        public PersistVector2 location { get; set; }

        /// <summary>
        /// If this has a time in which the event happens
        /// </summary>
        public bool hasTriggerTime { get; set; }

        /// <summary>
        /// The time in which this event is triggered, if the above is true.
        /// </summary>
        public DateTime triggerTime { get; set; }

        /// <summary>
        /// The title of the event
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// The content of the pin
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// The HTML representation of color for this pin.
        /// </summary>
        public string color { get; set; }
    }
}
