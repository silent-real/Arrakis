/*
 * Arrakis | Mods/Safety.cs
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
using Arrakis.Extensions;
using Arrakis.Menu;
using Arrakis.Notifications;
using Arrakis.Patches.Patchers;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Arrakis.Menu.Main;
using static Arrakis.Settings;

namespace Arrakis.Mods
{
    public class Safety
    {
        private static GameObject _radiusSphere;

        public static void AntiReportDisconnect()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                HandleAntiReportRadius(AntiReportRange);
                HandleAntiReport(() =>
                {
                    NetworkSystem.Instance.ReturnToSinglePlayer();
                });
            }
        }

        public static void AntiReportReconnect()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                HandleAntiReportRadius(AntiReportRange);
                HandleAntiReport(() =>
                {
                    string roomname = PhotonNetwork.CurrentRoom.Name;
                    NetworkSystem.Instance.ReturnToSinglePlayer();
                    PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomname, JoinType.Solo);
                });
            }
        }

        public static void AntiReportJoinRandom()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                HandleAntiReportRadius(AntiReportRange);
                HandleAntiReport(() =>
                {
                    NetworkSystem.Instance.ReturnToSinglePlayer();
                    PhotonNetworkController.Instance.AttemptToJoinPublicRoom(PhotonNetworkController.Instance.currentJoinTrigger);
                });
            }
        }

        private static void HandleAntiReportRadius(float radius)
        {
            if (disableAntiReportVisualizer)
            {
                DestroyARSphere();
                return;
            }

            Vector3 position = GorillaScoreboardTotalUpdater.allScoreboardLines.Where(x => x.linePlayer == NetworkSystem.Instance.LocalPlayer).Select(x => x.reportButton.transform.position)
            .FirstOrDefault();
            if (_radiusSphere == null)
            {
                _radiusSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GameObject.Destroy(_radiusSphere.GetComponent<SphereCollider>());
            }
            _radiusSphere.GetComponent<Renderer>().material.color = Settings.backgroundColor.GetCurrentColor();
            _radiusSphere.transform.localScale = new Vector3(radius, radius, radius);
            _radiusSphere.transform.position = position;
        }

        public static void DestroyARSphere()
        {
            if (_radiusSphere != null)
            {
                GameObject.Destroy(_radiusSphere);
                _radiusSphere = null;
            }
        }

        private static void HandleAntiReport(Action method)
        {
            if (method == null) return;
            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines.Where(line => line.linePlayer == NetworkSystem.Instance.LocalPlayer))
            {
                Transform reportButton = line.reportButton.transform;
                bool triggered = false;
                foreach (VRRig rig in VRRigCache.ActiveRigs.Where(rig => rig != null && rig != VRRig.LocalRig))
                {
                    if (triggered) break;
                    float disR = Vector3.Distance(reportButton.position, rig.rightHandTransform.position);
                    float disL = Vector3.Distance(reportButton.position, rig.leftHandTransform.position);
                    if (disR < AntiReportRange || disL < AntiReportRange)
                    {
                        triggered = true;
                        NotificationManager.SendNotification($"<color=yellow>[ANTIREPORT]</color> {rig.Creator.NickName} Attempted to report you.");
                        method.Invoke();
                    }
                }
            }
        }

        public static void Panic(bool force = false)
        {
            if (force)
            {
                foreach (ButtonInfo[] b in Buttons.buttons)
                {
                    foreach (ButtonInfo v in b)
                    {
                        if (v.enabled)
                            Toggle(v.buttonText);
                    }
                }
            }
            else
            {
                if (Settings.panicPrompt)
                {
                    Prompt("Would you like to disable all your mods ?", () =>
                    {
                        foreach (ButtonInfo[] b in Buttons.buttons)
                        {
                            foreach (ButtonInfo v in b)
                            {
                                if (v.enabled)
                                    Toggle(v.buttonText);
                            }
                        }
                    });
                }
                else
                {
                    foreach (ButtonInfo[] b in Buttons.buttons)
                    {
                        foreach (ButtonInfo v in b)
                        {
                            if (v.enabled)
                                Toggle(v.buttonText);
                        }
                    }
                }
            }
        }

        public static void NoFingerMovement()
        {
            ControllerInputPoller.instance.rightControllerPrimaryButton = false;
            ControllerInputPoller.instance.rightControllerPrimaryButtonTouch = false;
            ControllerInputPoller.instance.rightControllerSecondaryButton = false;
            ControllerInputPoller.instance.rightControllerSecondaryButtonTouch = false;
            ControllerInputPoller.instance.rightGrab = false;
            ControllerInputPoller.instance.rightControllerGripFloat = 0f;
            ControllerInputPoller.instance.rightControllerTriggerButton = false;
            ControllerInputPoller.instance.rightControllerIndexFloat = 0f;

            ControllerInputPoller.instance.leftControllerPrimaryButton = false;
            ControllerInputPoller.instance.leftControllerPrimaryButtonTouch = false;
            ControllerInputPoller.instance.leftControllerSecondaryButton = false;
            ControllerInputPoller.instance.leftControllerSecondaryButtonTouch = false;
            ControllerInputPoller.instance.leftGrab = false;
            ControllerInputPoller.instance.leftControllerGripFloat = 0f;
            ControllerInputPoller.instance.leftControllerTriggerButton = false;
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
        }

        public static void RPCProc()
        {
            MonkeAgent.instance.rpcErrorMax = int.MaxValue;
            MonkeAgent.instance.rpcCallLimit = int.MaxValue;
            MonkeAgent.instance.logErrorMax = int.MaxValue;
            MonkeAgent.instance.userRPCCalls.Clear();
            PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
            PhotonNetwork.QuickResends = int.MaxValue;
            PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
            PhotonNetwork.OpCleanRpcBuffer(GorillaTagger.Instance.myVRRig.GetView);
            PhotonNetwork.RemoveBufferedRPCs(GorillaTagger.Instance.myVRRig.ViewID, null, null);
            PhotonNetwork.RemoveRPCsInGroup(int.MaxValue);
            PhotonNetwork.SendAllOutgoingCommands();
            MonkeAgent.instance.OnPlayerLeftRoom(PhotonNetwork.LocalPlayer);
        }
        public static void BypassVCBan()
        {
            GorillaTagger.moderationMutedTime = -1f;
            GorillaTelemetry.PostNotificationEvent("Unmute");
            GorillaTagger.Instance.myRecorder.TransmitEnabled = true;
            if (KIDManager.Instance != null)
            {
                GameObject.Destroy(KIDManager.Instance);
            }
        }

        public static void AntiModerator()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (!rig.IsLocal())
                    {
                        if (rig._playerOwnedCosmetics.Concat().Contains("LBAAK.") || rig._playerOwnedCosmetics.Concat().Contains("LMAPY.") || rig._playerOwnedCosmetics.Concat().Contains("LBAAD."))
                        {
                            NotificationManager.SendNotification($"<color=yellow>[ANTIREPORT]</color> {rig.Creator.NickName} has one of the sticks or admin badge there id has been saved to a file.");
                            Color pc = rig.playerColor;
                            string content = $"RoomName: {PhotonNetwork.CurrentRoom.Name}\nNickName: {rig.Creator.NickName}\nUserID: {rig.Creator.UserId}\nColor: {string.Format("R: {0}, G: {1}, B: {2}", pc.r * 9, pc.g * 9, pc.b * 9)}";
                            File.WriteAllText($"{PluginInfo.BaseDirectory}/Antis/Staff/{rig.Creator.NickName}.txt", content);
                            NetworkSystem.Instance.ReturnToSinglePlayer();
                        }
                    }
                }
            }
        }

        public static void AntiContentCreator()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (!rig.IsLocal())
                    {
                        if (rig._playerOwnedCosmetics.Concat().Contains("LBADE.") || rig._playerOwnedCosmetics.Concat().Contains("LBAGS.") || rig._playerOwnedCosmetics.Concat().Contains("LBANI."))
                        {
                            NotificationManager.SendNotification($"<color=yellow>[ANTIREPORT]</color> {rig.Creator.NickName} has the moderatorstick there id has been saved to a file.");
                            Color pc = rig.playerColor;
                            string content = $"RoomName: {PhotonNetwork.CurrentRoom.Name}\nNickName: {rig.Creator.NickName}\nUserID: {rig.Creator.UserId}\nColor: {string.Format("R: {0}, G: {1}, B: {2}", pc.r * 9, pc.g * 9, pc.b * 9)}";
                            File.WriteAllText($"{PluginInfo.BaseDirectory}/Antis/ContentCreators/{rig.Creator.NickName}.txt", content);
                            NetworkSystem.Instance.ReturnToSinglePlayer();
                        }
                    }
                }
            }
        }

        private static float lastRpcClear = 0f;
        private static float rpcClearInterval = 5f;
        public static void AntiCrash() // nerd shit no clue if it works but blehhh :33 -sleepy
        {
            try
            {
                if (Time.time > lastRpcClear + rpcClearInterval)
                {
                    if (PhotonNetwork.NetworkingClient != null)
                    {
                        var peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
                        var outgoingQueueField = peer.GetType().GetField("outgoingStreamQueue", BindingFlags.Instance | BindingFlags.NonPublic);
                        var queue = outgoingQueueField?.GetValue(peer) as System.Collections.IList;
                        if (queue != null && queue.Count > 1000)
                        {
                            queue.Clear();
                            CustomConsole.Log("Cleared outgoing RPC queue to prevent crash", CustomConsole.LogType.Info);
                        }
                    }
                    lastRpcClear = Time.time;
                }
                if (GorillaTagger.Instance == null || GorillaTagger.Instance.myVRRig == null)
                    return;
            }
            catch { }
        }
        public static bool spopofing; // best name ever please dont change it -sleepy
        public static void SpoofPlatform(bool enabled)
        {
            spopofing = enabled;
            try
            {
                VRRig.LocalRig.netView.SendRPC("RPC_UpdateRankedInfo", RpcTarget.Others, 0, enabled ? 1 : 0, enabled ? 0 : 1);
            }
            catch { }
        }

        private static float spoofdelay = 0f;
        public static void SpoofSupportPage()
        {
            if (Time.time > spoofdelay)
            {
                GorillaComputer.instance.screenText.Set(GorillaComputer.instance.screenText.stringBuilder.ToString().Replace("STEAM", "QUEST").Replace(GorillaComputer.instance.buildDate, $"{GorillaComputer.instance.buildDate}\nBUILD CODE 4893\nMANAGED ACCOUNT: NO"));
                spoofdelay = Time.time + 0.1f;
            }
        }

        private static float AntiMemoryLeakDelay;
        public static void AntiMemoryLeak()
        {
            if (Time.time > AntiMemoryLeakDelay)
            {
                GC.Collect();
                AntiMemoryLeakDelay = Time.time + 45f;
            }
        }
        private static string originalName;
        private static float SpoofDelay;
        public static void BoardSpoof()
        {
            if (Time.time > SpoofDelay)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GorillaComputer.instance.friendJoinCollider.transform.position;

                if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
                {
                    originalName = PhotonNetwork.LocalPlayer.NickName;

                    foreach (NetPlayer target in NetworkSystem.Instance.PlayerListOthers)
                    {
                        var color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                        string name = Important.GenerateRandomString(UnityEngine.Random.Range(0, 14));
                        PhotonNetwork.LocalPlayer.NickName = name;

                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", target, new object[] { color.r, color.g, color.b });
                    }

                    VRRig.LocalRig.enabled = true;
                    PhotonNetwork.LocalPlayer.NickName = originalName;
                }
                SpoofDelay = Time.time + 30f;
            }
        }
        public static void NukeModCheckers()
        {
            FPSPatch.enabled = true;
            FPSPatch.spoofFPSValue = 255;
            Hashtable props = new Hashtable();
            foreach (string mod in modDictionary.Keys)
                props[mod] = true;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
        public static readonly Dictionary<string, string> modDictionary = new Dictionary<string, string> { // creds to Seralyth for the list ^-^
            { "genesis", "Genesis" },
            { "HP_Left", "Holdable Pad" },
            { "GrateVersion", "Grate" },
            { "void", "Void" },
            { "BANANAOS", "Banana OS" },
            { "GC", "Gorilla Craft" },
            { "CarName", "Gorilla Vehicles" },
            { "6p72ly3j85pau2g9mda6ib8px", "CCM V2" },
            { "FPS-Nametags for Zlothy", "FPS Tags" },
            { "cronos", "Cronos" },
            { "ORBIT", "Orbit" },
            { "Violet On Top", "Violet" },
            { "MP25", "Monke Phone" },
            { "GorillaWatch", "Gorilla Watch" },
            { "InfoWatch", "Gorilla Info Watch" },
            { "BananaPhone", "Banana Phone" },
            { "Vivid", "Vivid" },
            { "RGBA", "Custom Cosmetics" },
            { "cheese is gouda", "Whos Icheating" },
            { "shirtversion", "Gorilla Shirts" },
            { "gpronouns", "Gorilla Pronouns" },
            { "gfaces", "Gorilla Faces" },
            { "monkephone", "Monke Phone" },
            { "pmversion", "Player Models" },
            { "gtrials", "Gorilla Trials" },
            { "msp", "Monke Smartphone" },
            { "gorillastats", "Gorilla Stats" },
            { "using gorilladrift", "Gorilla Drift" },
            { "monkehavocversion", "Monke Havoc" },
            { "tictactoe", "Tic Tac Toe" },
            { "ccolor", "Index" },
            { "imposter", "Gorilla Among Us" },
            { "spectapeversion", "Spec Tape" },
            { "cats", "Cats" },
            { "made by biotest05 :3", "Dogs" },
            { "fys cool magic mod", "Fys Magic Mod" },
            { "colour", "Custom Cosmetics" },
            { "chainedtogether", "Chained Together" },
            { "goofywalkversion", "Goofy Walk" },
            { "void_menu_open", "Void" },
            { "violetpaiduser", "Violet Paid" },
            { "violetfree", "Violet Free" },
            { "obsidianmc", "Obsidian.Lol" },
            { "dark", "Shiba GT Dark" },
            { "hidden menu", "Hidden" },
            { "oblivionuser", "Oblivion" },
            { "eyerock reborn", "Eye Rock" },
            { "asteroidlite", "Asteroid Lite" },
            { "elux", "Elux" },
            { "cokecosmetics", "Coke Cosmetx" },
            { "GFaces", "G Faces" },
            { "github.com/maroon-shadow/SimpleBoards", "Simple Boards" },
            { "ObsidianMC", "Obsidian" },
            { "GTrials", "G Trials" },
            { "github.com/ZlothY29IQ/GorillaMediaDisplay", "Gorilla Media Display" },
            { "github.com/ZlothY29IQ/TooMuchInfo", "Too Much Info" },
            { "github.com/ZlothY29IQ/RoomUtils-IW", "Room Utils IW" },
            { "github.com/ZlothY29IQ/MonkeClick", "Monke Click" },
            { "github.com/ZlothY29IQ/MonkeClick-CI", "Monke Click CI" },
            { "github.com/ZlothY29IQ/MonkeRealism", "Monke Realism" },
            { "MediaPad", "Media Pad" },
            { "GorillaCinema", "Gorilla Cinema" },
            { "ChainedTogetherActive", "Chained Together" },
            { "GPronouns", "G Pronouns" },
            { "CSVersion", "Custom Skin" },
            { "github.com/ZlothY29IQ/Zloth-RecRoomRig", "Zloth Rec Room Rig" },
            { "ShirtProperties", "Shirts Old" },
            { "GorillaShirts", "Shirts" },
            { "GS", "Old Shirts" },
            { "6XpyykmrCthKhFeUfkYGxv7xnXpoe2", "CCM V2" },
            { "Body Tracking", "Body Track Old" },
            { "Body Estimation", "Han Body Est" },
            { "Gorilla Track", "Body Track" },
            { "CustomMaterial", "Custom Cosmetics" },
            { "I like cheese", "Rec Room Rig" },
            { "EmoteWheel", "Fortnite Emote Wheel" },
            { "untitled", "Untitled" },
            { "BoyDoILoveInformation Public", "BoyDoILoveInformation" },
            { "DTAOI", "DTAOI" },
            { "GorillaShop", "GorillaShop" },
            { "Fusioned", "Fusioned" },
            { "y u lookin in here weirdo", "Malachi Menu Reborn" },
            { "ØƦƁƖƬ", "Orbit" },
            { "Atlas", "Atlas" },
            { "𓂀𓆣𓋹𓏏𓇋⚚⚛⚡☯☢☣☠♛♚♜♞♟✶✷✸✹✺✻✼✽✾✿❀❁❂❃❄❅❆❇❈❉❊❋⟁⟆⟐⟡⟢⟣⟤⟥⟦⟧⟨⟩⟪⟫⟬⟭⟮⟯⟰⟱⟲⟳⟴⟵⟶", "𓂀𓆣𓋹𓏏𓇋⚚⚛⚡☯☢☣☠♛♚♜♞♟✶✷✸✹✺✻✼✽✾✿❀❁❂❃❄❅❆❇❈❉❊❋⟁⟆⟐⟡⟢⟣⟤⟥⟦⟧⟨⟩⟪⟫⟬⟭⟮⟯⟰⟱⟲⟳⟴⟵⟶" }
        };
        public static void AntiOverlay() =>
            GTPlayer.Instance.inOverlay = false;
    }
}