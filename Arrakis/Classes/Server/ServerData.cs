/*
 * Arrakis | Classes/Server/ServerData.cs
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

using Arrakis.Managers;
using Arrakis.Menu;
using Arrakis.Notifications;
using GorillaNetworking;
using Meta.WitAi.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static Arrakis.Menu.Main;

namespace Arrakis.Classes
{
    public class ServerData : MonoBehaviour
    {
        public static bool lockdown = false;
        public static string serverversion;
        public static string motd;
        public static string menustate;
        public static string[] detectedmods;
        public static string[] patchedmods;

        private static bool showprompt = false;

        private static bool loaded = false;
        private static bool bypass = false;
        private static bool motdfall = false;

        public void Awake()
        {
            CustomConsole.Log("Getting server data.", CustomConsole.LogType.Info);
            StartCoroutine(GetServerData());
        }

        private IEnumerator GetServerData()
        {
            using (UnityWebRequest webRequest = new UnityWebRequest($"{PluginInfo.ServerApi}/serverdata", "GET"))
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    CustomConsole.Log("Error getting server data loading boards.", CustomConsole.LogType.Error);
                    bypass = true;
                    yield break; 
                }
                ServerDataResponse data = JsonConvert.DeserializeObject<ServerDataResponse>(webRequest.downloadHandler.text);
                CustomConsole.Log("Got server data.", CustomConsole.LogType.Info);
                lockdown = data.lockdown;
                menustate = data.menustate;
                motd = data.motd;
                serverversion = data.serverversion;
                detectedmods = data.detectedmods;
                patchedmods = data.patchedmods;

                CustomConsole.Log($"Got detected mods {detectedmods.Length}", CustomConsole.LogType.Info);
                if (detectedmods.Length > 0)
                    StartCoroutine(SetDetectedMods());
                CustomConsole.Log($"Got patched mods {patchedmods.Length}", CustomConsole.LogType.Info);
                if (patchedmods.Length > 0)
                    StartCoroutine(SetPatchedMods());


                bypass = false;
                loaded = true;
            }
        }

        private List<string> allDetected = new List<string>();
        private List<string> allPatched = new List<string>();
        private IEnumerator SetDetectedMods()
        {
            if (detectedmods == null || detectedmods.Length == 0)
                yield break;
            while (GorillaComputer.instance == null || !GorillaComputer.instance.isConnectedToMaster)
                yield return null;
            yield return new WaitForSeconds(2f);
            foreach (string name in detectedmods)
            {
                if (string.IsNullOrWhiteSpace(name) || allDetected.Contains(name))
                    continue;
                ButtonInfo button = Main.GetIndex(name);
                if (button != null)
                {
                    string overlap = string.IsNullOrEmpty(button.overlapText) ? button.buttonText : button.overlapText;
                    button.detected = true;
                    button.overlapText = overlap + " <color=grey>[</color><color=red>DETECTED</color><color=grey>]</color>";
                    button.isTogglable = false;
                    button.method = () => NotificationManager.SendNotification("<color=grey>[</color><color=cyan>ARRAKIS</color><color=grey>]</color> This mod is <color=red>detected</color>.");
                    button.enableMethod = button.method;
                    button.disableMethod = button.method;
                }
                allDetected.Add(name);
            }
        }

        private IEnumerator SetPatchedMods()
        {
            if (patchedmods == null || patchedmods.Length == 0)
                yield break;
            while (GorillaComputer.instance == null || !GorillaComputer.instance.isConnectedToMaster)
                yield return null;
            yield return new WaitForSeconds(2f);
            foreach (string name in patchedmods)
            {
                if (string.IsNullOrWhiteSpace(name) || allPatched.Contains(name))
                    continue;
                ButtonInfo button = Main.GetIndex(name);
                if (button != null)
                {
                    string overlap = string.IsNullOrEmpty(button.overlapText) ? button.buttonText : button.overlapText;
                    button.patched = true;
                    button.overlapText = overlap + " <color=grey>[</color><color=yellow>PATCHED</color><color=grey>]</color>";
                    button.isTogglable = false;
                    button.method = () => NotificationManager.SendNotification("<color=grey>[</color><color=cyan>ARRAKIS</color><color=grey>]</color> This mod is <color=yellow>patched</color>.");
                    button.enableMethod = button.method;
                    button.disableMethod = button.method;
                }
                allPatched.Add(name);
            }
        }

        public void Update()
        {
            if (loaded || bypass)
            {
                if (Settings.disablecustomboards)
                {
                    if (BoardManager.Instance.motdTextTMP != null)
                        BoardManager.Instance.motdTextTMP.text = BoardManager.Instance.motdTextTMP.gameObject.GetComponent<PlayFabTitleDataTextDisplay>()._cachedText;
                }
                else 
                {
                    if (bypass)
                    {
                        if (!motdfall)
                        {
                            motd = $"THANK YOU FOR USING ARRAKIS, IF YOU SEE THIS THE SERVER DATA HAS NOT BEEN LOADED OR YOUR NOT ON WIFI.\nMENU VERSION: {PluginInfo.Version}, ALL SERVER ISSUES WILL BE FIXED ONCE NOVA HAS FIXED.\n\nTHIS SHOULD BE ONLY TEMP, MADE BY NOVA, SLEEPY\nJOIN DISCORD: {PluginInfo.DiscordLink}";
                            motdfall = true;
                        }
                    }

                    if (BoardManager.Instance.motdTextTMP == null)
                        return;
                    if (!string.IsNullOrEmpty(motd))
                        BoardManager.Instance.motdTextTMP.text = string.Format(motd, PluginInfo.Version, Buttons.buttons.SelectMany(list => list).ToArray().Length);

                    if (!bypass)
                    {
                        if (Version.Parse(PluginInfo.Version) < Version.Parse(serverversion))
                        {
                            if (!showprompt)
                            {
                                showprompt = true;
                                NotificationManager.SendNotification("<color=cyan>[UPDATE]</color> Arrakis Needs a update please update.");
                                Prompt($"Arrakis is on version {PluginInfo.Version} it needs to be on version {serverversion}, would you like to update the menu.", () =>
                                {
                                    Process.Start($"https://github.com/silent-real/Arrakis/releases/tag/V{serverversion}");
                                    NotificationManager.SendNotification("<color=cyan>[UPDATE]</color> Please check your computer for latest install.");
                                }, () =>
                                {
                                    NotificationManager.SendNotification("<color=cyan>[UPDATE]</color> Make sure to update the menu later.");
                                });
                            }
                        }
                    }
                }
            }
        }

        public class ServerDataResponse
        {
            public bool lockdown;
            public string menustate;
            public string motd;
            public string serverversion;
            public string[] detectedmods;
            public string[] patchedmods;
        }
    }
}