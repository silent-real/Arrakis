/*
 * Arrakis | Patches/HitBoxesPatch.cs
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
using static Arrakis.Mods.Advantage;

namespace Arrakis.Patches
{
    [HarmonyPatch(typeof(GorillaTagger), "get_sphereCastRadius")]
    public class HitBoxesPatch
    {
        public static bool enabled;
        public static void Postfix(GorillaTagger __instance, ref float __result)
        {
            if (enabled)
            {
                __result = hitboxScale;
            }
            else if (__result != 0.03f)
                __result = 0.03f;
        }
    }
}