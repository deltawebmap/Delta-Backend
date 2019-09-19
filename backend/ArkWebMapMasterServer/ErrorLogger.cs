using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer
{
    public class ErrorLogger
    {
        public static LiteCollection<LoggedErrorEntry> GetCollection()
        {
            return Program.db.GetCollection<LoggedErrorEntry>("logged_errors");
        }

        public static void LogStandardError(StandardError err, Microsoft.AspNetCore.Http.HttpContext e)
        {
            LoggedErrorEntry entry = new LoggedErrorEntry
            {
                description = err.screen_error,
                isStandardError = true,
                standardErrorCode = err.error_code,
                stackTrace = err.StackTrace,
            };
            FinishLogError(entry, e);
        }

        public static void LogException(Exception ex, Microsoft.AspNetCore.Http.HttpContext e)
        {
            LoggedErrorEntry entry = new LoggedErrorEntry
            {
                description = ex.Message,
                isStandardError = false,
                stackTrace = ex.StackTrace,
            };
            FinishLogError(entry, e);
        }

        private static void FinishLogError(LoggedErrorEntry entry, Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Try and authenticate the user
            DbUser user = null;
            try
            {
                user = Services.Users.UsersHttpHandler.AuthenticateUser(e, false, out string userToken);
            }
            catch { }
            entry.isAuth = user != null;
            if (user != null)
                entry.authUserId = user.id;
            entry.endpoint = e.Request.Path;
            entry.method = e.Request.Method;
            entry.time = DateTime.UtcNow.Ticks;

            //Add
            GetCollection().Insert(entry);
        }
    }
}
