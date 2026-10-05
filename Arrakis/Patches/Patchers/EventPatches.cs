/*
 * Arrakis | Patches/Patchers/EventPatches.cs
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

using System;
using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;

namespace Arrakis.Patches.Patchers
{
    [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.RunViewUpdate))]
    public class EventPatches
    {
        public static event Action OnSerialize;
        public static Func<bool> Override;

        public static bool Prefix()
        {
            if (!PhotonNetwork.InRoom)
                return true;
            try
            {
                OnSerialize?.Invoke();
            }
            catch (Exception e)
            {
                CustomConsole.Log($"Error in OnSerialize: {e}", CustomConsole.LogType.Error);
            }

            if (Override == null)
                return true;

            try
            {
                return Override();
            }
            catch (Exception e)
            {
                CustomConsole.Log($"Error in OverrideSerialization: {e}", CustomConsole.LogType.Error);
                return false;
            }
        }
    }
    [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.RPC), typeof(PhotonView), typeof(string), typeof(RpcTarget), typeof(Player), typeof(bool), typeof(object[]))]
    public class AntiEventThingIgIdfk // fuck you this is going here -sleepy
    {
        public static Dictionary<string, Func<bool>> FilteredRPCs = new Dictionary<string, Func<bool>>();

        public static bool Prefix(PhotonView view, string methodName, RpcTarget target, Player player, bool encrypt, params object[] parameters)
        {
            if (FilteredRPCs.Count <= 0)
                return true;

            try
            {
                if (FilteredRPCs.TryGetValue(methodName, out var function))
                    return function?.Invoke() ?? true;
            }
            catch (Exception e)
            {
                CustomConsole.Log($"Error in rpc filter.{methodName}: {e}", CustomConsole.LogType.Error);
            }

            return true;
        }
    }
    [HarmonyPatch(typeof(MonkeAgent), "ShouldDisconnectFromRoom")]
    public class ShouldDisconnectFromRoom
    {
        public static bool Prefix() => false;
    }
}