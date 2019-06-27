using ArkWebMapMasterServer.Exceptions;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.PresistEntities.Managers;
using LiteDB;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapMasterServer.Managers
{
    public static class ManageAuth
    {
        public static LiteCollection<ArkManager> GetManagersCollection()
        {
            return Program.db.GetCollection<ArkManager>("manager_users");
        }

        public static LiteCollection<ArkManagerToken> GetManagerTokensCollection()
        {
            return Program.db.GetCollection<ArkManagerToken>("manager_tokens");
        }

        public static ArkManager SignUp(string email, string password, string profileName, string profileImgToken)
        {
            //Create a user
            var collec = GetManagersCollection();

            //Verify username and password
            if (password.Length < 8)
                throw new ManagerSignupError("Password must be longer than 8 characters.", "password");
            if (password.Length > 128)
                throw new ManagerSignupError("Password must be shorter than 128 characters.", "password");
            if(collec.Find( x => x.email == email).Count() != 0)
                throw new ManagerSignupError("A manager already exists with this E-Mail. Try signing in?", "email");

            //Generate the password salt
            byte[] salt = Program.GenerateRandomBytes(64);

            //Hash the password
            byte[] hash = HashPassword(password, salt);

            //Verify profile
            if (profileName == null || profileImgToken == null)
                throw new ManagerSignupError("Profile is missing.", null);
            if(profileName.Length < 2 || profileName.Length > 24)
                throw new ManagerSignupError("Name must be between 2-24 characters.", "profile_name");
            if (profileImgToken == null)
                throw new ManagerSignupError("Missing profile image.", "profile_image");
            UserContentTokenPayload image = UserContentUploader.FinishContentUpload(profileImgToken);
            if (image == null)
                throw new ManagerSignupError("Image verification failed.", "profile_image");

            //Generate a user ID
            string id = Program.GenerateRandomString(24);
            while (collec.FindById(id) != null)
                id = Program.GenerateRandomString(24);

            //Generate an API token
            string apiToken = Program.GenerateRandomString(64);
            while (collec.Find(x => x.api_token == apiToken).Count() != 0)
                apiToken = Program.GenerateRandomString(64);

            //Create a user
            ArkManager m = new ArkManager
            {
                creationDate = DateTime.UtcNow.Ticks,
                email = email,
                password = hash,
                password_salt = salt,
                profile = new ArkManagerProfile
                {
                    name = profileName,
                    wide_image_url = image.url
                },
                api_token = apiToken,
                _id = id
            };

            //Add to database
            collec.Insert(m);

            return m;
        }

        public static ArkManager SignIn(string email, string password)
        {
            //Find the user with this E-Mail, if any
            ArkManager user = GetManagersCollection().FindOne(x => x.email == email);

            //Use this user's salt to hash this password. Then, compare it
            byte[] hash = HashPassword(password, user.password_salt);
            bool authOk = Program.CompareByteArrays(hash, user.password);

            //Fail?
            if (!authOk)
                return null;
            else
                return user;
        }

        public static string GenerateToken(ArkManager user)
        {
            //Generate a unique token
            var collec = GetManagerTokensCollection();
            string token = Program.GenerateRandomString(24);
            while (collec.FindById(token) != null)
                token = Program.GenerateRandomString(24);

            //Insert
            ArkManagerToken t = new ArkManagerToken
            {
                expiryDate = DateTime.UtcNow.AddHours(12).Ticks,
                managerId = user._id,
                _id = token
            };
            collec.Insert(t);
            return t._id;
        }

        public static ArkManager GetManagerById(string id)
        {
            return GetManagersCollection().FindById(id);
        }

        public static ArkManager ValidateToken(string t)
        {
            var collec = GetManagerTokensCollection();
            var result = collec.FindById(t);
            if (result == null)
                return null;
            if (DateTime.UtcNow.Ticks > result.expiryDate)
                return null;
            return GetManagerById(result.managerId);
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            return KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8);
        }
    }
}
