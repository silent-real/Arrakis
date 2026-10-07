/*
 * Arrakis | Mods/Settings.cs
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

using Arrakis.Classes;
using Arrakis.Classes.Menu;
using Arrakis.Managers;
using Arrakis.Menu;
using Arrakis.Mods;
using Arrakis.Notifications;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using static Arrakis.Menu.Main;

namespace Arrakis
{
    public class Settings
    {
        public static ExtGradient backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.cyan, Color.magenta) };
        public static ExtGradient[] buttonColors = new ExtGradient[]
        {
            new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) }, // Disabled
            new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) } // Enabled
        };
        public static Color[] textColors = new Color[]
        {
            Color.white, // Disabled
            Color.white // Enabled
        };

        public static void GlobalReturn()
        {
            NotificationManager.ClearAllNotifications();
            Toggle(Buttons.buttons[currentCategoryIndex][GetCategory("Main")].buttonText);
            if (prompts.Count > 0)
                StopCurrentPrompt();
        }

        public static bool fpsCounter = true;
        public static bool disconnectButton = true;
        public static bool rightHanded;
        public static bool disableNotifications;
        public static bool outlineMenu;
        public static bool menusounds = true;
        public static bool gunline = true;
        public static bool gunpointer = true;
        public static bool cosmeticfinder = true;

        public static bool showanticheatreports = false;
        public static bool showanticheatreportself = false;

        public static bool shouldBePC;

        public static bool followheadmesh = false;
        public static bool followmenutheme = false;
        public static bool fliparraylist = false;
        public static bool flipnotifications = false;
        public static bool menuanimation = false;
        public static bool highqualitytext = false;
        public static bool lowercasetext = false;
        public static bool uppercasetext = false;
        public static bool menutrail = false;
        public static bool pointertrail = false;
        public static bool disablepointer = false;
        public static bool disablemenudrop = false;
        public static bool disableautosave = false;
        public static bool disableReturnButton = false;
        public static bool disableSearchButton = false;
        public static bool lowgravitymenu = false;
        public static bool custommenutitle = false;
        public static bool disablemenutitle = false;
        public static bool disablepagenumber = false;
        public static bool disablecustomboards = false;
        public static bool disableroomnotifications = false;
        public static bool clearNotificationsOnDisconnect = false;
        public static bool disableFavBinds = false;
        public static bool disableQuickactionsBinds = false;
        public static bool ghostView = true;
        public static bool panicPrompt = true;
        public static bool stumptext = true;
        public static bool instanttag = true;

        public static KeyCode keyboardButton = KeyCode.Q;

        public static Vector3 menuSize = new Vector3(0.1f, 1f, 1f); // Depth, width, height
        public static int buttonsPerPage = 8;

        public static bool logphotonevents = false;

        public static bool disablevibrations;
        public static bool disablebuttonsounds;

        public static bool disableAntiReportVisualizer;
        public static bool disableNetworkedMenu;

        public static void JoinDiscord()
        {
            Prompt("Join Arrakis Discord", () =>
            {
                Process.Start("https://novax.lol/d");
            });
        }

        public static void Players()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Players", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." }
            };
            if (!NetworkSystem.Instance.InRoom)
                buttons.Add(new ButtonInfo { buttonText = "not in a room", label = true });
            else
            {
                for (int i = 0; i < NetworkSystem.Instance.PlayerListOthers.Length; i++)
                {
                    NetPlayer plr = NetworkSystem.Instance.PlayerListOthers[i];
                    buttons.Add(new ButtonInfo { buttonText = $"plr{i}", overlapText = plr.NickName, isTogglable = false, method =() => GoToPlayer(plr), toolTip = "Lets you see player info and more." });
                }
            }
            Buttons.buttons[GetCategory("Players")] = buttons.ToArray();
            CurrentCategoryName = "Players";
        }

        private static void GoToPlayer(NetPlayer plr)
        {
            string plrName = plr.NickName;
            VRRig rig = RigManager.GetVRRigFromPlayer(plr);
            List<ButtonInfo> buttons = new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "exit plr", overlapText = $"Exit {plrName}", method =() => Players(), isTogglable = false, toolTip = "Returns to the players page for the menu." },
                new ButtonInfo { buttonText = "Tag plr", overlapText = $"Tag {plrName}", method =() => Advantage.TagPlayer(plr), isTogglable = true, toolTip = "Tags the person." },
                new ButtonInfo { buttonText = "plr name", overlapText = $"Name: {plrName}", label = true },
                new ButtonInfo { buttonText = "plr id", overlapText = $"UserID: {plr.UserId}", label = true },
                new ButtonInfo { buttonText = "plr fps", overlapText = $"FPS: {rig.fps}", label = true },
            };
            if (Admin.Admins.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
            {
                buttons.AddRange(new[]
                {
                    new ButtonInfo { buttonText = "admin kick plr", overlapText = $"Admin Kick {plrName}", isTogglable = false, method =() => Admin.ExecuteCommand("kick", ReceiverGroup.All, plr.UserId) }
                });
            }
            Buttons.buttons[GetCategory("Temporary")] = buttons.ToArray();
            CurrentCategoryName = "Temporary";
        }

        public static float gradientSpeed = 0.5f;
        public static bool FloatMenu = false;

        private static int NotiDelayIndex = 0;
        private static string[] NotiDelayNames = new string[]
        {
            "Default", "Slow", "Very Slow", "Fast"
        };
        public static void ChangeNotificationDelay(bool increment = true)
        {
            if (increment)
            {
                NotiDelayIndex = (NotiDelayIndex + 1) % NotiDelayNames.Length;
            }
            else
            {
                NotiDelayIndex = (NotiDelayIndex - 1 + NotiDelayNames.Length) % NotiDelayNames.Length;
            }

            switch (NotiDelayIndex)
            {
                case 0: NotificationDelay = 1f; break; // Default
                case 1: NotificationDelay = 5f; break; // Slow
                case 2: NotificationDelay = 10f; break; // Very Slow
                case 3: NotificationDelay = 0.4f; break; // Fast
            }
            GetIndex("Change Notification Delay").overlapText = $"Change Notification Delay <color=grey>[<color=cyan>{NotiDelayNames[NotiDelayIndex]}</color>]</color>";
        }

        public static float buttonclickvolume = 0.4f;
        private static int buttonclickvolumeindex = 0;
        public static void ChangeButtonClickVolume(bool increment = true)
        {
            const int value = 10;
            if (increment)
            {
                buttonclickvolumeindex = (buttonclickvolumeindex + 1) % value;
            }
            else
            {
                buttonclickvolumeindex = (buttonclickvolumeindex - 1 + value) % value;
            }

            switch (buttonclickvolumeindex)
            {
                case 0: buttonclickvolume = 0.1f; break;
                case 1: buttonclickvolume = 0.2f; break;
                case 2: buttonclickvolume = 0.3f; break;
                case 3: buttonclickvolume = 0.4f; break;
                case 4: buttonclickvolume = 0.5f; break;
                case 5: buttonclickvolume = 0.6f; break;
                case 6: buttonclickvolume = 0.7f; break;
                case 7: buttonclickvolume = 0.8f; break;
                case 8: buttonclickvolume = 0.9f; break;
                case 9: buttonclickvolume = 1f; break;
            }
            GetIndex("Change Click Volume").overlapText = $"Change Click Volume <color=grey>[<color=cyan>{buttonclickvolume}f</color>]</color>";
        }
        private static int GunLineindex = 0;
        public static void ChangeGunline(bool increment = true)
        {
            const int value = 5;
            if (increment)
            {
                GunLineindex = (GunLineindex + 1) % value;
            }
            else
            {
                GunLineindex = (GunLineindex - 1 + value) % value;
            }

            switch (GunLineindex)
            {
                case 0:
                    gunLineStyle = 0;
                    GetIndex("Change Gun Line").overlapText = "Change Gun Line <color=grey>[<color=cyan>Default</color>]</color>";
                    break;
                case 1:
                    gunLineStyle = 1;
                    GetIndex("Change Gun Line").overlapText = "Change Gun Line <color=grey>[<color=cyan>Straight</color>]</color>";
                    break;
                case 2:
                    gunLineStyle = 2;
                    GetIndex("Change Gun Line").overlapText = "Change Gun Line <color=grey>[<color=cyan>Rainbow</color>]</color>";
                    break;
                case 3:
                    gunLineStyle = 3;
                    GetIndex("Change Gun Line").overlapText = "Change Gun Line <color=grey>[<color=cyan>ZigZag</color>]</color>";
                    break;
                case 4:
                    gunLineStyle = 4;
                    GetIndex("Change Gun Line").overlapText = "Change Gun Line <color=grey>[<color=cyan>Pulse</color>]</color>";
                    break;
            }
        }

        private static int AntiReportIndexRange = 0;
        public static float AntiReportRange = 0.55f;
        public static void ChangeAntiReportRange(bool increment = true)
        {
            const int value = 4;
            if (increment)
            {
                AntiReportIndexRange = (AntiReportIndexRange + 1) % value;
            }
            else
            {
                AntiReportIndexRange = (AntiReportIndexRange - 1 + value) % value;
            }

            switch (AntiReportIndexRange)
            {
                case 0:
                    AntiReportRange = 0.55f;
                    GetIndex("Change Anti Report Range").overlapText = "Change Anti Report Range <color=grey>[<color=cyan>Default</color>]</color>";
                    break;
                case 1:
                    AntiReportRange = 0.25f;
                    GetIndex("Change Anti Report Range").overlapText = "Change Anti Report Range <color=grey>[<color=cyan>Small</color>]</color>";
                    break;
                case 2:
                    AntiReportRange = 1f;
                    GetIndex("Change Anti Report Range").overlapText = "Change Anti Report Range <color=grey>[<color=cyan>Big</color>]</color>";
                    break;
                case 3:
                    AntiReportRange = 5f;
                    GetIndex("Change Anti Report Range").overlapText = "Change Anti Report Range <color=grey>[<color=cyan>Massive</color>]</color>";
                    break;
            }
        }

        public static void FreezePlayerInMenu()
        {
            if (menu != null)
                GorillaTagger.Instance.bodyCollider.attachedRigidbody.linearVelocity = new Vector3(0f, 0f, 0f);
        }
        public static void FreezeRigInMenu()
        {
            if (menu != null)
                VRRig.LocalRig.enabled = false;
            else
                VRRig.LocalRig.enabled = true;
        }

        public static int currentTheme = 0;
        public static void ChangeMenuTheme(bool increment = true)
        {
            const int value = 70;
            if (increment)
            {
                currentTheme = (currentTheme + 1) % value;
            }
            else
            {
                currentTheme = (currentTheme - 1 + value) % value;
            }

            switch (currentTheme)
            {
                case 0: // Default
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.cyan, Color.magenta) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Default</color>]</color>";
                    break;
                case 1: // Blue
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.blue) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Blue</color>]</color>";
                    break;
                case 2: // Red
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.blue) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Red</color>]</color>";
                    break;
                case 3: // Transparent
                    backgroundColor = new ExtGradient { transparent = true };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Transparent</color>]</color>";
                    break;
                case 4: // Pastel
                    backgroundColor = new ExtGradient { pastelRainbow = true };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Pastel</color>]</color>";
                    break;
                case 5: // Black Grey Fade
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.black, Color.grey) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>BG Fade</color>]</color>";
                    break;
                case 6: // White Black Fade
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.white, Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>WB Fade</color>]</color>";
                    break;
                case 7: // Full Black
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.grey;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Black</color>]</color>";
                    break;
                case 8: // Full White
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.white) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.white) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.white) };
                    textColors[0] = Color.black;
                    textColors[1] = Color.grey;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>White</color>]</color>";
                    break;
                case 9: // Rainbow
                    backgroundColor = new ExtGradient { rainbow = true };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Rainbow</color>]</color>";
                    break;
                case 10: // Porn Hub cause sleepy wanted it -nova
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.orange, Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.orange) };
                    textColors[0] = Color.orange;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>PH</color>]</color>";
                    break;
                case 11: // Grey
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.grey) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.grey) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Grey</color>]</color>";
                    break;
                case 12: // Dark Grey
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.1f, 0.1f, 0.1f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.1f, 0.1f, 0.1f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Dark Grey</color>]</color>";
                    break;
                case 13: // Yellow
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.yellow) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.yellow) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Yellow</color>]</color>";
                    break;
                case 14: // Cyan
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.cyan) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.cyan) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=black>Cyan</color>]</color>";
                    break;
                case 15: // Sunset
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.orange, new Color(0.5f, 0f, 0.5f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.orange) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Sunset</color>]</color>";
                    break;
                case 16: // Ocean
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.blue, Color.cyan) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.cyan) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Ocean</color>]</color>";
                    break;
                case 17: // Forest
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.green, Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.green) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Forest</color>]</color>";
                    break;
                case 18: // Lava
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.red, Color.yellow) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Lava</color>]</color>";
                    break;
                case 19: // Neon
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.magenta, Color.cyan) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.magenta) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Neon</color>]</color>";
                    break;
                case 20: // Midnight
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.05f, 0.05f, 0.2f), Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.2f, 0.2f, 0.5f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.cyan;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Midnight</color>]</color>";
                    break;
                case 21: // Gold
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.yellow, Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.yellow) };
                    textColors[0] = Color.yellow;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Gold</color>]</color>";
                    break;
                case 22: // Mint
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.green, Color.white) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.white) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.green) };
                    textColors[0] = Color.black;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Mint</color>]</color>";
                    break;
                case 23: // Blood
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.4f, 0f, 0f), Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.red) };
                    textColors[0] = Color.red;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Blood</color>]</color>";
                    break;
                case 24: // Vaporwave
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(1f, 0.4f, 0.8f), new Color(0.4f, 0.8f, 1f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.2f, 0.1f, 0.3f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.magenta) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.cyan;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Vaporwave</color>]</color>";
                    break;
                case 25: // Ice
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.cyan, Color.white) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.white) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.cyan) };
                    textColors[0] = Color.black;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Ice</color>]</color>";
                    break;
                case 26: // Ember
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.red, Color.orange) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.orange) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Ember</color>]</color>";
                    break;
                case 27: // Toxic
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.green, Color.yellow) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.green) };
                    textColors[0] = Color.black;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Toxic</color>]</color>";
                    break;
                case 28: // Royal
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.35f, 0f, 0.5f), new Color(0.1f, 0f, 0.2f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.15f, 0f, 0.25f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.85f, 0.65f, 0.1f)) };
                    textColors[0] = new Color(1f, 0.85f, 0.3f);
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Royal</color>]</color>";
                    break;
                case 29: // Monochrome
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.white, Color.grey) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.grey) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    textColors[0] = Color.black;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Mono</color>]</color>";
                    break;
                case 30: // Crimson
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.5f, 0f, 0.1f), Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.8f, 0.1f, 0.2f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Crimson</color>]</color>";
                    break;
                case 31: // Aqua
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0f, 0.4f, 0.5f), new Color(0f, 0.1f, 0.2f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.cyan) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Aqua</color>]</color>";
                    break;
                case 32: // Discord
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.2f, 0.22f, 0.28f), new Color(0.08f, 0.09f, 0.12f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.12f, 0.13f, 0.16f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.3f, 0.55f, 0.9f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Discord</color>]</color>";
                    break;
                case 33: // Bubblegum
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(1f, 0.5f, 0.8f), new Color(0.6f, 0.2f, 0.5f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.3f, 0.1f, 0.25f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(1f, 0.4f, 0.7f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Bubblegum</color>]</color>";
                    break;
                case 34: // Emerald
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0f, 0.4f, 0.2f), Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.1f, 0.8f, 0.4f)) };
                    textColors[0] = new Color(0.6f, 1f, 0.7f);
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Emerald</color>]</color>";
                    break;
                case 35: // Copper
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.7f, 0.4f, 0.2f), new Color(0.2f, 0.1f, 0.05f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.15f, 0.08f, 0.04f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.9f, 0.5f, 0.2f)) };
                    textColors[0] = new Color(1f, 0.8f, 0.6f);
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Copper</color>]</color>";
                    break;
                case 36: // Galaxy
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.2f, 0f, 0.4f), new Color(0f, 0f, 0.15f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.05f, 0f, 0.1f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.6f, 0.3f, 1f)) };
                    textColors[0] = new Color(0.8f, 0.7f, 1f);
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Galaxy</color>]</color>";
                    break;
                case 37: // Peach
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(1f, 0.7f, 0.5f), new Color(1f, 0.4f, 0.4f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.5f, 0.2f, 0.2f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(1f, 0.55f, 0.4f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Peach</color>]</color>";
                    break;
                case 38: // Matrix
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0f, 0.1f, 0f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0f, 0.5f, 0f)) };
                    textColors[0] = new Color(0.2f, 1f, 0.2f);
                    textColors[1] = new Color(0.2f, 1f, 0.2f);
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Matrix</color>]</color>";
                    break;
                case 39: // Steel
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.5f, 0.55f, 0.6f), new Color(0.15f, 0.17f, 0.2f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.2f, 0.22f, 0.25f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.7f, 0.75f, 0.8f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Steel</color>]</color>";
                    break;
                case 40: // Mono Solid
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.12f, 0.12f, 0.12f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.18f, 0.18f, 0.18f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.75f, 0.75f, 0.75f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Mono Solid</color>]</color>";
                    break;
                case 41: // Amethyst
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.4f, 0.2f, 0.6f), new Color(0.1f, 0.05f, 0.2f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.12f, 0.06f, 0.2f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.7f, 0.4f, 1f)) };
                    textColors[0] = new Color(0.85f, 0.7f, 1f);
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Amethyst</color>]</color>";
                    break;
                case 42: // Coral
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(1f, 0.45f, 0.4f), new Color(0.5f, 0.15f, 0.2f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.3f, 0.1f, 0.12f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(1f, 0.5f, 0.45f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Coral</color>]</color>";
                    break;
                case 43: // Lime
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.5f, 0.8f, 0.1f), new Color(0.1f, 0.2f, 0f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.6f, 1f, 0.2f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Lime</color>]</color>";
                    break;
                case 44: // Wine
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.4f, 0.05f, 0.15f), new Color(0.1f, 0f, 0.03f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.08f, 0f, 0.02f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.6f, 0.1f, 0.25f)) };
                    textColors[0] = new Color(1f, 0.8f, 0.85f);
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Wine</color>]</color>";
                    break;
                case 45: // Sky
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.4f, 0.7f, 1f), new Color(0.1f, 0.3f, 0.6f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.05f, 0.15f, 0.35f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.5f, 0.8f, 1f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Sky</color>]</color>";
                    break;
                case 46: // Charcoal
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.06f, 0.06f, 0.06f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.1f, 0.1f, 0.1f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.9f, 0.3f, 0.1f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Charcoal</color>]</color>";
                    break;
                case 47: // Flamingo
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(1f, 0.3f, 0.6f), new Color(1f, 0.6f, 0.3f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.3f, 0.05f, 0.15f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(1f, 0.4f, 0.5f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Flamingo</color>]</color>";
                    break;
                case 48: // Deep Sea
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0f, 0.15f, 0.25f), Color.black) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0f, 0.6f, 0.7f)) };
                    textColors[0] = new Color(0.6f, 0.9f, 1f);
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Deep Sea</color>]</color>";
                    break;
                case 49: // Sand
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.8f, 0.7f, 0.45f), new Color(0.4f, 0.3f, 0.15f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.25f, 0.18f, 0.08f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.9f, 0.75f, 0.4f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Sand</color>]</color>";
                    break;
                case 50: // Violet Noir
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.15f, 0f, 0.2f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.1f, 0f, 0.15f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.7f, 0.2f, 0.9f)) };
                    textColors[0] = new Color(0.9f, 0.7f, 1f);
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Violet Noir</color>]</color>";
                    break;
                case 51: // Frost
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.8f, 0.9f, 1f), new Color(0.4f, 0.5f, 0.6f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.2f, 0.25f, 0.3f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.white) };
                    textColors[0] = Color.black;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Frost</color>]</color>";
                    break;
                case 52: // Magma
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.6f, 0.1f, 0f), new Color(0.1f, 0f, 0f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(1f, 0.5f, 0f)) };
                    textColors[0] = new Color(1f, 0.7f, 0.3f);
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Magma</color>]</color>";
                    break;
                case 53: // Jade
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0f, 0.5f, 0.4f), new Color(0f, 0.15f, 0.12f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0f, 0.1f, 0.08f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.2f, 0.85f, 0.7f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Jade</color>]</color>";
                    break;
                case 54: // Rose Gold
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.9f, 0.6f, 0.55f), new Color(0.4f, 0.2f, 0.2f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.25f, 0.12f, 0.12f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.95f, 0.7f, 0.6f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Rose Gold</color>]</color>";
                    break;
                case 55: // Void
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.02f, 0.02f, 0.04f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.05f, 0.05f, 0.08f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.4f, 0.1f, 0.6f)) };
                    textColors[0] = new Color(0.7f, 0.5f, 0.9f);
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Void</color>]</color>";
                    break;
                case 56: // Tangerine
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(1f, 0.55f, 0.1f), new Color(0.5f, 0.2f, 0f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.3f, 0.12f, 0f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(1f, 0.6f, 0.15f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Tangerine</color>]</color>";
                    break;
                case 57: // Cyberpunk
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.9f, 0f, 0.5f), new Color(0f, 0.7f, 0.8f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.05f, 0f, 0.1f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(1f, 0.9f, 0f)) };
                    textColors[0] = new Color(0f, 1f, 0.9f);
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Cyberpunk</color>]</color>";
                    break;
                case 58: // Olive
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.4f, 0.45f, 0.15f), new Color(0.12f, 0.14f, 0.05f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.08f, 0.1f, 0.03f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.6f, 0.7f, 0.2f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Olive</color>]</color>";
                    break;
                case 59: // Plasma
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.6f, 0f, 1f), new Color(1f, 0f, 0.4f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.1f, 0f, 0.15f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.8f, 0.2f, 1f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Plasma</color>]</color>";
                    break;
                case 60: // Espresso
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.25f, 0.15f, 0.08f), new Color(0.08f, 0.05f, 0.02f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.06f, 0.04f, 0.02f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.7f, 0.5f, 0.3f)) };
                    textColors[0] = new Color(0.95f, 0.85f, 0.7f);
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Espresso</color>]</color>";
                    break;
                case 61: // Electric
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0f, 0f, 0.1f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0f, 0f, 0.05f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0f, 0.7f, 1f)) };
                    textColors[0] = new Color(0.3f, 0.9f, 1f);
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Electric</color>]</color>";
                    break;
                case 62: // Blush
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(1f, 0.75f, 0.8f), new Color(0.7f, 0.4f, 0.5f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.35f, 0.15f, 0.2f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(1f, 0.6f, 0.7f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Blush</color>]</color>";
                    break;
                case 63: // Obsidian
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.04f, 0.04f, 0.06f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.08f, 0.08f, 0.1f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.2f, 0.6f, 0.9f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Obsidian</color>]</color>";
                    break;
                case 64: // Poison
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.3f, 0.6f, 0f), new Color(0.05f, 0.1f, 0f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.5f, 1f, 0f)) };
                    textColors[0] = new Color(0.7f, 1f, 0.3f);
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Poison</color>]</color>";
                    break;
                case 65: // Twilight
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(0.3f, 0.2f, 0.5f), new Color(0.8f, 0.4f, 0.5f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.1f, 0.05f, 0.2f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.7f, 0.4f, 0.8f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Twilight</color>]</color>";
                    break;
                case 66: // Snow
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.95f, 0.95f, 0.97f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.85f, 0.85f, 0.9f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.3f, 0.5f, 0.8f)) };
                    textColors[0] = Color.black;
                    textColors[1] = Color.white;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Snow</color>]</color>";
                    break;
                case 67: // Inferno
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(1f, 0.3f, 0f), new Color(0.4f, 0f, 0f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(1f, 0.7f, 0f)) };
                    textColors[0] = new Color(1f, 0.9f, 0.6f);
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Inferno</color>]</color>";
                    break;
                case 68: // Teal Noir
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0f, 0.12f, 0.13f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0f, 0.08f, 0.09f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0f, 0.7f, 0.65f)) };
                    textColors[0] = new Color(0.6f, 1f, 0.95f);
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Teal Noir</color>]</color>";
                    break;
                case 69: // Candy
                    backgroundColor = new ExtGradient { colors = ExtGradient.GetSimpleGradient(new Color(1f, 0.4f, 0.7f), new Color(0.4f, 0.6f, 1f)) };
                    buttonColors[0] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(0.2f, 0.1f, 0.3f)) };
                    buttonColors[1] = new ExtGradient { colors = ExtGradient.GetSolidGradient(new Color(1f, 0.7f, 0.9f)) };
                    textColors[0] = Color.white;
                    textColors[1] = Color.black;
                    GetIndex("Change Menu Theme").overlapText = "Change Menu Theme <color=grey>[<color=cyan>Candy</color>]</color>";
                    break;
            }
        }

        private static int fontstyle = 0;
        public static FontStyle currentStyle = FontStyle.Italic;
        public static void ChangeFontStyle(bool increment = true)
        {
            const int value = 4;
            if (increment)
            {
                fontstyle = (fontstyle + 1) % value;
            }
            else
            {
                fontstyle = (fontstyle - 1 + value) % value;
            }

            switch (fontstyle)
            {
                case 0:
                    currentStyle = FontStyle.Italic;
                    GetIndex("Change Font Style").overlapText = "Change Font Style <color=grey>[<color=cyan>Italic</color>]</color>";
                    break;
                case 1:
                    currentStyle = FontStyle.Bold;
                    GetIndex("Change Font Style").overlapText = "Change Font Style <color=grey>[<color=cyan>Bold</color>]</color>";
                    break;
                case 2:
                    currentStyle = FontStyle.Normal;
                    GetIndex("Change Font Style").overlapText = "Change Font Style <color=grey>[<color=cyan>Normal</color>]</color>";
                    break;
                case 3:
                    currentStyle = FontStyle.BoldAndItalic;
                    GetIndex("Change Font Style").overlapText = "Change Font Style <color=grey>[<color=cyan>Bold & Italic</color>]</color>";
                    break;
            }
        }

        private static int fonttype = 0;
        private static readonly string[] fontNames =
        {
            "Arial",
            "Comic Sans MS",
            "Chalkboard",
            "Segoe Print",
            "Verdana",
            "Tahoma",
            "Courier New",
            "Times New Roman"
        };
        public static Font currentFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        public static void ChangeFont(bool increment = true)
        {
            if (increment)
            {
                fonttype = (fonttype + 1) % fontNames.Length;
            }
            else
            {
                fonttype = (fonttype - 1 + fontNames.Length) % fontNames.Length;
            }

            switch (fonttype)
            {
                case 0:
                    currentFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    break;
                case 1:
                    currentFont = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 20);
                    break;
                case 2:
                    currentFont = Font.CreateDynamicFontFromOSFont("Chalkboard", 20);
                    break;
                case 3:
                    currentFont = Font.CreateDynamicFontFromOSFont("Segoe Print", 20);
                    break;
                case 4:
                    currentFont = Font.CreateDynamicFontFromOSFont("Verdana", 20);
                    break;
                case 5:
                    currentFont = Font.CreateDynamicFontFromOSFont("Tahoma", 20);
                    break;
                case 6:
                    currentFont = Font.CreateDynamicFontFromOSFont("Courier New", 20);
                    break;
                case 7:
                    currentFont = Font.CreateDynamicFontFromOSFont("Times New Roman", 20);
                    break;
            }
            GetIndex("Change Font").overlapText = $"Change Font <color=grey>[<color=cyan>{fontNames[fonttype]}</color>]</color>";
        }


        // Projectile Settings
        public static bool allowbigsnowballcolor = false;
        private static int currentprojectilecolor = 0;
        public static Color projectileColor = Color.white;
        private static string[] ProjectileThing =
        {
            "White", "Red", "Blue", "Black", "Yellow"
        };
        public static void ChangeProjectilesColor(bool increment = true)
        {
            const int value = 5;
            if (increment)
            {
                currentprojectilecolor = (currentprojectilecolor + 1) % value;
            }
            else
            {
                currentprojectilecolor = (currentprojectilecolor - 1 + value) % value;
            }

            switch (currentprojectilecolor)
            {
                case 0: projectileColor = Color.white; break;
                case 1: projectileColor = Color.red; break;
                case 2: projectileColor = Color.blue; break;
                case 3: projectileColor = Color.black; break;
                case 4: projectileColor = Color.yellow; break;
            }
            GetIndex("Change Projectile Color").overlapText = $"Change Projectile Color <color=grey>[<color=cyan>{ProjectileThing[currentprojectilecolor]}</color>]</color>";
        }

        private static int TagAuraDistanceIndex = 0;
        public static void ChangeTagAuraDistance(bool increment = true)
        {
            string[] ranges = new string[] { "Default", "Close", "Far" };
            if (increment)
            {
                TagAuraDistanceIndex = (TagAuraDistanceIndex + 1) % ranges.Length;
            }
            else
            {
                TagAuraDistanceIndex = (TagAuraDistanceIndex - 1 + ranges.Length) % ranges.Length;
            }

            switch (TagAuraDistanceIndex)
            {
                case 0: Advantage.TagAuraRange = 1.2f; break; // Default
                case 1: Advantage.TagAuraRange = 0.7f; break; // Close
                case 2: Advantage.TagAuraRange = 3f; break; // Far
            }
            GetIndex("Change Tag Aura Range").overlapText = $"Change Tag Aura Range <color=grey>[<color=cyan>{ranges[TagAuraDistanceIndex]}</color>]</color>";
        }

        private static int WallWalkAmountIndex = 0;
        public static void ChangeWallWalkAmount(bool increment = true)
        {
            float[] powers = new float[] { 8.8f, 9.8f, 10.8f, 11.8f, 6.8f, 7.8f };
            if (increment)
            {
                WallWalkAmountIndex = (WallWalkAmountIndex + 1) % powers.Length;
            }
            else
            {
                WallWalkAmountIndex = (WallWalkAmountIndex - 1 + powers.Length) % powers.Length;
            }

            Movement.WallWalkPower = powers[WallWalkAmountIndex];
            GetIndex("Change Wall Walk Power").overlapText = $"Change Wall Walk Power <color=grey>[<color=cyan>{powers[WallWalkAmountIndex]}f</color>]</color>";
        }

        // Movement Settings
        public static bool stickyplats = false;

        public static float wasdflyspeed = 10f;
        public static float flyspeed = 10f;
        public static int currentFlySpeed = 0;
        public static void ChangeFlySpeed(bool increment = true)
        {
            string[] speeds = { "Normal", "Very Slow", "Slow", "Fast", "Very Fast" };
            float[] values = { 10f, 2f, 5f, 20f, 40f };
            if (increment)
            {
                currentFlySpeed = (currentFlySpeed + 1) % speeds.Length;
            }
            else
            {
                currentFlySpeed = (currentFlySpeed - 1 + speeds.Length) % speeds.Length;
            }

            flyspeed = values[currentFlySpeed];
            GetIndex("Change Fly Speed").overlapText = $"Change Fly Speed <color=grey>[<color=cyan>{speeds[currentFlySpeed]}</color>]</color>";
        }

        public static float digsize = 3f;
        private static int currentDigSize = 0;
        public static void ChangeDigSize(bool increment = true)
        {
            string[] sizes = { "Normal", "Very Small", "Small", "Large", "Very Large" };
            float[] values = { 3f, 0.3f, 1f, 10f, 50f };
            if (increment)
            {
                currentDigSize = (currentDigSize + 1) % sizes.Length;
            }
            else
            {
                currentDigSize = (currentDigSize - 1 + sizes.Length) % sizes.Length;
            }
            digsize = values[currentDigSize];
            GetIndex("Change Dig Size").overlapText = $"Change Dig Size <color=grey>[<color=cyan>{sizes[currentDigSize]}</color>]</color>";
        }

        private static int currentFPS = 0;
        private static int currentPing = 0;

        public static void ChangeFPS(bool increment = true)
        {
            string[] names =
            {
                "1", "10", "20", "30", "40", "50", "60",
                "72", "90", "120", "144", "165", "240",
                "360", "500", "1000", "32767"
            };

            short[] values =
            {
                0, 10, 20, 30, 40, 50, 60,
                72, 90, 120, 144, 165, 240,
                360, 500, 1000, 32767
            };

            if (increment)
            {
                currentFPS = (currentFPS + 1) % values.Length;
            }
            else
            {
                currentFPS = (currentFPS - 1 + values.Length) % values.Length;
            }

            Arrakis.Patches.Patchers.GTPlayerStatsP.FPS = values[currentFPS];
            Arrakis.Patches.Patchers.GTPlayerStatsP.SpoofFPS = true;

            GetIndex("Change Spoofed Fps").overlapText = $"Change Spoofed Fps <color=grey>[<color=cyan>{names[currentFPS]}</color>]</color>";
        }

        public static void ChangePing(bool increment = true)
        {
            string[] names =
            {
                "1", "10", "20", "30", "40", "50",
                "75", "100", "150", "200", "300",
                "500", "1000", "32767"
            };

            short[] values =
            {
                1, 10, 20, 30, 40, 50,
                75, 100, 150, 200, 300,
                500, 1000, 32767
            };

            if (increment)
            {
                currentPing = (currentPing + 1) % values.Length;
            }
            else
            {
                currentPing = (currentPing - 1 + values.Length) % values.Length;
            }

            Arrakis.Patches.Patchers.GTPlayerStatsP.Ping = values[currentPing];
            Arrakis.Patches.Patchers.GTPlayerStatsP.SpoofPing = true;

            GetIndex("Change Spoofed Ping").overlapText = $"Change Spoofed Ping <color=grey>[<color=cyan>{names[currentPing]}</color>]</color>";
        }

        public class SavedSettings
        {
            public int clicksound { get; set; }
            public int buttonclickvolumeindex { get; set; }
            public int currentTheme { get; set; }
            public int fontstyle { get; set; }
            public int fonttype { get; set; }
            public int currentFlySpeed { get; set; }
            public int currentDigSize { get; set; }
            public int currentprojectilecolor { get; set; }
            public int GunLineindex { get; set; }
            public int AntiReportIndexRange { get; set; }
            public List<string> enabledMods { get; set; } = new List<string>();
            public List<string> favorites { get; set; } = new List<string>();
            public List<string> quickactions { get; set; } = new List<string>();
        }

        public static void SaveSettings()
        {
            Directory.CreateDirectory(PluginInfo.BaseDirectory);

            SavedSettings settings = new SavedSettings
            {
                clicksound = AudioManager.clicksound,
                buttonclickvolumeindex = buttonclickvolumeindex,
                currentTheme = currentTheme,
                fontstyle = fontstyle,
                fonttype = fonttype,
                currentFlySpeed = currentFlySpeed,
                currentDigSize = currentDigSize,
                currentprojectilecolor = currentprojectilecolor,
                GunLineindex = GunLineindex,
                AntiReportIndexRange = AntiReportIndexRange,
                enabledMods = Buttons.buttons.SelectMany(x => x).Where(x => x.enabled).Select(x => x.buttonText).ToList(),
                favorites = favorites,
                quickactions = quickactions
            };
            File.WriteAllText(Path.Combine(PluginInfo.BaseDirectory, "SavedSettings.json"), JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static void LoadSettings()
        {
            NotificationManager.ClearAllNotifications();

            string path = Path.Combine(PluginInfo.BaseDirectory, "SavedSettings.json");
            if (!File.Exists(path))
                return;
            try
            {
                SavedSettings settings = JsonConvert.DeserializeObject<SavedSettings>(File.ReadAllText(path));
                if (settings == null)
                    return;
                AudioManager.clicksound = settings.clicksound - 1;
                AudioManager.ChangeClickSound();

                buttonclickvolumeindex = settings.buttonclickvolumeindex - 1;
                ChangeButtonClickVolume();

                currentTheme = settings.currentTheme - 1;
                ChangeMenuTheme();

                fontstyle = settings.fontstyle - 1;
                ChangeFontStyle();

                fonttype = settings.fonttype - 1;
                ChangeFont();

                currentFlySpeed = settings.currentFlySpeed - 1;
                ChangeFlySpeed();

                currentDigSize = settings.currentDigSize - 1;
                ChangeDigSize();

                currentprojectilecolor = settings.currentprojectilecolor - 1;
                ChangeProjectilesColor();

                GunLineindex = settings.GunLineindex - 1;
                ChangeGunline();

                AntiReportIndexRange = settings.AntiReportIndexRange - 1;
                ChangeAntiReportRange();

                HashSet<string> enabled = settings.enabledMods.ToHashSet();
                foreach (ButtonInfo button in Buttons.buttons.SelectMany(x => x))
                {
                    bool shouldBeEnabled = enabled.Contains(button.buttonText);
                    if (button.enabled != shouldBeEnabled)
                        Toggle(button.buttonText);
                }
                favorites.Clear();
                foreach (var fav in settings.favorites)
                    favorites.Add(fav);

                quickactions.Clear();
                foreach (var quick in settings.quickactions)
                    quickactions.Add(quick);
            }
            catch (Exception ex)
            {
                CustomConsole.Log($"Failed to load settings: {ex}", CustomConsole.LogType.Warning);
            }
        }
    }
}