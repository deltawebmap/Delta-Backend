﻿using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.Managers;
using ArkWebMapMasterServer.PresistEntities.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Providers
{
    public static class ProvidersHttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Authenticate
            ArkManager user = null;
            if (e.Request.Headers.ContainsKey("Authorization"))
            {
                //Read Authorization header
                string token = e.Request.Headers["Authorization"];
                if (token.StartsWith("Bearer "))
                    token = token.Substring("Bearer ".Length);
                user = ManageAuth.ValidateToken(token);
            }
            if (user == null)
                throw new StandardError("You're not signed in.", StandardErrorCode.AuthRequired);

            //Handle each
            if (path == "@me")
                return ProvidersMeService.OnHttpRequest(e, user);
            if (path.StartsWith("machines/"))
                return ArrayActionsHandler.OnHttpRequest(e, path.Substring("machines/".Length), user, ManageMachines.GetMachinesCollection(), ProvidersMachineService.CreateMachine, ProvidersMachineService.SelectGet, ProvidersMachineService.SelectPost, ProvidersMachineService.SelectDelete);
            if (path.StartsWith("servers/"))
                return ArrayActionsHandler.OnHttpRequest(e, path.Substring("servers/".Length), user, ManageServers.GetServersCollection(), ProvidersServerService.CreateMachine, ProvidersServerService.SelectGet, ProvidersServerService.SelectPost, ProvidersServerService.SelectDelete);

            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }
    }
}
