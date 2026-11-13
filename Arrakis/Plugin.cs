/*
using static Arrakis.HarmonyLoader;
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
using Arrakis.Mods;
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
        public static Coroutine discordC;

        public void Awake()
        {
            HarmonyLoader.ApplyPatches();
            CustomConsole.LoadStart();

            DiscordRpc.Initialize();

            GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
        }

        public static IEnumerator DRPC()
        {
            var retry = new WaitForSeconds(10f);
            var refresh = new WaitForSeconds(60f);
            while (true)
            {
                if (Settings.disablediscordrpc)
                {
                    DiscordRpc.Shutdown();
                    discordC = null;
                    yield break;
                }
                if (!DiscordRpc.Connected)
                {
                    DiscordRpc.Initialize();
                    yield return retry;
                }
                else
                {
                    DiscordRpc.Update();
                    yield return refresh;
                }
            }
        }

        public void OnPlayerSpawned()
        {
            Settings.LoadSettings();

            holder = new GameObject("Arrakis");

            holder.AddComponent<NotificationManager>();
            holder.AddComponent<BoardManager>();

            holder.AddComponent<CRunner>();
            holder.AddComponent<LogEvent>();

            holder.AddComponent<UtilsGUI>();
            holder.AddComponent<PcGui>();

            holder.AddComponent<ServerData>();
            holder.AddComponent<Admin>();

            holder.AddComponent<RpcRoomWatcher>();

            if (!Directory.Exists(PluginInfo.BaseDirectory))
                Directory.CreateDirectory(PluginInfo.BaseDirectory);
            if (!Directory.Exists($"{PluginInfo.BaseDirectory}\\Rooms"))
                Directory.CreateDirectory($"{PluginInfo.BaseDirectory}\\Rooms");
            if (!File.Exists($"{PluginInfo.BaseDirectory}/CustomTitle.txt") && Main.GetIndex("Custom Menu Title").enabled)
                KeyboardManager.Input("What do you want the title to be?", thing => File.WriteAllText($"{PluginInfo.BaseDirectory}/CustomTitle.txt", thing));

            Patches.CosmeticPatch.enabled = true;
            if (Main.OneIn(500))
            {
                if (Main.OneIn(2))
                    Main.Sussy = true;
                else
                    Main.RareChance = true;
            }

            if (!Settings.disablediscordrpc)
                discordC = CRunner.instance.StartCoroutine(DRPC());
        }

        public void OnDisable()
        {
            HarmonyLoader.RemovePatches();
            DiscordRpc.Shutdown();
            Advantage.TagOnJoin();
        }
    }
}