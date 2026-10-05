/*
 * Arrakis | Patches/Logging/SendReport.cs
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

using Arrakis.Notifications;
using HarmonyLib;
using static Arrakis.Settings;

namespace Arrakis.Patches
{
    [HarmonyPatch(typeof(MonkeAgent), nameof(MonkeAgent.SendReport))]
    internal class SendReport
    {
        private static void Prefix(string susReason, string susId, string susNick)
        {
            bool isSelf = susId == NetworkSystem.Instance.LocalPlayer.UserId;
            if (showanticheatreportself)
            {
                if (isSelf)
                {
                    NotificationManager.SendNotification($"<color=yellow>[ANTICHEAT]</color> Reported {susNick} for {susReason}.", 4f);
                    CustomConsole.Log($"AntiCheat Reported {susNick}, {susId} for {susReason}", CustomConsole.LogType.Info);
                }
            }
            else if (showanticheatreports)
            {
                if (!isSelf)
                {
                    NotificationManager.SendNotification($"<color=yellow>[ANTICHEAT]</color> Reported {susNick} for {susReason}.", 4f);
                    CustomConsole.Log($"AntiCheat Reported {susNick}, {susId} for {susReason}", CustomConsole.LogType.Info);
                }
            }
            susReason = null;
            susId = null;
            susNick = null;
        }
    }
}