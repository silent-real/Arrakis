/*
 * Arrakis | Patches/Patchers/GTPlayerStats.cs
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

namespace Arrakis.Patches.Patchers
{
    [HarmonyPatch(typeof(GTPlayerStats), nameof(GTPlayerStats.DelayedUpdate))]
    public class GTPlayerStatsP // nova im lazy so add a setting for both of them like 0, 10, 20 to like 144 ig idk and maybe short.maxvalue (32767)
    {
        public static bool SpoofFPS;
        public static bool SpoofPing;
        public static short FPS = 90;
        public static short Ping = 10;
        public static bool Prefix()
        {
            if (SpoofFPS || SpoofPing)
            {
                if (SpoofFPS)
                    GTPlayerStats.FPS = FPS;
                if (SpoofPing)
                    GTPlayerStats.Ping = Ping;
                return false;
            }
            return true;
        }
    }
}
