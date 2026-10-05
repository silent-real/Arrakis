/*
 * Arrakis | Patches/Patchers/DistancePatch.cs
 *
 * Copyright (C) 2026 Arrakis
 * https://github.com/nauth-studios/Arrakis
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

using HarmonyLib;
using UnityEngine;

namespace Arrakis.Patches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.IsPositionInRange))]
    public class DistancePatch // what the fuck did i just made dude it looks so ass its 12:30 am -sleepy
    {
        public static bool enabled;

        public static void Postfix(VRRig __instance, ref bool __result, Vector3 position, float range)
        {
            NetPlayer player = g(__instance) ?? null;
            if ((enabled && __instance.isLocal) || (player != null && b(player)))
                __result = true;
        }
        public static bool b(NetPlayer Player) => Player == NetworkSystem.Instance.LocalPlayer;
        public static NetPlayer g(VRRig p) => p.Creator ?? NetworkSystem.Instance.GetPlayer(NetworkSystem.Instance.GetOwningPlayerID(p.rigSerializer.gameObject));
    }
}