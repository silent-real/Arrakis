/*
 * Arrakis | Patches/Patchers/GardPatches.cs
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

using HarmonyLib;
using UnityEngine;

namespace Arrakis.Patches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.GrabbedByPlayer))]
    public class GardGrabPatch
    {
        public static bool enabled;
        public static bool Prefix(VRRig __instance, VRRig grabbedByRig, bool grabbedBody, bool grabbedLeftHand, bool grabbedWithLeftHand) =>
            !enabled;
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.DroppedByPlayer))]
    public class DropPatch
    {
        public static bool Prefix(VRRig __instance, VRRig grabbedByRig, Vector3 throwVelocity) =>
            !GardGrabPatch.enabled;
    }

    [HarmonyPatch(typeof(GuardianRPCs), nameof(GuardianRPCs.GuardianLaunchPlayer))]
    public class LaunchPatch
    {
        public static bool Prefix(Vector3 velocity) =>
            !GardGrabPatch.enabled;
    }
}