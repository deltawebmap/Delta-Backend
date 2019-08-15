using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapOfflineData.Entities
{
    /// <summary>
    /// Maps a server to a commit
    /// </summary>
    public class DataServer
    {
        /// <summary>
        /// The server ID
        /// </summary>
        public string _id { get; set; }

        /// <summary>
        /// The latest commit
        /// </summary>
        public string latest_commit { get; set; }

        /// <summary>
        /// The commit before this
        /// </summary>
        public string previous_commit { get; set; }
    }
}
