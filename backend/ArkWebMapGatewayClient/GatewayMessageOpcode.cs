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
        RealtimeMapMovement = 8,
        MessageServerStateChange = 9,
        UserAddServer = 10,
        UserLogOut = 11,
        RealtimeOnlinePlayerUpdate = 12,
        SendPushNotificationToTribe = 13,
        MessageUpdateServer = 14,
        MessageUpdateTribeDinoSettings = 15,
        SubserverMapUpdated = 16,
        SubserverOfflineDataUpdated = 17
    }
}
