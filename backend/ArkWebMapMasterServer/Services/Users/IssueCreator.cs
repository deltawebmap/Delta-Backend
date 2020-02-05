
using ArkWebMapMasterServer.NetEntities;
using LibDeltaSystem.Db.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class IssueCreator
    {
        private static Dictionary<string, string> client_name_topics = new Dictionary<string, string>
        {
            {"web", "Web Platform" },
            {"android", "Android Platform" }
        };

        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser u)
        {
            //Decode issue
            CreateIssueRequest request = Program.DecodePostBody<CreateIssueRequest>(e);

            //Get topic name
            string client_tag;
            if (client_name_topics.ContainsKey(request.client_name))
                client_tag = client_name_topics[request.client_name];
            else
                throw new StandardError("Unknown Client", StandardErrorCode.InvalidInput);

            //Get server
            DbServer server = await Program.connection.GetServerByIdAsync(request.server_id);
            if (server == null)
                throw new StandardError("Server Not Found", StandardErrorCode.InvalidInput);
            int? tribeIdInt = server.TryGetTribeIdAsync(Program.connection, u.steam_id).GetAwaiter().GetResult();
            string tribeId = tribeIdInt.HasValue ? tribeIdInt.Value.ToString() : "*No Tribe ID*";

            //Get the screenshot
            var screenshot = UserContentUploader.FinishContentUpload(request.screenshot_token);

            //Get attachments and make their body
            string attachment_body = "";
            for(int i = 0; i<request.attachment_tokens.Length; i++)
            {
                var attachment = UserContentUploader.FinishContentUpload(request.attachment_tokens[i]);
                attachment_body += $"[{request.attachment_names[i].Replace("]", "\\]")}]({attachment.url}) ";
            }
            if (request.attachment_tokens.Length == 0)
                attachment_body = "*No attachments*";

            //Create the body
            string body = $"*This issue was reported automatically inside of the app.*\n\n**[Description]**\n{request.body_description}\n\n**[Expected Result]**\n{request.body_expected}\n\n**[Screenshot]**\n![image]({screenshot.url})\n\n**[Client Info]**\n**Client-Name**: {request.client_name},\n**Client-Name**:  {request.client_info}\n\n**[Attachments]**\n{attachment_body}\n\n**[User/Server Data]**\n**User Internal ID**: {u._id},\n**Server Internal ID**: {server.id},\n**Server Map**: {server.latest_server_map},\n**Tribe ID**: {tribeId}\n**Server Name**: {server.display_name}\n\n**[Report Info]**\nThis report was created by the Delta Web Map backend server.";

            //Submit
            string response_string;
            using (HttpClient hc = new HttpClient())
            {
                //Send
                hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Program.config.github_api_key);
                hc.DefaultRequestHeaders.Add("User-Agent", "DeltaWebMap-Master-Server");
                var response = await hc.PostAsync("https://api.github.com/repos/deltawebmap/Delta-Web-Map-User-Reports/issues", new StringContent(JsonConvert.SerializeObject(new GitHubIssueCreateRequest
                {
                    body = body,
                    title = $"[{request.topic}] {request.title}",
                    labels = new string[]
                    {
                        "Auto Generated",
                        client_tag
                    }
                })));

                //Check
                response_string = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Unexpected Status Code");
            }

            //Decode
            await Program.QuickWriteJsonToDoc(e, new CreateIssueResponse
            {
                url = JsonConvert.DeserializeObject<GitHubIssueCreateResponse>(response_string).html_url
            });
        }
    }
}
