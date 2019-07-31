using ArkHttpServer.Tools;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.NetEntities.ItemSearch
{
    public class WebArkInventoryPlayer : WebArkInventoryHolder
    {
        public string name;
        public string icon;
        public string sub_name;

        public static WebArkInventoryPlayer Convert(ArkPlayer p)
        {
            return new WebArkInventoryPlayer
            {
                icon = SteamUserData.GetSteamProfile(p.steamId).avatarfull,
                name = p.steamName,
                sub_name = p.playerName
            };
        }
    }
}
