using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapMasterServer.Tools
{
    public static class MirrorTokenTool
    {
        public static LiteCollection<ArkMirrorToken> GetCollection()
        {
            return Program.db.GetCollection<ArkMirrorToken>("mirror_tokens");
        }

        public static ArkMirrorToken TryMatchToken(string token)
        {
            var results = GetCollection().Find(x => x.hasToken == true && x.token == token);
            if (results.Count() == 1)
                return results.First();
            else
                return null;
        }
    }
}
