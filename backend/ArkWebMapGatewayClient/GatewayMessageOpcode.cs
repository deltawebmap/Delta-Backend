using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient
{
    public enum GatewayMessageOpcode
    {
        OnDrawableMapChange = 0,
        SetSessionId = 1,
        OnServerListUpdate = 2,
        MessageDirListing = 3,
        OnMachineUpdateServerList = 4,
        OnMirrorDinoUpdate = 5,
    }
}
