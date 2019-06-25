using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ArkWebMapGatewayClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace ArkWebMapLightspeed
{
    class Program
    {
        public static AWMGatewayClient gateway;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        public static Task MainAsync()
        {
            //Connect to the GATEWAY
            gateway = AWMGatewayClient.CreateClient(GatewayClientType.System, "LIGHTSPEED", "", 1, 1, true, new GatewayHandler(), SystemKeys.ObtainKey());

            //Start session
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, 43296, listenOptions =>
                    {
                        listenOptions.UseHttps("certificate.pfx", SystemKeys.ObtainCertKey());
                    });
                    options.Listen(addr, 43295);
                })
                .UseStartup<Program>()
                .Build();

            return host.RunAsync();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Run(HttpHandler.OnHttpRequest);
        }

        public static Task QuickWriteToDoc(Microsoft.AspNetCore.Http.HttpContext context, string content, string type = "text/html", int code = 200)
        {
            var response = context.Response;
            response.StatusCode = code;
            response.ContentType = type;

            //Load the template.
            string html = content;
            var data = Encoding.UTF8.GetBytes(html);
            response.ContentLength = data.Length;
            return response.Body.WriteAsync(data, 0, data.Length);
        }
    }
}
