using ArkWebMapGatewayClient.Messages;
using ArkWebMapGatewayClient.Messages.Entities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Tools;
using LibDeltaSystem.Db.System;
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

        public override Tuple<UpdateEntityRealtimePosition, int> ProcessMsg(DbServer s, ArkMirrorToken auth)
        {
            return null;
        }
    }
}
