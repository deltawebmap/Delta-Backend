using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Init
{
    public class ArkSetupProxyMessage
    {
        public ArkSetupProxyMessage_Type type;
        public Dictionary<string, string> data;

        public string from_ip;
    }

    public enum ArkSetupProxyMessage_Type
    {
        WebClientHello = 0,
        ServerHello = 1,

        WebClientRequestServerTestPort = 2, //The client would like to request the server to begin running a test TCP server on a port.
        ServerPortTestReadyToClient = 3, //Server has port test ready and the client should run it.

        CheckArkFile = 4, //Client prompts the server to check for an Ark file
        CheckArkFileResponse = 5, //Server responds wether or not the file existed.

        UploadConfigAndFinish = 6, //Client uploads a configuration file to the server to use.
        ServerGoodbye = 7, //Essentially a goodbye from the server before it shuts down and stops responding to requests.
    }
}
