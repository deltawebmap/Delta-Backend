using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapLauncher
{
    public class LauncherConfig
    {
        public LauncherConfig_SubserverRelease latest_subserver;
        public LauncherConfig_LauncherOptions launcher_options;
    }

    public class LauncherConfig_SubserverRelease
    {
        public float version;
        public Dictionary<string, LauncherConfig_SubserverReleaseBinary> binaries;
        public string change_notes;
    }

    public class LauncherConfig_SubserverReleaseBinary
    {
        public string url;
        public string binary_pathname;
    }

    public class LauncherConfig_LauncherOptions
    {
        public double bg_refresh_ms;

        public bool maintenance_on;
        public string maintenance_msg;
        public bool maintenance_skippable;
    }
}
