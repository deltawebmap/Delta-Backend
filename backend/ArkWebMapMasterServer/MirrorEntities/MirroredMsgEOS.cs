using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.MirrorEntities
{
    public class MirroredMsgEOS : MirroredMessage
    {
        public override void ReadMsg(MirrorProtocolReader reader)
        {
            //Nothing to read...
            opcode = MirroredOpcode.EOS;
        }

        public override void ProcessMsg(ArkServer s, ArkMirrorToken auth)
        {
            
        }
    }
}
