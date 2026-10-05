/*
 * Arrakis | Classes/Server/Admin.cs
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

using Arrakis.Mods;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static Arrakis.Classes.RigManager;
using static Arrakis.Menu.Main;

namespace Arrakis.Classes.Menu
{
    public delegate void AdminCommandHandler(AdminContext ctx);
    public readonly struct AdminContext
    {
        public readonly Photon.Realtime.Player Sender;
        public readonly object[] Args;
        public readonly string AdminName;
        public readonly bool IsHighAdmin;

        public AdminContext(Photon.Realtime.Player sender, object[] args, string adminName, bool isHighAdmin)
        {
            Sender = sender;
            Args = args ?? new object[0];
            AdminName = adminName;
            IsHighAdmin = isHighAdmin;
        }

        public int Count => Args.Length;
        public bool Has(int index) => index >= 0 && index < Args.Length;
        public T Get<T>(int index) => Has(index) && Args[index] is T v ? v : default;
        public string Str(int index) => Get<string>(index);
        public Vector3 Vec(int index) => Get<Vector3>(index);
        public NetPlayer TargetById(int index) => GetPlayerFromID(Str(index));
    }

    public class Admin : MonoBehaviour
    {
        public static Admin Instance;
        public static byte adminbyte = 68;

        private class AdminCommand
        {
            public string Name;
            public AdminCommandHandler Handler;
            public bool RequireHighAdmin;
            public bool RequireSenderAdmin;
        }

        private static readonly Dictionary<string, AdminCommand> Commands =
            new Dictionary<string, AdminCommand>(System.StringComparer.OrdinalIgnoreCase);
        public static void RegisterCommand(string name, AdminCommandHandler handler, bool requireHighAdmin = false, bool requireSenderAdmin = true)
        {
            if (string.IsNullOrEmpty(name) || handler == null) return;
            Commands[name] = new AdminCommand
            {
                Name = name,
                Handler = handler,
                RequireHighAdmin = requireHighAdmin,
                RequireSenderAdmin = requireSenderAdmin
            };
        }
        public static bool UnregisterCommand(string name) => 
            Commands.Remove(name);

        public void Awake()
        {
            Instance = this;
            RegisterDefaults();
            PhotonNetwork.NetworkingClient.EventReceived += EventReceived;
        }

        private float loadtime = 0f;
        private float reload = 0f;
        private int attempts = 0;
        public void Update()
        {
            if (loadtime > 0f && Time.time > loadtime && GorillaComputer.instance.isConnectedToMaster)
            {
                loadtime = Time.time + 5f;
                attempts++;
                if (attempts >= 4)
                {
                    CustomConsole.Log("Unable to load admin data.", CustomConsole.LogType.Info);
                    loadtime = -1;
                    return;
                }
                Instance.StartCoroutine(GetAdminData());
            }

            if (reload > 0f)
            {
                if (Time.time > reload)
                {
                    reload = Time.time + 30f;
                    Instance.StartCoroutine(GetAdminData());
                }
            }
            else
            {
                if (GorillaComputer.instance.isConnectedToMaster)
                    reload = Time.time + 5f;
            }
        }

        private void EventReceived(EventData eventdata)
        {
            try
            {
                if (eventdata.Code != adminbyte) return;
                Photon.Realtime.Player sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(eventdata.Sender);
                object[] args = eventdata.CustomData == null ? new object[] { } : (object[])eventdata.CustomData;
                string command = args.Length > 0 ? (string)args[0] : "";
                HandleConsoleEvent(sender, args, command);
            }
            catch { }
        }

        private static readonly Dictionary<VRRig, float> confirmUsingDelay = new Dictionary<VRRig, float>();
        public static readonly Dictionary<Player, (string, string)> userDictionary = new Dictionary<Player, (string, string)>();
        public static float indicatorDelay = 0f;

        private static void HandleConsoleEvent(Photon.Realtime.Player sender, object[] args, string command)
        {
            if (string.IsNullOrEmpty(command)) return;
            if (!Commands.TryGetValue(command, out var cmd)) return;
            bool senderIsAdmin = Admins.TryGetValue(sender.UserId, out var administrator);
            bool highadmin = senderIsAdmin && HigherAdmins.Contains(administrator);
            if (cmd.RequireSenderAdmin && !senderIsAdmin) return;
            if (cmd.RequireHighAdmin && !highadmin) return;
            var ctx = new AdminContext(sender, args, administrator, highadmin);
            try
            {
                cmd.Handler(ctx);
            }
            catch (System.Exception e)
            {
                CustomConsole.Log($"Admin command '{command}' failed: {e.Message}", CustomConsole.LogType.Error);
            }
        }

        private static bool defaultsRegistered = false;
        private static void RegisterDefaults()
        {
            if (defaultsRegistered) return;
            defaultsRegistered = true;

            RegisterCommand("kick", ctx =>
            {
                NetPlayer target = ctx.TargetById(1);
                SpawnBeacon(GetVRRigFromPlayer(target).headMesh.transform.position, 1f);
                if (!Admins.ContainsKey(target.UserId) || ctx.IsHighAdmin)
                {
                    if (ctx.Str(1) == PhotonNetwork.LocalPlayer.UserId)
                        NetworkSystem.Instance.ReturnToSinglePlayer();
                }
            });

            RegisterCommand("kickall", ctx =>
            {
                foreach (Photon.Realtime.Player plr in Admins.ContainsKey(PhotonNetwork.LocalPlayer.UserId) ? PhotonNetwork.PlayerListOthers : PhotonNetwork.PlayerList)
                    SpawnBeacon(GetVRRigFromPlayer(plr).headMesh.transform.position, 1f);
                if (!Admins.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                    NetworkSystem.Instance.ReturnToSinglePlayer();
            });

            RegisterCommand("bring", ctx =>
            {
                Vector3 pos = ctx.Vec(1);
                GTPlayer.Instance.TeleportTo(pos, GTPlayer.Instance.transform.rotation, true);
                VRRig.LocalRig.transform.position = pos;
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            });

            RegisterCommand("restoremicrophone", ctx =>
            {
                Soundboard.RestoreMicrophone();
            });

            RegisterCommand("isusing", ctx =>
            {
                ExecuteCommand("confirmusing", ctx.Sender.ActorNumber);
            });

            RegisterCommand("lightningstrike", ctx =>
            {
                SpawnBeacon(ctx.Vec(1), 2f);
            });

            RegisterCommand("confirmusing", ctx =>
            {
                if (!Admins.ContainsKey(PhotonNetwork.LocalPlayer.UserId)) return;
                if (indicatorDelay <= Time.time) return;

                VRRig vrrig = GetVRRigFromPlayer(ctx.Sender);
                if (confirmUsingDelay.TryGetValue(vrrig, out float delay))
                {
                    if (Time.time < delay) return;
                    confirmUsingDelay.Remove(vrrig);
                }
                confirmUsingDelay.Add(vrrig, Time.time + 5f);
                userDictionary[vrrig.Creator.GetPlayerRef()] = (ctx.Str(1), ctx.Str(2));
                CommunicateConsole("confirmusing", ctx.Sender.ActorNumber, ctx.Str(1), ctx.Str(2));
                SpawnBeacon(GetVRRigFromPlayer(GetNetPlayerFromID(ctx.Sender.UserId)).headMesh.transform.position, 1f);
            }, requireSenderAdmin: false);
        }

        public static void CommunicateConsole(string command, int id, params object[] args)
        {
            string eventName = $"%ARRAKIS%Syncer||{command}";
            if (args.Length > 0)
                eventName += $"||{string.Join("||", args)}";
            PlayerGameEvents.MiscEvent(eventName, id);
        }
        public static void ExecuteCommand(string command, RaiseEventOptions options, params object[] parameters)
        {
            if (!NetworkSystem.Instance.InRoom)
                return;
            if (options.Receivers == ReceiverGroup.All || (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber)))
            {
                if (options.Receivers == ReceiverGroup.All)
                    options.Receivers = ReceiverGroup.Others;
                if (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber))
                    options.TargetActors = options.TargetActors.Where(id => id != NetworkSystem.Instance.LocalPlayer.ActorNumber).ToArray();
                HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { command }.Concat(parameters).ToArray(), command);
            }
            PhotonNetwork.RaiseEvent(adminbyte, new object[] { command }.Concat(parameters).ToArray(), options, SendOptions.SendReliable);
        }
        public static void ExecuteCommand(string command, int[] targets, params object[] parameters) =>
            ExecuteCommand(command, new RaiseEventOptions { TargetActors = targets }, parameters);
        public static void ExecuteCommand(string command, int target, params object[] parameters) =>
            ExecuteCommand(command, new RaiseEventOptions { TargetActors = new[] { target } }, parameters);
        public static void ExecuteCommand(string command, ReceiverGroup target, params object[] parameters) =>
            ExecuteCommand(command, new RaiseEventOptions { Receivers = target }, parameters);

        private static void SpawnBeacon(Vector3 pos, float destroyDelay = 0.5f)
        {
            GameObject holder = new GameObject();
            LineRenderer line = holder.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.material.shader = Shader.Find("GUI/Text Shader");
            line.positionCount = 2;
            line.startWidth = 0.3f;
            line.endWidth = 0.3f;
            VRRig.LocalRig.PlayHandTapLocal(68, false, 0.25f);
            VRRig.LocalRig.PlayHandTapLocal(68, true, 0.25f);
            line.startColor = new Color(0.7f, 0.5f, 0.7f, 1f);
            line.endColor = new Color(0.7f, 0.5f, 0.7f, 1f);
            line.SetPosition(0, pos + new Vector3(0f, 999f, 0f));
            line.SetPosition(1, pos - new Vector3(0f, 999f, 0f));
            GameObject.Destroy(holder, destroyDelay);
        }

        public static readonly Dictionary<string, string> Admins = new Dictionary<string, string>();
        public static readonly List<string> HigherAdmins = new List<string>();
        private static bool gaveadminmods = false;
        private IEnumerator GetAdminData()
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{PluginInfo.ServerApi}/admins"))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    CustomConsole.Log($"Request failed: {request.error}", CustomConsole.LogType.Error);
                    yield break;
                }
                Admins.Clear();
                HigherAdmins.Clear();
                JObject json = JObject.Parse(request.downloadHandler.text);
                foreach (JObject admin in (JArray)json["Admins"])
                {
                    string name = admin["Name"]?.ToString();
                    string userId = admin["UserId"]?.ToString();
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(userId))
                        Admins[userId] = name;
                }
                foreach (JToken token in (JArray)json["HigherAdmins"])
                    HigherAdmins.Add(token.ToString());

                if (!gaveadminmods && PhotonNetwork.LocalPlayer.UserId != null && Admins.TryGetValue(PhotonNetwork.LocalPlayer.UserId, out var administrator))
                {
                    gaveadminmods = true;
                    SetUpAdminPanel(administrator);
                }
            }
        }

        public void OnDisable() =>
            PhotonNetwork.NetworkingClient.EventReceived -= EventReceived;
    }
}