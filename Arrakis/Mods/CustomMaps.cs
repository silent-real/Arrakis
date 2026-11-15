/*
 * Arrakis | Mods/CustomMaps.cs
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

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static Arrakis.Menu.Main;

namespace Arrakis.Mods
{
    public class CustomMaps
    {
        public class SlapBattles
        {
            public static void FlingAll()
            {
                foreach (NetPlayer plr in NetworkSystem.Instance.PlayerListOthers)
                {
                    MovePlayer(plr, Vector3.up * 50f * 5f);
                }
            }

            public static void BringAll()
            {
                Vector3 targetPosition = GorillaTagger.Instance.headCollider.transform.position;
                foreach (NetPlayer plr in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig rig = GorillaGameManager.StaticFindRigForPlayer(plr);
                    if (rig == null)
                        continue;
                    Vector3 direction = targetPosition - rig.transform.position;
                    MovePlayer(plr, direction);
                }
            }

            public static void GiveFlyGun()
            {
                if (GetGunInput(false))
                {
                    var GunData = RenderGun();
                    GameObject NewPointer = GunData.NewPointer;
                    RaycastHit Ray = GunData.Ray;

                    if (gunLocked && lockTarget != null)
                    {
                        if (lockTarget.rightThumb.calcT > 0f)
                        {
                            MovePlayer(lockTarget.Creator, lockTarget.transform.position += lockTarget.headConstraint.transform.position * Time.deltaTime * 15f);
                        }
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

            public static void FLingGun()
            {
                if (GetGunInput(false))
                {
                    var GunData = RenderGun();
                    GameObject NewPointer = GunData.NewPointer;
                    RaycastHit Ray = GunData.Ray;
                    if (GetGunInput(true))
                    {
                        VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                        if (rig != null && rig != VRRig.LocalRig)
                        {
                            MovePlayer(rig.Creator, Vector3.up * 50f * 5f);
                        }
                    }
                }
            }

            public static void MovePlayer(NetPlayer plr, Vector3 velo)
            {
                object[] data = new object[]
                {
                    "slapped", plr.GetPlayerRef(), velo,
                    PhotonNetwork.LocalPlayer, 1.0
                };
                PhotonNetwork.RaiseEvent(180, data, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
            }
        }
    }
}