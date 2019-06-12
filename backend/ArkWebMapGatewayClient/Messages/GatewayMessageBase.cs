using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class GatewayMessageBase
    {
        /// <summary>
        /// Status code
        /// </summary>
        public GatewayMessageOpcode opcode;

        /// <summary>
        /// Extra data
        /// </summary>
        public Dictionary<string, string> headers;
    }
}
