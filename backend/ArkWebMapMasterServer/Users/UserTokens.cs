using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapMasterServer.Users
{
    public static class UserTokens
    {
        public static LiteCollection<UserToken> GetCollection()
        {
            return Program.db.GetCollection<UserToken>("user_tokens");
        }

        public static string GenerateUserToken(ArkUser u)
        {
            //Generate unique token
            string tokenString = Program.GenerateRandomString(32);
            LiteCollection<UserToken> collec = GetCollection();
            while (collec.Count(x => x.token == tokenString) != 0)
                tokenString = Program.GenerateRandomString(32);

            //Create object
            UserToken token = new UserToken
            {
                createTime = DateTime.UtcNow.Ticks,
                token = tokenString,
                uid = u._id
            };

            //Inser
            collec.Insert(token);

            //Respond
            return tokenString;
        }

        public static ArkUser ValidateUserToken(string token)
        {
            //Find all tokens matching
            var matchingTokens = GetCollection().Find(x => x.token == token).ToArray();

            //If there were no tokens found, respond with that
            if (matchingTokens.Length == 0)
                return null;

            //If there was a token, get user
            string uid = matchingTokens[0].uid;
            return UserAuth.GetUserById(uid);
        }
    }
}
