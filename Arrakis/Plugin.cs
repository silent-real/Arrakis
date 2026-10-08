/*
 * Arrakis | Plugin.cs
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

using System.Collections;
using System.IO;
using Arrakis.Classes;
using Arrakis.Classes.Menu;
using Arrakis.Managers;
using Arrakis.Menu;
using Arrakis.Notifications;
using Arrakis.Patches;
using BepInEx;
using UnityEngine;

namespace Arrakis
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static GameObject holder;
        private void Awake()
        {
            HarmonyLoader.ApplyPatches();
            CustomConsole.LoadStart();
            DiscordRpc.Initialize();
            StartCoroutine(DRPC());
            GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
        }

        private static IEnumerator DRPC()
        {
            var retry = new WaitForSeconds(10f);
            var refresh = new WaitForSeconds(60f);
            while (!DiscordRpc.Connected)
            {
                yield return retry;
                if (!DiscordRpc.Connected)
                    DiscordRpc.Initialize();
            }
            while (true)
            {
                yield return refresh;
                if (DiscordRpc.Connected)
                    DiscordRpc.Update();
                else
                {
                    DiscordRpc.Initialize();
                    yield return retry;
                }
            }
        }

        public void OnPlayerSpawned()
        {
            SubscriptionKIDPatch.Apply();

            Settings.LoadSettings();

            holder = new GameObject("Arrakis");
            holder.AddComponent<BoardManager>();
            holder.AddComponent<NotificationManager>();
            holder.AddComponent<CRunner>();
            holder.AddComponent<LogEvent>();
            holder.AddComponent<UtilsGUI>();
            holder.AddComponent<PcGui>();
            holder.AddComponent<ServerData>();
            holder.AddComponent<Admin>();

            if (!Directory.Exists(PluginInfo.BaseDirectory))
                Directory.CreateDirectory(PluginInfo.BaseDirectory);
            if (!Directory.Exists($"{PluginInfo.BaseDirectory}\\Rooms"))
                Directory.CreateDirectory($"{PluginInfo.BaseDirectory}\\Rooms");
            if (!File.Exists($"{PluginInfo.BaseDirectory}/CustomTitle.txt"))
                File.WriteAllText($"{PluginInfo.BaseDirectory}/CustomTitle.txt", "your title");

            Patches.CosmeticPatch.enabled = true;
            if (Main.OneIn(500))
            {
                if (Main.OneIn(2))
                    Main.Sussy = true;
                else
                    Main.RareChance = true;
            }
        }

        private void OnDisable()
        {
            HarmonyLoader.RemovePatches();
            DiscordRpc.Shutdown();
        }
    }
}