using System;
using System.Collections.Generic;
using System.Text;
using ArkWebMapMasterServer.PresistEntities.Managers;

namespace ArkWebMapMasterServer.NetEntities.Managers
{
    public class InternalMachineConfigResponse
    {
        public string id;
        public ArkManagerProfile profile;
        public ArkManagerServer[] servers;
        public ArkManagerMachine machine;
        public Dictionary<string, InternalMachineConfigResponseServerInfo> linked_servers;
    }

    public class InternalMachineConfigResponseServerInfo
    {
        public string id;
        public byte[] creds;
    }
}
