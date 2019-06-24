using ArkHttpServer.Entities;
using ArkSaveEditor.World.WorldTypes;
using ArkWebMapLightspeedClient.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer
{
    public static class UserServerDataManager
    {
        public static UserServerData GetUserData(MasterServerArkUser user, int tribeId)
        {
            var collec = ArkWebServer.db.GetCollection<UserServerData>("user_server_data");
            var item = collec.FindOne(x => x.tribeId == tribeId && x.userId == user.id);
            if (item != null)
                return item;
            item = new UserServerData
            {
                userId = user.id,
                tribeId = tribeId
            };
            item._id = collec.Insert(item);
            return item;
        }

        public static TribeServerData GetTribeData(int tribeId)
        {
            var collec = ArkWebServer.db.GetCollection<TribeServerData>("tribe_server_data");
            var item = collec.FindOne(x => x.tribeId == tribeId);
            if (item != null)
                return item;
            item = new TribeServerData
            {
                tribeId = tribeId
            };
            item._id = collec.Insert(item);
            return item;
        }

        public static DinoServerData GetDinoData(ArkDinosaur dino)
        {
            var collec = ArkWebServer.db.GetCollection<DinoServerData>("dino_server_data");
            var item = collec.FindOne(x => x.tribeId == dino.tribeId && x.dinoId == dino.dinosaurId.ToString());
            if (item != null)
                return item;
            item = new DinoServerData
            {
                tribeId = dino.tribeId,
                dinoId = dino.dinosaurId.ToString()
            };
            item._id = collec.Insert(item);
            return item;
        }

        public static void PutUserData(UserServerData d)
        {
            var collec = ArkWebServer.db.GetCollection<UserServerData>("user_server_data");
            collec.Update(d);
        }

        public static void PutTribeData(TribeServerData d)
        {
            var collec = ArkWebServer.db.GetCollection<TribeServerData>("tribe_server_data");
            collec.Update(d);
        }

        public static void PutDinoData(DinoServerData d)
        {
            var collec = ArkWebServer.db.GetCollection<DinoServerData>("dino_server_data");
            collec.Update(d);
        }
    }
}
