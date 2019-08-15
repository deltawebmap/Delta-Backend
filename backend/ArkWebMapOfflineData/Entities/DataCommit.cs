using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapOfflineData.Entities
{
    /// <summary>
    /// Commit saved to the database
    /// </summary>
    public class DataCommit
    {
        /// <summary>
        /// Commit ID
        /// </summary>
        public string _id { get; set; }

        /// <summary>
        /// The time this was uploaded at
        /// </summary>
        public long time { get; set; }

        /// <summary>
        /// Maps tribe IDs to filenames in the database
        /// </summary>
        public Dictionary<int, string> files { get; set; }

        /// <summary>
        /// The version that was uploaded
        /// </summary>
        public int data_version { get; set; }

        /// <summary>
        /// Set to true when data is fully uploaded
        /// </summary>
        public bool is_ready { get; set; }
    }
}
