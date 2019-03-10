using ArkBridgeSharedEntities.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ArkWebMapSlaveServerConsole
{
    public class TcpTester
    {
        static TcpListener test_tcp_listener;

        public static void OnBeginRequest(ArkSetupProxyMessage message)
        {
            try
            {
                //Start listener on port
                int port = int.Parse(message.data["port"]);

                //If it is already set, kill
                if (test_tcp_listener != null)
                {
                    try
                    {
                        test_tcp_listener.Stop();
                    }
                    catch
                    {

                    }
                }

                test_tcp_listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
                test_tcp_listener.Start();

                test_tcp_listener.BeginAcceptTcpClient(OnSocketOpened, message);

                //Send OK message
                Program.SendMasterMessage(new ArkSetupProxyMessage
                {
                    data = new Dictionary<string, string>
                    {
                        {"ok","true" }
                    },
                    type = ArkSetupProxyMessage_Type.ServerPortTestReadyToClient
                });
            } catch
            {
                //Kill server and allow the test to fail.
                try
                {
                    test_tcp_listener.Stop();
                } catch
                {

                }

                //Send failed
                Program.SendMasterMessage(new ArkSetupProxyMessage
                {
                    data = new Dictionary<string, string>
                    {
                        {"ok","false" }
                    },
                    type = ArkSetupProxyMessage_Type.ServerPortTestReadyToClient
                });
            }
        }

        static void OnSocketOpened(IAsyncResult ar)
        {
            try
            {
                //Get socket
                Socket sock = test_tcp_listener.EndAcceptSocket(ar);
                sock.ReceiveTimeout = 5000;

                byte[] buf = new byte[5];
                sock.Receive(buf);

                //Check
                byte[] expectedInput = new byte[] { 0x50, 0x69, 0x6E, 0x67, 0x21 };
                if (!CompareByteArrays(buf, expectedInput))
                {
                    throw new Exception(); //Fail test
                }

                //Send reply back
                sock.Send(new byte[] { 0x50, 0x6F, 0x6E, 0x67, 0x21 });

                //Close
                sock.Close();
                test_tcp_listener.Stop();
            } catch
            {

            }
        }

        public static bool CompareByteArrays(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
                return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                    return false;
            }
            return true;
        }
    }
}
