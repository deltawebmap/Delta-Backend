using System;
using System.Collections.Generic;
using System.IO;
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

        CheckArkFile = 4, //Client prompts the server to check for an Ark file
        CheckArkFileResponse = 5, //Server responds wether or not the file existed.

        UploadConfigAndFinish = 6, //Client uploads a configuration file to the server to use.
        ServerGoodbye = 7, //Essentially a goodbye from the server before it shuts down and stops responding to requests.

        FilePickerGetDrives = 8, //Returns all drives on the system
        FilePickerGetDrivesResponse = 9, //Response to the above

        FilePickerGetDir = 10, //Returns all files and directories inside of a folder
        FilePickerGetDirResponse = 11, //Response to the above
        FilePickerGetDirError = 12, //Error to above
    }
}
