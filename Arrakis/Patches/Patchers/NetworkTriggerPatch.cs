using GorillaNetworking;
using HarmonyLib;

namespace Arrakis.Patches.Patchers
{
    [HarmonyPatch(typeof(GorillaNetworkJoinTrigger), nameof(GorillaNetworkJoinTrigger.OnBoxTriggered))]
    public class NetworkTriggerPatch
    {
        public static bool enabled;
        public static bool Prefix() =>
            !enabled;
    }
}