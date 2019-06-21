using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages.Entities
{
    public class ArkTribeMapDrawingPoint
    {
        public float ex { get; set; } //Pos X
        public float ey { get; set; } //Pos Y
        public bool n { get; set; } //Is new
    }
}
