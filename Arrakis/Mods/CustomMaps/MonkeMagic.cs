/*
 * Arrakis | Mods/CustomMaps/MonkeMagic.cs
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
using Arrakis.Managers.CustomMaps;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static Arrakis.Menu.Main;

namespace Arrakis.Mods.CustomMaps
{
    public class MonkeMagic : CustomMap // ty to E for pulling this (irl friend) -sleepy
    {
        public override long MapID => 5107228;
        public override ButtonInfo[] Buttons => new[]
        {
            new ButtonInfo { buttonText = "Monke Magic Crash All", method = MonkeMagicCrashAll, isTogglable = true, toolTip = "Crashes everyone if they have the map loaded." },
            new ButtonInfo { buttonText = "Monke Magic Crash Gun", method = MonkeMagicCrashGun, isTogglable = true, toolTip = "Crashes someone if they have the map loaded." },
            new ButtonInfo { buttonText = "Monke Magic Lighiting Strike Gun", method = MonkeMagicLighitingStrikeGun, isTogglable = true, toolTip = "Lighting strikes someone if they have the map loaded." },
            new ButtonInfo { buttonText = "Monke Magic Spawn Lucy All", method = SpawnLucyAll, isTogglable = false, toolTip = "Lighting strikes someone if they have the map loaded." },
            new ButtonInfo { buttonText = "Monke Magic Change Material All", method = ChangeMaterialAll, isTogglable = true, toolTip = "Lighting strikes someone if they have the map loaded." },
        };
        private static float lightningDelay;
        public static void LightningStrike(int ActorNumber)
        {
            if (!(Time.time > lightningDelay)) return;
            lightningDelay = Time.time + 0.2f;
            PhotonNetwork.RaiseEvent(180, new object[] { "SummonThunder", (double)ActorNumber }, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
            Safety.RPCProc();
        }
        private static float lucyDelay;
        public static void SpawnLucy(int ActorNumber)
        {
            if (!(Time.time > lucyDelay)) return;
            lightningDelay = Time.time + 0.2f;
            PhotonNetwork.RaiseEvent(180, new object[] { "SummonLucy", (double)ActorNumber }, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
            Safety.RPCProc();
        }
        private static float materialDelay;
        public static void ChangeMaterial(int ActorNumber)
        {
            if (Time.time > materialDelay)
            {
                materialDelay = Time.time + 0.2f;
                PhotonNetwork.RaiseEvent(180, new object[] { "ChangingMaterial", (double)ActorNumber, (double)Random.Range(0, VRRig.LocalRig.materialsToChangeTo.Length) }, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                Safety.RPCProc();
            }
        }
        public static float crashDelay;
        public static void CrashPlayer(int ActorNumber)
        {
            if (Time.time > crashDelay)
            {
                PhotonNetwork.RaiseEvent(180, new object[] { "leaveGame", (double)ActorNumber, false, (double)ActorNumber }, new RaiseEventOptions
                {
                    TargetActors = new[]
                    {
                        ActorNumber
                    }
                }, SendOptions.SendReliable);
                Safety.RPCProc();
                crashDelay = Time.time + 0.1f;
            }
        }
        public static void MonkeMagicCrashAll()
        {
            foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
                CrashPlayer(player.ActorNumber);
        }
        public static void MonkeMagicCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (lockTarget != null && gunLocked)
                {
                    CrashPlayer(lockTarget.Creator.ActorNumber);
                }
                if (GetGunInput(true))
                {
                    VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                    if (rig != null && rig != VRRig.LocalRig)
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
            }
        }
        public static void MonkeMagicLighitingStrikeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (lockTarget != null && gunLocked)
                {
                    LightningStrike(lockTarget.Creator.ActorNumber);
                }
                if (GetGunInput(true))
                {
                    VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                    if (rig != null && rig != VRRig.LocalRig)
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
            }
        }
        public static void SpawnLucyAll()
        {
            foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
                SpawnLucy(player.ActorNumber);
        }
        public static void ChangeMaterialAll()
        {
            foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
                ChangeMaterial(player.ActorNumber);
        }
    }
}