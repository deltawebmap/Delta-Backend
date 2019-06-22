using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient
{
    public enum GatewayMessageOpcode
    {
        PingRequest = 0,
        PingResponse = 1,
        TribeMapInput = 2,
        TribeMapFrontendOutput = 3,
        SetSessionId = 4,
        TribeMapBackendOutput = 5,
        EchoToTribe = 6,
        OnDrawableMapChange = 7,
        RealtimeMapMovement = 8
    }
}
