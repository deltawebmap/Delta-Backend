using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient
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
        public Dictionary<string, string> headers = new Dictionary<string, string>();
    }
}
