using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class OAuthApplicationsHandler : SelectItemUserAuthDeltaService<DbOauthApp>
    {
        public const string ICON_APP_ID = "GPzQeYyDBApTcXBk";

        public OAuthApplicationsHandler(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<DbOauthApp> GetItemByRequestedString(string id)
        {
            throw new NotImplementedException();
        }

        public override async Task OnRequestToItem(DbOauthApp item)
        {
            throw new NotImplementedException();
        }

        public override async Task OnRequestNoItem()
        {
            //This is a request to create. Make sure it is a POST
            if(GetMethod() != LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
            {
                await WriteString("Method Not Supported", "text/plain", 400);
                return;
            }

            //Decode request body
            CreateApplicationRequest request = await DecodePOSTBody<CreateApplicationRequest>();

            //Verify that all required elements are listed
            List<EditResponseError> errors = new List<EditResponseError>();
            if (request.name == null)
                errors.Add(new EditResponseError("NAME", "This field is required."));
            if (request.description == null)
                errors.Add(new EditResponseError("DESCRIPTION", "This field is required."));
            if (request.redirect_uri == null)
                errors.Add(new EditResponseError("REDIRECT_URI", "This field is required."));
            if (await TryRespondWithError(e, errors))
                return;

            //Verify that all fields match requirements
            if (request.name.Length == 0)
                errors.Add(new EditResponseError("NAME", "This field is required."));
            else if (request.name.Length < 2)
                errors.Add(new EditResponseError("NAME", "Name must be at least 2 characters long."));
            else if (request.name.Length > 24)
                errors.Add(new EditResponseError("NAME", "Name must be at less than 24 characters."));
            if (request.description.Length == 0)
                errors.Add(new EditResponseError("DESCRIPTION", "This field is required."));
            else if (request.description.Length < 2)
                errors.Add(new EditResponseError("DESCRIPTION", "Description must be at least 2 characters long."));
            else if (request.description.Length > 256)
                errors.Add(new EditResponseError("DESCRIPTION", "Description must be less than 256 characters."));
            if (!request.redirect_uri.StartsWith("http://") && !request.redirect_uri.StartsWith("https://"))
                errors.Add(new EditResponseError("REDIRECT_URI", "Only http and https redirects are permitted."));
            if (await TryRespondWithError(e, errors))
                return;

            //If an icon is set, verify it
            string icon = null;
            if (request.icon_token != null)
            {
                var iconInfo = await Program.connection.GetUserContentByToken(request.icon_token);
                if (iconInfo == null)
                    errors.Add(new EditResponseError("ICON", "Icon verification failed."));
                else if (iconInfo.application_id != ICON_APP_ID)
                    errors.Add(new EditResponseError("ICON", "Icon verification failed."));
                else
                    icon = iconInfo.url;
            }

            //Generate an application ID and secret
            string appId = LibDeltaSystem.Tools.SecureStringTool.GenerateSecureString(24);
            while (await Program.connection.GetOAuthAppByAppID(appId) != null)
                appId = LibDeltaSystem.Tools.SecureStringTool.GenerateSecureString(24);
            string appSecret = LibDeltaSystem.Tools.SecureStringTool.GenerateSecureString(42);

            //Create oauth app
            DbOauthApp app = new DbOauthApp
            {
                client_id = appId,
                client_secret = appSecret,
                description = request.description,
                icon_url = icon,
                name = request.name,
                owner_id = user.id,
                redirect_uri = request.redirect_uri,
                _id = MongoDB.Bson.ObjectId.GenerateNewId()
            };

            //Insert in database
            await Program.connection.system_oauth_apps.InsertOneAsync(app);

            //Write app info
            await WriteJSON(app);
        }

        async Task<bool> TryRespondWithError(Microsoft.AspNetCore.Http.HttpContext e, List<EditResponseError> errors)
        {
            if (errors.Count == 0)
                return false;
            await WriteJSON(new EditResponse
            {
                ok = false,
                errors = errors
            });
            return true;
        }

        class CreateApplicationRequest
        {
            public string name;
            public string description;
            public string redirect_uri;
            public string icon_token;
        }

        class CreateApplicationResponse : EditResponse
        {
            public DbOauthApp app;
        }

        class EditResponse
        {
            public bool ok;
            public List<EditResponseError> errors;
        }

        class EditResponseError
        {
            public string field;
            public string text;

            public EditResponseError(string field, string text)
            {
                this.field = field;
                this.text = text;
            }
        }
    }
}
