using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR
{
    public class MEHoloConstant
    {
        public const string LiveServerHandlerName = "/SpectatorView";

        public const string LiveConfigFile = "MEConfigLive.ini";

        public const string NetworkConfigFile = "MEConfigNetwork.ini";

        public const string CalibrationConfigFile = "MEConfigCalibration.ini";

        public const string LiveAgentConfigFile = "MEConfigLiveAgent.ini";

#if ME_LIVE_ACTIVE
        public const bool IsLiveActive = true;
#else
        public const bool IsLiveActive = false;
#endif
    }
}