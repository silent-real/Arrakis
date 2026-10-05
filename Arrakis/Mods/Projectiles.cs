/*
using static UnityEngine.Color;
 * Arrakis | Mods/Projectiles.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Arrakis.Extensions;
using Arrakis.Managers;
using Arrakis.Patches.Patchers;
using ExitGames.Client.Photon;
using GorillaNetworking;
using GorillaTag.CosmeticSystem;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR;
using static Arrakis.Menu.Main;

namespace Arrakis.Mods
{
    public class Projectiles
    {
        public static int ProjectileIndex = 0;
        public static string[] ProjectilesNames = new string[]
        {
            "SnowballRightAnchor", "GrowingSnowballRightAnchor", "WaterBalloonRightAnchor", "LavaRockAnchor",
            "BucketGiftFunctionalAnchor_Right", "ScienceCandyRightAnchor", "FishFoodRightAnchor", "HotDogRightAnchor",
            "Fireworks_Anchor Variant_Right Hand", "AppleRightAnchor", "BookRightAnchor", "CoinRightAnchor",
            "EggRightHand_Anchor Variant", "IceCreamRightAnchor", "GrowingMashedPotatoRightAnchor", "ChipsRightAnchor",
            "ApplePieRightAnchor", "BerryPieRightAnchor", "CornRightAnchor", "TurkeyLegRightAnchor", "GoalpostFootball_Anchor_RightHand",
            "HotCocoaCup_Anchor_RIGHT", "PillowProjectile_Anchor_RIGHT"
        };
        private static string CurrentProjectile = "SnowballRightAnchor";
        public static void ChangeProjectile(bool increment = true)
        {
            if (increment)
            {
                ProjectileIndex = (ProjectileIndex + 1) % ProjectilesNames.Length;
            }
            else
            {
                ProjectileIndex = (ProjectileIndex - 1 + ProjectilesNames.Length) % ProjectilesNames.Length;
            }

            CurrentProjectile = ProjectilesNames[ProjectileIndex];
            GetIndex("Change Projectile").overlapText = $"Change Projectile <color=grey>[<color=cyan>{ProjectilesNames[ProjectileIndex]}</color>]</color>";
        }

        public static void ProjectileSpammer()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
                SpawnProjectile(CurrentProjectile, GorillaTagger.Instance.rightHandTransform.position, Vector3.zero, Settings.projectileColor);
            else
                cachedThrow.SetSnowballActiveLocal(false);
        }

        public static void ProjectileLauncher()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
                SpawnProjectile(CurrentProjectile, GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up * 50f, Settings.projectileColor);
            else
                cachedThrow.SetSnowballActiveLocal(false);
        }

        public static void ProjectileGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = NewPointer.transform.position;
                    SpawnProjectile(CurrentProjectile, NewPointer.transform.position + new Vector3(0f, 0.6f, 0f), Vector3.zero, Settings.projectileColor);
                }
                else
                {
                    VRRig.LocalRig.enabled = true;
                    cachedThrow.SetSnowballActiveLocal(false);
                }
            }
        }
        public static void ProjectileBlindGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (lockTarget != null && gunLocked)
                {
                    ProjectileBlindPlayer(lockTarget);
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
            }
        }
        public static void ProjectileBlindAll()
        {
            EventPatches.Override = () =>
            {
                if (NetworkSystem.Instance.InRoom)
                {
                    Experimental.MultiSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });
                    Vector3 arcpos = VRRig.LocalRig.transform.position;
                    foreach (NetPlayer plr in NetworkSystem.Instance.PlayerListOthers)
                    {
                        VRRig rig = GorillaGameManager.StaticFindRigForPlayer(plr);
                        VRRig.LocalRig.transform.position = rig.transform.position - Vector3.one * 3f;
                        Experimental.SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new [] { plr.ActorNumber }});
                        ProjectileBlindPlayer(rig);
                    }
                    Safety.RPCProc();
                    VRRig.LocalRig.enabled = true;
                    VRRig.LocalRig.transform.position = arcpos;
                    return false;
                }
                return true;
            };
        }
        public static Color BlindColor = Color.black;
        public static readonly Color[] BlindColors =
        {
            Color.black, Color.white, Color.red, Color.green,
            Color.blue, Color.yellow, Color.cyan,  Color.magenta,
            Color.orange, Color.purple, Color.hotPink, Color.brown,
            new Color(167, 167, 255) // the pan sexual furry ashley made me add this -sleepy
        };

        public static readonly string[] BlindColorNames =
        {
            "Black", "White", "Red", "Green",
            "Blue", "Yellow", "Cyan", "Magenta",
            "Orange",  "Purple", "Hot Pink", "Brown",
            "Perano"
        };
        public static int BlindColorIndex = 0;
        public static void ChangeBlindColor(bool increment = true)
        {
            if (increment)
            {
                BlindColorIndex = (BlindColorIndex + 1) % BlindColorNames.Length;
            }
            else
            {
                BlindColorIndex = (BlindColorIndex - 1 + BlindColorNames.Length) % BlindColorNames.Length;
            }

            CurrentProjectile = BlindColorNames[BlindColorIndex];
            GetIndex("Change Blind Color").overlapText = $"Change Projectile <color=grey>[<color=cyan>{BlindColorNames[BlindColorIndex]}</color>]</color>";
        }
        public static void ProjectileBlindPlayer(VRRig rig) =>
            SpawnProjectile("EggRightHand_Anchor Variant", rig.headMesh.transform.position + new Vector3(0f, 0.1f, 0f), new Vector3(0f, -15f, 0f), BlindColor);

        private static SnowballThrowable cachedThrow = null;
        private static GrowingSnowballThrowable cachedGThrow = null;
        private static float delay = 0f;
        public static void SpawnProjectile(string projectileName, Vector3 position, Vector3 velocity, Color color, int scale = 0)
        {
            try
            {
                color.a = 255;
                SnowballThrowable throwable = GetProjectile(projectileName);
                if (throwable != null)
                {
                    if (!throwable.gameObject.activeSelf)
                    {
                        throwable.SetSnowballActiveLocal(true);
                        throwable.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        throwable.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                        cachedThrow = throwable;
                    }
                    if (Time.time > delay)
                    {
                        if (projectileName.Contains("Growing"))
                        {
                            GrowingSnowballThrowable growing = throwable as GrowingSnowballThrowable;
                            cachedGThrow = growing;
                            if (NetworkSystem.Instance.InRoom)
                            {
                                PhotonNetwork.RaiseEvent(176, new object[] { growing.changeSizeEvent._eventId, scale },
                                    new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Encrypt = true, Reliability = false });
                                PhotonNetwork.RaiseEvent(176, new object[] { growing.snowballThrowEvent._eventId, position, velocity, GetIncrement(position, velocity, scale) },
                                    new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Encrypt = true, Reliability = false });
                            }
                        }
                        else
                        {
                            if (NetworkSystem.Instance.InRoom)
                            {
                                Color32 c = color;
                                int ps = projectileName == "SlingshotProjectile" ? 0 : (projectileName.ToLower().Contains("left") ? 1 : 2);
                                object[] senddata = new object[]
                                {
                                    position, velocity, ps, GetIncrement(position, velocity, throwable.transform.lossyScale.x),
                                    true, c.r, c.g, c.b, c.a
                                };
                                object[] eventdata = new object[]
                                {
                                    NetworkSystem.Instance.ServerTimestamp, 0, senddata
                                };
                                PhotonNetwork.RaiseEvent(3, eventdata, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                            }
                        }
                        delay = Time.time + 1.0f;
                    }
                }
            }
            catch { }
        }

        private static int increment;
        private static int GetIncrement(Vector3 pos, Vector3 velo, float scale)
        {
            try
            {
                GameObject g = new GameObject();
                SlingshotProjectile p = g.AddComponent<SlingshotProjectile>();
                int d = ProjectileTracker.AddAndIncrementLocalProjectile(p, velo, pos, scale);
                increment = d;
                GameObject.Destroy(g);
                return increment;
            }
            catch { increment++; return increment; }
        }


        public static Dictionary<string, SnowballThrowable> snowballs;
        private static bool loaded;
        public static SnowballThrowable GetProjectile(string projectileName) // Credits seralyth for this
        {
            if (CosmeticsV2Spawner_Dirty.isPrepared)
            {
                var throwables = ((AllCosmeticsArraySO)CosmeticsController.instance.v2_allCosmeticsInfoAssetRef.Asset).sturdyAssetRefs.Where(x => x.obj != null
                && x.obj.info.isThrowable).Select(x => x.obj.info.playFabID).Distinct().ToList();
                if (snowballs == null || snowballs.Count != (throwables.Count - 3))
                {
                    if (!CosmeticsV2Spawner_Dirty.isPrepared)
                        return null;
                    if (!GorillaComputer.instance.isConnectedToMaster)
                        return null;
                    if (!loaded && (CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringLeft.Count >= 1
                        && CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringRight.Count >= 1))
                    {
                        loaded = true;
                        CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringLeft.ForEach(v => VRRig.LocalRig.cosmeticsObjectRegistry.Cosmetic(v.Value));
                        CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringRight.ForEach(v => VRRig.LocalRig.cosmeticsObjectRegistry.Cosmetic(v.Value));
                        return null;
                    }
                    snowballs = new Dictionary<string, SnowballThrowable>();
                    foreach (SnowballMaker Maker in new[] { SnowballMaker.leftHandInstance, SnowballMaker.rightHandInstance })
                    {
                        foreach (SnowballThrowable Throwable in Maker.snowballs)
                        {
                            try
                            {
                                snowballs.Add(Throwable.transform.parent.gameObject.name, Throwable);
                            }
                            catch { }
                        }
                    }
                }
                projectileName += "(Clone)";
                if (!snowballs.TryGetValue(projectileName, out var projectile))
                    return null;
                return projectile;
            }
            return null;
        }
    }
}