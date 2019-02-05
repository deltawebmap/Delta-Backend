using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Linq;
using ArkWebMapMasterServer.NetEntities;

namespace ArkWebMapMasterServer.Users
{
    /// <summary>
    /// This covers authenticating users, but not anything else. Not even tokens.
    /// </summary>
    public static class UserAuth
    {
        public static LiteCollection<ArkUser> GetCollection()
        {
            return Program.db.GetCollection<ArkUser>("users");
        }

        public static ArkUser GetUserById(string id)
        {
            var collec = GetCollection();
            var found = collec.FindOne(x => x._id == id);
            return found;
        }

        public static ArkUser GetUserByAuthName(string id)
        {
            var collec = GetCollection();
            var found = collec.FindOne(x => x.auth.uid == id);
            return found;
        }

        private static ArkUser GenericCreateUser()
        {
            //Generate a unique ID for this user.
            string id = Program.GenerateRandomString(24);
            var collec = GetCollection();
            while(collec.Count( x => x._id == id) != 0)
                id = Program.GenerateRandomString(24);

            //Create user
            ArkUser u = new ArkUser
            {
                _id = id,
                servers = new List<string>(),
                screen_name = "",
                profile_image_url = "",
                auth_method = ArkUserSigninMethod.None,
                auth = new IAuthMethod()
            };

            //Insert
            collec.Insert(u);

            return u;
        }

        public static ArkUser CreateUserWithSteam(string steamId, SteamProfile profile)
        {
            //Check if this username already exists
            if (GetCollection().Count(x => x.auth.uid == steamId) != 0)
                return null;

            //Generate user
            ArkUser u = GenericCreateUser();
            u.screen_name = profile.personaname;
            u.profile_image_url = profile.avatarfull;
            u.is_steam_verified = true;
            u.steam_id = steamId;

            //Add auth method
            u.auth = new AuthMethod_Steam
            {
                uid = steamId
            };

            //Set auth method
            u.auth_method = ArkUserSigninMethod.SteamProfile;

            //Update
            u.Update();

            //Respond
            return u;
        }

        public static ArkUser CreateUserWithUsernameAndPassword(string username, string password)
        {
            //Check if this username already exists
            if (GetCollection().Count(x => x.auth.uid == username) != 0 || username.Length > 24 || username.Length < 4)
                return null;
            
            //Generate the password salt
            byte[] salt = Program.GenerateRandomBytes(64);

            //Hash the password
            byte[] hash = HashPassword(password, salt);

            //Generate user
            ArkUser u = GenericCreateUser();
            u.screen_name = username;

            //Add auth method
            u.auth = new AuthMethod_UsernamePassword
            {
                password = hash,
                salt = salt,
                uid = username
            };

            //Set auth method
            u.auth_method = ArkUserSigninMethod.UsernamePassword;

            //Update
            u.Update();

            //Respond
            return u;
        }

        public static ArkUser SignInUserWithUsernamePassword(string username, string password)
        {
            //Find all users with this signin method
            var matchingNames = GetCollection().Find(x => x.auth_method == ArkUserSigninMethod.UsernamePassword && x.auth.uid == username).ToArray();

            //If there are more than 1 of these, fatal error
            if (matchingNames.Length > 1)
                throw new StandardError("There were more than one matching users with the same username.", StandardErrorCode.InternalSigninError);

            //If there are zero users with this username, return null
            if (matchingNames.Length == 0)
                return null;

            ArkUser user = matchingNames[0];
            AuthMethod_UsernamePassword auth = (AuthMethod_UsernamePassword)user.auth;

            //Hash our password based on the salt of the existing user
            byte[] passwordHash = HashPassword(password, auth.salt);

            //Compare
            bool authOk = Program.CompareByteArrays(passwordHash, auth.password);

            //If auth was ok, return the user. Else, return null
            if (authOk)
                return user;
            else
                return null;
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            return KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8);
        }
    }
}
