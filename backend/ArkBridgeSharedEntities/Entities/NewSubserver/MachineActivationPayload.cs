using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities.NewSubserver
{
    public class MachineActivationPayload
    {
        public int version_minor;
        public int version_major;
        public string enviornment;
        public string mode;
        public string shorthand_token;
    }
}
