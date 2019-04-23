using ArkHttpServer.PersistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkHttpServer.HttpServices.TribeNotes
{
    public class TribeNoteTool
    {
        public static LiteCollection<TribeNote> GetCollection()
        {
            return ArkWebServer.db.GetCollection<TribeNote>("tribe_notes");
        }

        public static TribeNote CreateNote(TribeNote inputParams, PersistUser user, int tribeId)
        {
            //Create the base note, then apply changes
            var collec = GetCollection();

            //Generate an ID
            string id = ArkWebServer.GenerateRandomString(24);
            while (collec.Find(x => x._id == id).Count() != 0)
                id = ArkWebServer.GenerateRandomString(24);

            //Create base note
            TribeNote source = new TribeNote
            {
                _id = id,
                tribeId = tribeId,
                creator = user,
                creationDate = DateTime.UtcNow,
                title = inputParams.title
            };

            //Apply content
            ApplyChanges(source, inputParams, user);
            source.lastUpdateDate = source.creationDate;

            //Save to database
            collec.Insert(source);

            return source;
        }

        public static TribeNote EditNote(TribeNote source, TribeNote inputParams, PersistUser user)
        {
            var collec = GetCollection();

            //Apply changes
            ApplyChanges(source, inputParams, user);

            //Save
            collec.Update(source);

            return source;
        }

        public static TribeNote GetNoteAndVerify(string id, int tribeId)
        {
            var collec = GetCollection();

            //Grab note
            TribeNote note = collec.FindOne(x => x._id == id);
            if (note == null)
                throw new Exception("Note not found.");
            if (note.tribeId != tribeId)
                throw new Exception("This note does not belong to your tribe.");

            return note;
        }

        /// <summary>
        /// Edits the source input.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="request"></param>
        /// <param name="editor"></param>
        public static void ApplyChanges(TribeNote source, TribeNote request, PersistUser editor)
        {
            source.lastUpdateDate = DateTime.UtcNow;
            source.lastUpdater = editor;
            source.hasLocation = request.hasLocation;
            source.location = request.location;
            source.hasTriggerTime = request.hasTriggerTime;
            source.triggerTime = request.triggerTime;
            source.content = request.content;
            source.color = request.color;
        }
    }
}
