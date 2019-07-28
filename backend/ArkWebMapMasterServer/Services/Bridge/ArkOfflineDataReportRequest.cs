using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Bridge
{
    public static class ArkOfflineDataReportRequest
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s)
        {
            //Since offline reports can be large, they follow a special format. For integer size, read the integer tribe ID, integer length, and then GZipped compressed data
            int version;
            try
            {
                version = ReadIntFromStream(e.Request.Body);
                int arraySize = ReadIntFromStream(e.Request.Body);
                for (int i = 0; i < arraySize; i++)
                {
                    //Read data
                    int tribeId = ReadIntFromStream(e.Request.Body);
                    int contentLength = ReadIntFromStream(e.Request.Body);
                    byte[] content = new byte[contentLength];
                    for(int j = 0; j< contentLength; j++)
                        e.Request.Body.Read(content, j, 1);

                    //Update in database
                    Tools.OfflineTribeDataTool.UpdateArkData(s._id, tribeId, content, version);
                }
            } catch (Exception ex)
            {
                throw new StandardError("Decompression failure.", StandardErrorCode.InvalidInput, ex);
            }

            //Update server
            s.latest_offline_data_version = version;
            s.Update();

            //Return ok
            return Program.QuickWriteStatusToDoc(e, true);
        }

        private static int ReadIntFromStream(Stream s)
        {
            byte[] buf = new byte[4];
            s.Read(buf, 0, buf.Length);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buf);
            return BitConverter.ToInt32(buf);
        }
    }
}
