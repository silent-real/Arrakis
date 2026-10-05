/*
 * Arrakis | Mods/Important.cs
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

using Arrakis.Notifications;
using ExitGames.Client.Photon;
using GorillaNetworking;
using GorillaTagScripts;
using HarmonyLib;
using Photon.Pun;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static Arrakis.Menu.Main;

namespace Arrakis.Mods
{
    public class Important
    {
        public static void QuitGame()
        {
            Prompt("Are you sure you want to exit Gorilla Tag.", () =>
            {
                Application.Quit();
            });
        }

        public static void UnloadMenu()
        {
            Prompt("Are you sure you want to unload the menu you wont be able to use it again unless you restart youe game", () =>
            {
                if (menu != null)
                    DestroyMenu();
            });
        }

        public static void ChangeName(string newname)
        {
            PhotonNetwork.LocalPlayer.NickName = newname;
            GorillaComputer.instance.savedName = newname;
            GorillaComputer.instance.currentName = newname;
            GorillaTagger.Instance.offlineVRRig.SetNameTagText(newname);
            PlayerPrefs.SetString("playerName", newname);
            PlayerPrefs.Save();
        }

        public static void Disconnect() =>
            NetworkSystem.Instance.ReturnToSinglePlayer();

        public static void JoinRandomRoom() =>
            PhotonNetworkController.Instance.AttemptToJoinPublicRoom(PhotonNetworkController.Instance.currentJoinTrigger);

        public static void Reconnect()
        {
            if (NetworkSystem.Instance.InRoom)
                NetworkSystem.Instance.ReturnToSinglePlayer();
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(reconnectingRoomName, JoinType.Solo);
        }

        public static void DisableWindBarriers()
        {
            GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Environment/Forest_ForceVolumes/").SetActive(false);
            GameObject.Find("Environment Objects/LocalObjects_Prefab/ForestToHoverboard/TurnOnInForestAndHoverboard/ForestDome_CollisionOnly").SetActive(false);
        }
        public static void EnableWindBarriers()
        {
            GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Environment/Forest_ForceVolumes/").SetActive(true);
            GameObject.Find("Environment Objects/LocalObjects_Prefab/ForestToHoverboard/TurnOnInForestAndHoverboard/ForestDome_CollisionOnly").SetActive(true);
        }

        public static void ButtonClick()
        {
            if (Mouse.current.leftButton.isPressed)
            {
                Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                Physics.Raycast(ray, out var Ray, 512f, NoInvisLayerMask());
                GorillaTagger.Instance.rightHandTriggerCollider.GetComponent<TransformFollow>().enabled = false;
                GorillaTagger.Instance.rightHandTriggerCollider.transform.position = Ray.point;
            }
            else
            {
                GorillaTagger.Instance.rightHandTriggerCollider.GetComponent<TransformFollow>().enabled = true;
            }
        }

        public static void ConnectToRegion(string region) =>
            PhotonNetwork.ConnectToRegion(region);

        public static void Reauth() =>
            MothershipAuthenticator.Instance.BeginLoginFlow();

        public static void JoinRoom(string roomCode) =>
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomCode, JoinType.Solo);

        public static void DisableAFKKick() =>
            PhotonNetworkController.Instance.disableAFKKick = true;
        public static void EnsableAFKKick() =>
            PhotonNetworkController.Instance.disableAFKKick = false;

        public static void FirstPerson()
        {
            if (TPC != null)
            {
                if (menu != null && !XRSettings.isDeviceActive)
                    return;
                TPC.fieldOfView = 90f;
                TPC.gameObject.transform.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>().enabled = false;
                TPC.gameObject.transform.position = GorillaTagger.Instance.headCollider.transform.position;
                TPC.gameObject.transform.rotation = Quaternion.Lerp(TPC.transform.rotation, GorillaTagger.Instance.headCollider.transform.rotation, 0.075f);
            }
        }
        public static void DisableFirstPerson()
        {
            if (TPC != null)
            {
                TPC.fieldOfView = 60f;
                TPC.gameObject.transform.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>().enabled = true;
            }
        }
        public static void UnlockComp() =>
            GorillaComputer.instance.CompQueueUnlockButtonPress();
        public static void ChangeQueue(string queue) =>
            GorillaComputer.instance.currentQueue = queue;
        public static void ChangeGamemode(string gamemode) =>
            GorillaComputer.instance.SetGameModeWithoutButton(gamemode);
        public static void CLearNotis() =>
            NotificationManager.ClearAllNotifications();

        /*public static void BuyBarrel() // noted out incase barrel mods comeback (i dont think they will) -sleepy
        {
            int barrelPrice = CosmeticsController.instance.GetItemFromDict("LMAPE.").cost;
            int currentRocks = CosmeticsController.instance.currencyBalance;
            if (currentRocks >= barrelPrice)
            {
                PromptSingle("The barrel is currently being bought.");
                CosmeticsController.instance.currentCart.Insert(0, CosmeticsController.instance.GetItemFromDict("LMAPE."));
                CosmeticsController.instance.ProcessExternalUnlock("LMAPE.", true, false);
                NotificationManager.SendNotification($"<color=cyan>[ARRAKIS]</color> Successfully bought barrel.");
            }
            else
            {
                PromptSingle($"You currently only have {currentRocks} shiny rocks you need {barrelPrice - currentRocks} more the barrel costs {barrelPrice}.");
            }
        }*/

        public static async void CreatePublicLobby(string roomName)
        {
            GorillaNetworkJoinTrigger trigger = PhotonNetworkController.Instance.currentJoinTrigger;
            if (trigger == null || trigger.networkZone == "private")
            {
                float closest = float.MaxValue;
                foreach (GorillaNetworkJoinTrigger joinTrigger in Object.FindObjectsByType<GorillaNetworkJoinTrigger>(FindObjectsSortMode.None))
                {
                    float distance = Vector3.Distance(VRRig.LocalRig.transform.position, joinTrigger.transform.position);
                    if (distance < closest)
                    {
                        closest = distance;
                        trigger = joinTrigger;
                    }
                }
            }
            RoomConfig config = new RoomConfig
            {
                createIfMissing = true,
                isJoinable = true,
                isPublic = true,
                MaxPlayers = 10
            };
            config.CustomProps = new Hashtable
            {
                { "gameMode", trigger.GetFullDesiredGameModeString() },
                { "platform", Traverse.Create(PhotonNetworkController.Instance).Field("platformTag").GetValue<string>() },
                { "queueName", GorillaComputer.instance.currentQueue },
                { "language", LocalisationManager.CurrentLanguage.ToString() },
                { "fan_club", SubscriptionManager.IsLocalSubscribed().ToString().ToLower() }
            };
            await NetworkSystem.Instance.ConnectToRoom(roomName, config, -1);
        }
        public static string GenerateRandomString(int a = 4)
        {
            string chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789";
            string roomName = "";
            for (int i = 0; i < a; i++)
                roomName += chars[UnityEngine.Random.Range(0, chars.Length)];
            return roomName;
        }
    }
}