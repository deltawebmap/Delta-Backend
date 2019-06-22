using ArkWebMapMasterServer.MirrorEntities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArkWebMapMasterServer.Tools
{
    public class MirrorProtocolReader
    {
        public StreamReader r;

        public MirrorProtocolReader(StreamReader r)
        {
            this.r = r;
        }

        public string ReadNextString()
        {
            //Read until end of stream or until a |.
            char[] buffer = new char[1024];
            int index = 0;
            while(!r.EndOfStream)
            {
                r.ReadBlock(buffer, index, 1);
                if (buffer[index] == '|')
                    break;
                index++;
            }

            //Convert to string
            return new string(buffer, 0, index);
        }

        public int ReadNextInt()
        {
            string data = ReadNextString();
            return int.Parse(data);
        }

        public MirroredOpcode ReadNextOpcode()
        {
            int data = ReadNextInt();
            return (MirroredOpcode)data;
        }

        public MirroredVector3 ReadNextVector3()
        {
            string data = ReadNextString();
            string[] splitData = data.Split(',');
            float x = float.Parse(splitData[0]);
            float y = float.Parse(splitData[1]);
            float z = float.Parse(splitData[2]);
            return new MirroredVector3
            {
                x = x,
                y = y,
                z = z
            };
        }

        public bool CheckIfNextEntryExists()
        {
            if (r.EndOfStream)
                return false;
            //Add more checks later...
            return true;
        }

        public MirroredMessage ReadMessage()
        {
            //Read opcode
            MirroredOpcode opcode = ReadNextOpcode();
            MirroredMessage msg;
            switch(opcode)
            {
                case MirroredOpcode.EOS: msg = new MirroredMsgEOS(); break;
                case MirroredOpcode.Player: msg = new MirroredMsgPlayer(); break;
                case MirroredOpcode.Dino: msg = new MirroredMsgDino(); break;
                default: throw new Exception("Unexpected opcode.");
            }

            //Read message
            msg.ReadMsg(this);

            return msg;
        }
    }
}
