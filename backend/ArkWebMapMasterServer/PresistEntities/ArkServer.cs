using ArkBridgeSharedEntities;
using ArkBridgeSharedEntities.Entities;
using ArkWebMapGatewayClient.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class ArkServer
    {
        /// <summary>
        /// Name shown in the UI
        /// </summary>
        public string display_name { get; set; }

        /// <summary>
        /// URL to a server icon.
        /// </summary>
        public string image_url { get; set; }

        /// <summary>
        /// Has a custom icon
        /// </summary>
        public bool has_custom_image { get; set; }

        /// <summary>
        /// ID of the owner of the server
        /// </summary>
        public string owner_uid { get; set; }

        /// <summary>
        /// Creds checked to verify the connection between the slave server.
        /// </summary>
        public byte[] server_creds { get; set; }

        /// <summary>
        /// ID of the server
        /// </summary>
        [JsonProperty("id")]
        public string _id { get; set; }

        /// <summary>
        /// The latest local accounts the server has. These are Ark accounts, not our accounts.
        /// </summary>
        public List<ArkSlaveReport_PlayerAccount> latest_server_local_accounts { get; set; }

        /// <summary>
        /// The latest map the server was on.
        /// </summary>
        public string latest_server_map { get; set; }

        /// <summary>
        /// Latest time of the Ark server
        /// </summary>
        public float latest_server_time { get; set; }

        /// <summary>
        /// The time the last server report was downloaded.
        /// </summary>
        public long latest_server_report_downloaded { get; set; }

        /// <summary>
        /// If we have the above four values
        /// </summary>
        public bool has_server_report { get; set; }

        /// <summary>
        /// If the server was deleted, this is set to true.
        /// </summary>
        public bool is_deleted { get; set; }

        /// <summary>
        /// If this is set to false, no auth will be required to access this. However, it will not appear in a users' server list.
        /// </summary>
        public bool require_auth_to_view { get; set; }

        /// <summary>
        /// If this is set to true, this will be treated as a demo server.
        /// </summary>
        public bool is_demo_server { get; set; }

        /// <summary>
        /// Is published and public
        /// </summary>
        public bool is_published { get; set; }

        /// <summary>
        /// If this managed by a provider
        /// </summary>
        public bool is_managed { get; set; }

        /// <summary>
        /// The ID of the provider managing this server.
        /// </summary>
        public string provider_id { get; set; }

        /// <summary>
        /// The linked provider server ID
        /// </summary>
        public string provider_server_id { get; set; }

        /// <summary>
        /// The version of the latest offline data. -1 signals that this has never been downloaded.
        /// </summary>
        public int latest_offline_data_version { get; set; } = -1;

        public void Update()
        {
            //If needed, get a new icon
            if(!has_custom_image)
            {
                image_url = GetPlaceholderIcon();
            }
            
            //Save
            ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetCollection().Update(this);
        }

        public string GetPlaceholderIcon()
        {
            return StaticGetPlaceholderIcon(display_name);
        }

        public static string StaticGetPlaceholderIcon(string display_name)
        {
            //Find letters
            string[] words = display_name.Split(' ');
            char[] charset = "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
            string output = "";
            for(int i = 0; i<words.Length; i++)
            {
                if (output.Length >= 2)
                    break;
                if(words[i].Length > 1)
                {
                    char c = words[i][0];
                    if(charset.Contains(c))
                    {
                        string sc = new string(new char[] { c });
                        if (output.Length == 0)
                            sc = sc.ToUpper();
                        else
                            sc = sc.ToLower();
                        output += sc;
                    }
                }
            }

            //Now, return URL
            return "https://icon-assets.deltamap.net/legacy/placeholder_server_images/" + output + ".png";
        }

        public bool TryGetTribeId(string steamId, out int tribeId)
        {
            tribeId = -1;
            var foundArkPlayers = latest_server_local_accounts.Where(x => x.player_steam_id == steamId);
            if (foundArkPlayers.Count() == 1)
            {
                tribeId = foundArkPlayers.First().player_tribe_id;
                return true;
            } else
            {
                return false;
            }
        }

        public bool TryGetOfflineDataForTribeStreamed(string steamId, out DateTime time, Stream outputStream)
        {
            if (has_server_report)
            {
                if (TryGetTribeId(steamId, out int tribeId))
                {
                    return Tools.OfflineTribeDataTool.GetArkDataDecompressedStreamed(this._id, tribeId, out time, outputStream);
                }
            }
            time = DateTime.MinValue;
            return false;
        }

        public bool TryGetOfflineDataForTribe(string steamId, out DateTime time, out string data)
        {
            time = DateTime.MinValue;
            data = null;
            if (has_server_report)
            {
                if (TryGetTribeId(steamId, out int tribeId))
                {
                    data = Tools.OfflineTribeDataTool.GetArkDataDecompressed(this._id, tribeId, out time);
                }
            }
            
            return data != null;
        }
    }
}
