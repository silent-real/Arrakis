/*
 * Arrakis | Patches/Safety/GorillaNotPatches.cs
 *
 * Copyright (C) 2026 Arrakis
 * https://github.com/silent-real/Arrakis
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using GorillaNetworking;
using HarmonyLib;

namespace Arrakis.Patches.Safety
{
    public class GorillaNotPatches
    {
        public static bool enabled = true;

        [HarmonyPatch(typeof(MonkeAgent), nameof(MonkeAgent.QuitDelay), MethodType.Enumerator)]
        public class NoQuitDelay
        {
            private static bool Prefix() =>
                !enabled;
        }

        [HarmonyPatch(typeof(MonkeAgent), nameof(MonkeAgent.CloseInvalidRoom))]
        public class NoCloseInvalidRoom
        {
            private static bool Prefix() =>
                !enabled;
        }

        [HarmonyPatch(typeof(MonkeAgent), nameof(MonkeAgent.ShouldDisconnectFromRoom))]
        public class NoShouldDisconnectFromRoom
        {
            private static bool Prefix(ref bool __result)
            {
                __result = false;
                return !enabled;
            }
        }

        [HarmonyPatch(typeof(MonkeAgent), nameof(MonkeAgent.SendReport))]
        public class NoSendReport
        {
            [HarmonyPatch(typeof(MonkeAgent), "Start")]
            [HarmonyPostfix]
            private static void RaiseLimits(MonkeAgent __instance)
            {
                __instance.rpcCallLimit = int.MaxValue;
                __instance.rpcErrorMax = int.MaxValue;
                __instance.logErrorMax = int.MaxValue;
            }
            private static bool Prefix(string susReason, string susId, string susNick, MonkeAgent __instance)
            {
                if (susId == NetworkSystem.Instance.LocalPlayer.UserId)
                {
                    __instance.reportedPlayers.Clear();
                    __instance.sendReport = false;
                    __instance._suspiciousPlayerId = "";
                    __instance._suspiciousPlayerName = "";
                    __instance._suspiciousReason = "";
                    __instance.rpcCallLimit = int.MaxValue;
                    __instance.rpcErrorMax = int.MaxValue;
                    __instance.logErrorMax = int.MaxValue;
                    __instance.logErrorCount = int.MaxValue;
                    susId = "";
                    susReason = "";
                    susNick = "";
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GorillaNetworkPublicTestsJoin), nameof(GorillaNetworkPublicTestsJoin.GracePeriod), MethodType.Enumerator)]
        public class NoGracePeriod
        {
            private static bool Prefix() =>
                false;
        }

        [HarmonyPatch(typeof(GorillaNetworkPublicTestJoin2), nameof(GorillaNetworkPublicTestJoin2.GracePeriod), MethodType.Enumerator)]
        public class NoGracePeriod2
        {
            private static bool Prefix() =>
                false;
        }

        [HarmonyPatch(typeof(GorillaPlayerScoreboardLine), nameof(GorillaPlayerScoreboardLine.ReportPlayer))]
        public class NoReportPlayer
        {
            public static bool Prefix() =>
                !enabled;
        }

        [HarmonyPatch(typeof(GorillaScoreboardTotalUpdater), nameof(GorillaScoreboardTotalUpdater.ReportMute))]
        public class NoReportMute
        {
            public static bool Prefix() =>
                !enabled;
        }

        [HarmonyPatch(typeof(GorillaServer), nameof(GorillaServer.UploadGorillanalytics))]
        public class NoUGA
        {
            public static bool Prefix() =>
                !enabled;
        }
    }
}