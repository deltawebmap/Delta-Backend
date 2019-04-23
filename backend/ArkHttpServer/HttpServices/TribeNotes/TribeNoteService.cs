using ArkBridgeSharedEntities.Requests;
using ArkHttpServer.Entities;
using ArkHttpServer.NetEntities;
using ArkHttpServer.PersistEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.HttpServices.TribeNotes
{
    public class TribeNoteService
    {
        public static Task OnPinRequest(Microsoft.AspNetCore.Http.HttpContext e, MasterServerArkUser user, int tribeId)
        {
            //Get the request method
            RequestHttpMethod method = ArkWebServer.FindRequestMethod(e);

            //Decide what to do based on the method. Get=Get, Post=Edit, Put=Make, Delete=delete
            if (method == RequestHttpMethod.get || method == RequestHttpMethod.post || method == RequestHttpMethod.delete)
            {
                //Find the pin first
                if (!e.Request.Query.ContainsKey("id"))
                    throw new Exception("Unknown pin ID.");

                //Grab from database
                TribeNote n = TribeNoteTool.GetNoteAndVerify(e.Request.Query["id"], tribeId);

                //Choose
                if (method == RequestHttpMethod.get)
                    return ArkWebServer.QuickWriteJsonToDoc(e, n);
                else if (method == RequestHttpMethod.post)
                    return OnPostRequest(e, n, user, tribeId);
                else
                    return OnDeleteRequest(e, n, user, tribeId);
            } else if (method == RequestHttpMethod.put)
            {
                //A pin will be created.
                return OnPutRequest(e, user, tribeId);
            } else
            {
                //Invalid method
                throw new Exception("Unknown method.");
            }
        }

        public static Task OnSyncRequest(Microsoft.AspNetCore.Http.HttpContext e, MasterServerArkUser user, int tribeId)
        {
            //Check if we specified a time to check from
            DateTime startTime = new DateTime(0);
            if (e.Request.Query.ContainsKey("t"))
                startTime = new DateTime(long.Parse(e.Request.Query["t"]));

            //Grab pins
            TribeNote[] pins = TribeNoteTool.GetCollection().Find(x => x.tribeId == tribeId && x.lastUpdateDate > startTime).ToArray();

            //Create reply
            TribeNoteSyncReply reply = new TribeNoteSyncReply
            {
                next = ArkWebServer.api_prefix + "/world/tribes/notes/sync?t=" + DateTime.UtcNow.Ticks.ToString(),
                pins = pins
            };

            //Write
            return ArkWebServer.QuickWriteJsonToDoc(e, reply);
        }

        private static TribeNote ReadNoteFromBody(Microsoft.AspNetCore.Http.HttpContext e)
        {
            return ArkWebServer.DecodePostBody<TribeNote>(e);
        }

        private static Task OnDeleteRequest(Microsoft.AspNetCore.Http.HttpContext e, TribeNote source, MasterServerArkUser user, int tribeId)
        {
            //Delete it
            TribeNoteTool.GetCollection().Delete(source._id);

            //Send notification
            SendTribeNotification(source, user, tribeId, "deleted", "Note was removed.");

            //Write it
            return ArkWebServer.QuickWriteJsonToDoc(e, new OkReply
            {
                ok = true
            });
        }

        private static Task OnPostRequest(Microsoft.AspNetCore.Http.HttpContext e, TribeNote source, MasterServerArkUser user, int tribeId)
        {
            //Edit the pin and return it
            TribeNote n = TribeNoteTool.EditNote(source, ReadNoteFromBody(e), PersistUser.ConvertUser(user));

            //Send notification
            SendTribeNotification(source, user, tribeId, "edited");

            //Write it
            return ArkWebServer.QuickWriteJsonToDoc(e, n);
        }

        private static Task OnPutRequest(Microsoft.AspNetCore.Http.HttpContext e, MasterServerArkUser user, int tribeId)
        {
            //Verify note
            TribeNote n = ReadNoteFromBody(e);
            if (n.title == null || n.content == null)
                throw new Exception("Missing title or content.");
            if (n.title.Length > 1028 || n.content.Length > 2048)
                throw new Exception("Title is greater than 1028 or content is greater than 2048.");

            //Add some defaults
            if (n.color == null)
                n.color = "#5585e6";

            //Create the pin and return it
            n = TribeNoteTool.CreateNote(n, PersistUser.ConvertUser(user), tribeId);

            //Send notification
            SendTribeNotification(n, user, tribeId, "created");

            //Write it
            return ArkWebServer.QuickWriteJsonToDoc(e, n);
        }

        private static void SendTribeNotification(TribeNote source, MasterServerArkUser user, int tribeId, string action, string overrideBody = null)
        {
            try
            {
                string body = "";

                //Add time info
                if (source.hasTriggerTime)
                {
                    TimeSpan offset = source.triggerTime - DateTime.UtcNow;
                    if (offset.TotalSeconds <= 0)
                    {
                        body = "Now, ";
                    }
                    else if (offset.TotalMinutes < 60)
                    {
                        body = $"In {Math.Round(offset.TotalMinutes, 0)} minutes, ";
                    }
                    else if (offset.TotalHours < 24)
                    {
                        body = body = $"In {Math.Round(offset.TotalHours, 0)} hours, ";
                    }
                    else
                    {
                        body = $"In {Math.Round(offset.TotalDays, 0)} days, ";
                    }
                }
                body += source.content;

                //Override if needed
                if (overrideBody != null)
                    body = overrideBody;

                //Send notification
                ArkWebServer.tribeNotificationCode(tribeId, new TribeNotification
                {
                    data = new Dictionary<string, string>
                {
                    {"note_name", source.title },
                    {"editor_name", user.screen_name },
                    {"action",action },
                    {"body",body }
                },
                    type = TribeNotificationType.TribeNoteEdit
                });
            } catch
            {
                //Ignore...
            }
        }
    }
}
