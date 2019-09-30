using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities.NewSubserver
{
    public class RemoteDirListing
    {
        public bool children_ok;
        public List<RemoteDir> children;
    }

    public class RemoteDir
    {
        public string type;
        public string name;
        public string pathname;
        public bool children_ok;
        public List<RemoteDir> children;
    }
}
