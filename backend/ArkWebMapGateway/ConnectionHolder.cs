using ArkWebMapGateway.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway
{
    public static class ConnectionHolder
    {
        public static MasterServerGatewayConnection master;
        public static List<FrontendGatewayConnection> users = new List<FrontendGatewayConnection>();
        public static List<SystemGatewayConnection> systemClients = new List<SystemGatewayConnection>();
        public static List<NotificationConnection> notificationClients = new List<NotificationConnection>();
    }
}
