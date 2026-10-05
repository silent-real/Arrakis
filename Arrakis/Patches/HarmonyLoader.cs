/*
 * Arrakis | Patches/HarmonyLoader.cs
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
using System;
using System.Reflection;

namespace Arrakis
{
    public class HarmonyLoader
    {
        private static Harmony harmony = null;
        private static bool patched = false;

        public static void ApplyPatches()
        {
            if (patched)
                return;
            patched = true;
            CustomConsole.Log("Applying patches");
            harmony = new Harmony(PluginInfo.GUID);
            int success = 0;
            int failed = 0;
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                try
                {
                    var methods = harmony.GetPatchedMethods();
                    harmony.CreateClassProcessor(type).Patch();
                    success++;
                }
                catch (Exception ex)
                {
                    failed++;
                    CustomConsole.Log($"[PATCH FAILED] {type.FullName} — {ex.GetType().Name}: {ex.Message}");
                }
            }
            CustomConsole.Log($"Patched — {success} succeeded, {failed} failed");
        }

        public static void RemovePatches()
        {
            if (!patched)
                return;
            patched = false;
            CustomConsole.Log("Removing patches");
            harmony.UnpatchSelf();
            CustomConsole.Log("Removed harmony patches");
        }
    }
}