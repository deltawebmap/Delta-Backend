using LibDeltaSystem;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.Services.Auth.AppAuth
{
    public abstract class AppAuthRequestTemplate : BasicDeltaService
    {
        public AppAuthRequestTemplate(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public static readonly string[] PREFLIGHT_OUT_URLS = new string[]
        {
            "https://deltamap.net/login/return/?state={STATE}",
            "https://dev.deltamap.net/login/return/?state={STATE}"
        };
    }
}
