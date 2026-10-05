/*
 * Arrakis | Mods/Experimental.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arrakis.Classes;
using Arrakis.Classes.Menu;
using Arrakis.Extensions;
using Arrakis.Managers;
using Arrakis.Patches.Patchers;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaNetworking;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR;
using static Arrakis.Classes.RigManager;
using static Arrakis.Menu.Main;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Arrakis.Mods
{
    public class Experimental
    {
        public static void GetRPCData() =>
            File.WriteAllLines($"{PluginInfo.BaseDirectory}/RPCData.txt", PhotonNetwork.PhotonServerSettings.RpcList);

        public static void GetTryonCosmetics()
        {
            foreach (CosmeticsController.CosmeticItem item in CosmeticsController.instance.allCosmetics)
            {
                if (item.canTryOn && item.cost != 0)
                {
                    CustomConsole.Log($"Found all items {(item.itemName != "" ? item.itemName : item.overrideDisplayName)} , {item.appliedCosmeticPlayFabID} , {item.cost}", CustomConsole.LogType.Info);
                }
            }
        }
        private static bool antiKickEvents;
        public static bool AntiKickEvents // kinda works? -sleepy
        {
            get
            {
                return antiKickEvents;
            }
            set
            {
                if (antiKickEvents == value)
                    return;
                antiKickEvents = value;
                if (value)
                {
                    PhotonNetwork.SerializationRate = 2;
                    AntiEventThingIgIdfk.FilteredRPCs["OnHandTapRPC"] = () => false;
                    AntiEventThingIgIdfk.FilteredRPCs["RPC_UpdateCosmeticsWithTryonPacked"] = () => false;

                    EventPatches.Override = delegate
                    {
                        SendSerialize(VRRig.LocalRig.netView.GetView);
                        return true;
                    };
                }
                else
                {
                    PhotonNetwork.SerializationRate = 10;
                    EventPatches.Override = null;
                    AntiEventThingIgIdfk.FilteredRPCs.Remove("OnHandTapRPC");
                    AntiEventThingIgIdfk.FilteredRPCs.Remove("RPC_UpdateCosmeticsWithTryonPacked");
                }
            }
        }
        public static void SendSerialize(PhotonView view, RaiseEventOptions options = null, int timeOffset = 0, float delay = 0f) // used chatgpt to clean up this shit of a function, it looked so ass before -sleepy
        {
            if (!NetworkSystem.Instance.InRoom)
                return;
            if (view == null)
            {
                CustomConsole.Log("PhotonView was null.", CustomConsole.LogType.Error);
                return;
            }
            List<object> data = PhotonNetwork.OnSerializeWrite(view);
            if (data == null || data.Count == 0)
                return;
            bool reliable = view.Synchronization == ViewSynchronization.ReliableDeltaCompressed || view.mixedModeIsReliable;
            var batchKey = new PhotonNetwork.RaiseEventBatch
            {
                Reliable = reliable,
                Group = view.Group
            };
            var batches = PhotonNetwork.serializeViewBatches;
            if (!batches.ContainsKey(batchKey))
            {
                batches.Add(batchKey, new PhotonNetwork.SerializeViewBatch(batchKey, 2));
            }
            PhotonNetwork.SerializeViewBatch batch = (PhotonNetwork.SerializeViewBatch)batches[batchKey];
            batch.Add(data);
            RaiseEventOptions raiseOptions;
            if (options == null)
            {
                raiseOptions = PhotonNetwork.serializeRaiseEvOptions;
            }
            else
            {
                raiseOptions = new RaiseEventOptions
                {
                    CachingOption = PhotonNetwork.serializeRaiseEvOptions.CachingOption,
                    Flags = PhotonNetwork.serializeRaiseEvOptions.Flags,
                    InterestGroup = PhotonNetwork.serializeRaiseEvOptions.InterestGroup,
                    Receivers = options.Receivers,
                    TargetActors = options.TargetActors
                };
            }
            List<object> payload = batch.ObjectUpdates;
            payload[0] = PhotonNetwork.ServerTimestamp + timeOffset;
            payload[1] = PhotonNetwork.currentLevelPrefix == 0 ? null : (object)PhotonNetwork.currentLevelPrefix;
            byte eventCode = (byte)(reliable ? Photon.Pun.PunEvent.SendSerializeReliable : Photon.Pun.PunEvent.SendSerialize);
            SendOptions send = reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable;
            if (delay > 0f)
            {
                List<object> copiedPayload = new List<object>(payload);
                CRunner.instance.StartCoroutine(SerializationDelay(() =>
                { PhotonNetwork.NetworkingClient.OpRaiseEvent(eventCode, copiedPayload, raiseOptions, send); }, delay));
            }
            else
            {
                PhotonNetwork.NetworkingClient.OpRaiseEvent(eventCode, payload, raiseOptions, send);
            }
            batch.Clear();
        }

        public static IEnumerator SerializationDelay(Action callback, float delay)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }
        public static void MultiSerialize(bool exclude = false, PhotonView[] viewFilter = null, int timeOffset = 0, float delay = 0f)
        {
            if (!NetworkSystem.Instance.InRoom)
                return;
            viewFilter ??= Array.Empty<PhotonView>();
            NonAllocDictionary<int, PhotonView> photonViewList = PhotonNetwork.photonViewList;
            List<PhotonView> viewsToSerialize = new List<PhotonView>();
            List<int> filteredViewIDs = viewFilter.Select(view => view.ViewID).ToList();
            foreach (PhotonView photonView in photonViewList.Values)
            {
                if (!photonView.IsMine || photonView.Synchronization == ViewSynchronization.Off || !photonView.isActiveAndEnabled || PhotonNetwork.blockedSendingGroups.Contains(photonView.Group))
                    continue;
                if (exclude)
                    if (!filteredViewIDs.Contains(photonView.ViewID))
                        viewsToSerialize.Add(photonView);
                else
                    if (filteredViewIDs.Contains(photonView.ViewID))
                        viewsToSerialize.Add(photonView);
            }
            foreach (PhotonView view in viewsToSerialize)
                SendSerialize(view, null, timeOffset, delay);
        }

        public static void PartyLagGun() // kicks after a while? -sleepy
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;

                if (lockTarget != null && gunLocked && FriendshipGroupDetection.Instance.IsInMyGroup(lockTarget.Creator.GetPlayerRef().UserId))
                {
                    AntiKickEvents = true;
                    for (int i = 0; i < 3400; i++)
                        FriendshipGroupDetection.Instance.photonView.RPC("RequestPartyGameMode", lockTarget.Creator.GetPlayerRef(),
                            new object[] { GameMode.gameModeKeyByName.Keys.ToArray()[Random.Range(0, GameMode.gameModeKeyByName.Keys.Count)] });
                    Safety.RPCProc();
                }

                if (GetGunInput(true))
                {
                    VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                    if (!rig.IsLocal())
                    {
                        lockTarget = rig;
                        gunLocked = true;
                    }
                }
            }
            else
            {
                lockTarget = null;
                gunLocked = false;
                AntiKickEvents = false;
            }
        }

        public static float plagDelay;
        public static void PartyLagAll()
        {
            if (FriendshipGroupDetection.Instance.IsInParty && Time.time > plagDelay)
            {
                plagDelay = Time.time + 10f;
                var partyPlayers = NetworkSystem.Instance.PlayerListOthers.Where(plr => FriendshipGroupDetection.Instance.IsInMyGroup(plr.UserId)).ToList();
                for (int i = 0; i < 3400; i++)
                {
                    foreach (var plr in partyPlayers)
                    {
                        var photonPlayer = PhotonNetwork.CurrentRoom.GetPlayer(plr.ActorNumber);
                        FriendshipGroupDetection.Instance.photonView.RPC("RequestPartyGameMode", photonPlayer,
                            new object[] { GameMode.gameModeKeyByName.Keys.ToArray()[Random.Range(0, GameMode.gameModeKeyByName.Keys.Count)] });
                    }
                }
                Safety.RPCProc();
            }
        }
        public static readonly string prop = "arrakis v" + PluginInfo.Version;
        public static bool HasProp(Player player)
        {
            return player.CustomProperties.ContainsKey(prop);
        }
        public static List<Player> GetAllArrakisUsers()
        {
            List<Player> Users = new List<Player>();
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (HasProp(p))
                    Users.Add(p);
            }
            return Users;
        }
        public static void SpamPrideCube()
        {
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                Texture2D texture = AssetBundleLoader.LoadTexture("pride.png");
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.mainTexture = texture;
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                cube.transform.localScale = Vector3.one * 0.25f;
                Renderer renderer = cube.GetComponent<Renderer>();
                renderer.material = mat;
                Rigidbody rb = cube.AddComponent<Rigidbody>();
                rb.mass = 1f;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                Object.Destroy(cube, 5f);
            }
        }

        public static void SwitchToTcp()
        {
            var loadBalancingPeer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
            loadBalancingPeer.TransportProtocol = ConnectionProtocol.Tcp;
            loadBalancingPeer.peerBase = new TPeer
            {
                photonPeer = loadBalancingPeer,
                usedTransportProtocol = ConnectionProtocol.Tcp,
                DoFraming = true
            };
        }

        public static void SwitchToUdp()
        {
            var loadBalancingPeer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
            loadBalancingPeer.TransportProtocol = ConnectionProtocol.Udp;
            loadBalancingPeer.peerBase = new EnetPeer
            {
                photonPeer = loadBalancingPeer,
                usedTransportProtocol = ConnectionProtocol.Udp
            };
            NetworkSystem.Instance.ReturnToSinglePlayer();
            Important.Reauth();
        }


        // Admin Mods
        public static float admindelay;
        public static void AdminKickAll() =>
            Admin.ExecuteCommand("kickall", ReceiverGroup.All);

        public static void AdminRestoreMicAll()
        {
            if (Time.time > admindelay)
            {
                admindelay = Time.time + 0.05f;
                Admin.ExecuteCommand("restoremicrophone", ReceiverGroup.All);
            }
        }
        public static void AdminRestroeMicGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                    if (!rig.IsLocal())
                    {
                        if (Time.time > admindelay)
                        {
                            admindelay = Time.time + 0.1f;
                            Admin.ExecuteCommand("restoremicrophone", GetNetPlayerFromVRRig(rig).ActorNumber);
                        }
                    }
                }
            }
        }

        public static void AdminKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                    if (!rig.IsLocal())
                    {
                        if (Time.time > admindelay)
                        {
                            admindelay = Time.time + 0.1f;
                            Admin.ExecuteCommand("kick", ReceiverGroup.All, GetNetPlayerFromVRRig(rig).UserId);
                        }
                    }
                }
            }
        }
        public static void AdminBringGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                    if (!rig.IsLocal())
                    {
                        if (Time.time > admindelay)
                        {
                            admindelay = Time.time + 0.1f;
                            Admin.ExecuteCommand("bring", GetNetPlayerFromVRRig(rig).ActorNumber, GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 1.5f, 0f));
                        }
                    }
                }
            }
        }
        public static void AdminBringAll()
        {
            if (Time.time > admindelay)
            {
                admindelay = Time.time + 0.05f;
                Admin.ExecuteCommand("bring", ReceiverGroup.Others, GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 1.5f, 0f));
            }
        }
        public static void AdminLightningStrikeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                    if (!rig.IsLocal())
                    {
                        if (Time.time > admindelay)
                        {
                            admindelay = Time.time + 0.1f;
                            Admin.ExecuteCommand("lightningstrike", ReceiverGroup.All, NewPointer.transform.position);
                        }
                    }
                }
            }
        }
        public static void GetMenuUsers()
        {
            if (Time.time > Admin.indicatorDelay)
            {
                Admin.ExecuteCommand("isusing", ReceiverGroup.All);
                Admin.indicatorDelay = Time.time + 2f;
            }
        }
    }
}