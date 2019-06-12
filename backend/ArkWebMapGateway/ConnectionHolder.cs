using ArkWebMapGateway.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway
{
    public static class ConnectionHolder
    {
        public static MasterServerGatewayConnection master;
        public static Dictionary<string, SubServerGatewayConnection> subservers = new Dictionary<string, SubServerGatewayConnection>(); //Server IDs
        public static List<FrontendGatewayConnection> users = new List<FrontendGatewayConnection>();
        public static List<NotificationConnection> notificationClients = new List<NotificationConnection>();
    }
}
