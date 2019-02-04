using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.Servers
{
    public class ArkServerInviteManager
    {
        public static LiteCollection<ArkServerInvite> GetCollection()
        {
            return Program.db.GetCollection<ArkServerInvite>("player_invites");
        }

        public static ArkServerInvite CreateInvite(ArkUser u, ArkServer s, TimeSpan timeUntilExpires)
        {
            //Generate the random ID
            var collec = GetCollection();
            string id = Program.GenerateRandomString(24);
            while (collec.Count(x => x._id == id) != 0)
                id = Program.GenerateRandomString(24);

            //Create object
            ArkServerInvite invite = new ArkServerInvite
            {
                creation_time = DateTime.UtcNow.Ticks,
                expiration_time = DateTime.UtcNow.Add(timeUntilExpires).Ticks,
                inviter_uid = u._id,
                server_uid = s._id,
                is_tribe = false,
                tribe_uid = null,
                uses = 0,
                _id = id
            };

            //Insert
            collec.Insert(invite);

            //Return
            return invite;
        }

        /// <summary>
        /// Gets an invite and validates that it is still valid.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ArkServerInvite GetInviteById(string id)
        {
            var invite = GetCollection().FindOne(x => x._id == id);
            if (invite == null)
                return null;
            if (DateTime.UtcNow.Ticks > invite.expiration_time)
                return null;
            return invite;
        }

        /// <summary>
        /// Makes a user join a server.
        /// </summary>
        /// <returns></returns>
        public static ArkServer AcceptInvite(ArkUser u, string id)
        {
            //Get invite
            ArkServerInvite invite = GetInviteById(id);

            //Join
            if(!u.servers.Contains(invite.server_uid))
                u.servers.Add(invite.server_uid);

            //Update
            u.Update();
            invite.uses++;
            invite.Update();

            //Return server
            return invite.GetServer();
        }
    }
}
