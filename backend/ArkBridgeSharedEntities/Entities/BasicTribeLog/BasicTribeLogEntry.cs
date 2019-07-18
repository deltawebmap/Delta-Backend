using ArkSaveEditor.World.WorldTypes.ArkTribeLogEntries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities.BasicTribeLog
{
    public class BasicTribeLogEntry
    {
        public Dictionary<string, BasicTribeLogPlayerOrDinoTarget> targets { get; set; }
        public List<string> steamIds { get; set; }
        public ArkTribeLogEntryType type { get; set; }
        public ArkTribeLogEntryPriority priority { get; set; }
        public string gameDay { get; set; }
        public string gameTime { get; set; }
        public string serverId { get; set; }
        public int tribeId { get; set; }
        public DateTime time { get; set; }

        public long _id { get; set; } //Only for DB
    }

    public class BasicTribeLogPlayerOrDinoTarget
    {
        public bool isDino { get; set; }
        public BasicTribeLogDinoTarget dino { get; set; }
        public BasicTribeLogPlayerTarget player { get; set; }

        public BasicTribeLogPlayerOrDinoTarget()
        {

        }

        public BasicTribeLogPlayerOrDinoTarget(ArkTribeLogPlayerOrDinoTarget t, ref List<string> steamIds)
        {
            isDino = t.isDino;
            if (t.isDino)
                dino = new BasicTribeLogDinoTarget(t.dino);
            else
                player = new BasicTribeLogPlayerTarget(t.player, ref steamIds);
        }

        public BasicTribeLogPlayerOrDinoTarget(ArkTribeLogPlayerTarget t, ref List<string> steamIds)
        {
            isDino = false;
            player = new BasicTribeLogPlayerTarget(t, ref steamIds);
        }

        public BasicTribeLogPlayerOrDinoTarget(ArkTribeLogDinoTarget t)
        {
            isDino = true;
            dino = new BasicTribeLogDinoTarget(t);
        }
    }

    public class BasicTribeLogPlayerTarget
    {
        public bool exact { get; set; } //If this is true, this was the only result.
        public bool found { get; set; } //If this is not true, use just the playername instead.

        public string name { get; set; } //Never null.

        //Below could be null
        public string playerName { get; set; }
        public string ingamePlayerName { get; set; }
        public string steamPlayerId { get; set; }
        public int? tribeId { get; set; }

        public BasicTribeLogPlayerTarget()
        {

        }

        public BasicTribeLogPlayerTarget(ArkTribeLogPlayerTarget t, ref List<string> steamIds)
        {
            exact = t.exact;
            found = t.found;
            name = t.name;
            if(t.profile != null)
            {
                playerName = t.profile.playerName;
                ingamePlayerName = t.profile.ingamePlayerName;
                steamPlayerId = t.profile.steamPlayerId;
                tribeId = t.profile.tribeId;
                if (!steamIds.Contains(t.profile.steamPlayerId))
                    steamIds.Add(t.profile.steamPlayerId);
            }
        }
    }

    public class BasicTribeLogDinoTarget
    {
        public bool exact { get; set; } //If this is true, this was the only result.
        public bool found { get; set; } //If this is not true, use just the playername instead.

        public bool isTamed { get; set; } //If this is a tamed dino
        public string dinoImg { get; set; } //The icon. Could be null.
        public string name { get; set; } //Never null.
        public int level { get; set; } //Never null.
        public string displayClassname { get; set; } //Never null.

        //Below could all be null
        public string dinoId { get; set; }

        public BasicTribeLogDinoTarget()
        {

        }

        public BasicTribeLogDinoTarget(ArkTribeLogDinoTarget t)
        {
            exact = t.exact;
            found = t.found;
            isTamed = t.isTamed;
            if (t.dinoEntry != null)
                dinoImg = t.dinoEntry.icon.image_url;
            name = t.name;
            level = t.level;
            displayClassname = t.displayClassname;
            if (t.profile != null)
                dinoId = t.profile.dinosaurId.ToString();
        }
    }
}
