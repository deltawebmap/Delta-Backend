using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.PresistEntities.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth
{
    public static class ManagerAuthHttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            var method = Program.FindRequestMethod(e);
            if (path == "signup" && method == RequestHttpMethod.post)
            {
                //Create an account
                //Decode body
                ArkManagerSignupPayload body = Program.DecodePostBody<ArkManagerSignupPayload>(e);

                //Create user
                ArkManager u;
                try
                {
                    u = Managers.ManageAuth.SignUp(body.email, body.password, body.name, body.img_token);
                } catch (Exceptions.ManagerSignupError err)
                {
                    return Program.QuickWriteJsonToDoc(e, new ArkManagerAuthResponse
                    {
                        ok = false,
                        msg = err.error,
                        element = err.element
                    });
                }

                //Create a token
                string token = Managers.ManageAuth.GenerateToken(u);
                return Program.QuickWriteJsonToDoc(e, new ArkManagerAuthResponse
                {
                    ok = true,
                    token = token
                });
            }
            if(path == "signin" && method == RequestHttpMethod.post)
            {
                //Sign into an existing account
                //Decode body
                ArkManagerSignupPayload body = Program.DecodePostBody<ArkManagerSignupPayload>(e);

                //Sign in
                ArkManager m = Managers.ManageAuth.SignIn(body.email, body.password);

                //Failed?
                if(m == null)
                {
                    return Program.QuickWriteJsonToDoc(e, new ArkManagerAuthResponse
                    {
                        ok = false,
                        msg = "Username or Password Incorrect",
                        element = "password"
                    });
                }

                //Create a token
                string token = Managers.ManageAuth.GenerateToken(m);
                return Program.QuickWriteJsonToDoc(e, new ArkManagerAuthResponse
                {
                    ok = true,
                    token = token
                });
            }
            
            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }
    }
}
