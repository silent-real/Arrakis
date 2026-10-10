/*
using static UnityEngine.Time;
 * Arrakis | Mods/Master.cs
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
using Arrakis.Managers;
using Arrakis.Notifications;
using GorillaTag.Sports;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using static Arrakis.Menu.Main;

namespace Arrakis.Mods
{
    public class Master
    {
        public static void UntagAll()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (NetPlayer plr in NetworkSystem.Instance.PlayerListOthers)
                {
                    foreach (GorillaTagManager tag in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                    {
                        tag.currentInfected.Remove(plr);
                        tag.InfectionRoundEnd();
                    }
                }
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }

        public static void UntagSelf()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (GorillaTagManager tag in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                {
                    tag.currentInfected.Remove(NetworkSystem.Instance.LocalPlayer);
                    tag.InfectionRoundEnd();
                }
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }

        public static void NoGuardian()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (GorillaGuardianZoneManager zone in GuardianZMan())
                {
                    if (zone.IsZoneValid() && zone.CurrentGuardian == NetworkSystem.Instance.LocalPlayer)
                        zone.SetGuardian(null);
                }
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }

        public static void BecomeGuardian()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (TappableGuardianIdol t in GetAllTappables())
                {
                    if (t.manager != null && !t.isChangingPositions)
                    {
                        GorillaGuardianZoneManager zoneManager = t.zoneManager;
                        if (zoneManager.IsZoneValid() && t.manager != null && zoneManager.CurrentGuardian == null)
                            zoneManager.SetGuardian(NetworkSystem.Instance.LocalPlayer);
                    }
                }
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }

        public static void SetGuardianGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    if (PhotonNetwork.LocalPlayer.IsMasterClient)
                    {
                        VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                        if (!rig.IsLocal())
                        {
                            foreach (TappableGuardianIdol t in GetAllTappables())
                            {
                                if (t.manager != null && !t.isChangingPositions)
                                {
                                    GorillaGuardianZoneManager zoneManager = t.zoneManager;
                                    if (zoneManager.IsZoneValid() && t.manager != null && zoneManager.CurrentGuardian == null)
                                        zoneManager.SetGuardian(rig.Creator);
                                }
                            }
                        }
                    }
                    else
                    {
                        NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
                    }
                }
            }
        }

        private static Dictionary<PhotonView, LineRenderer> tp = new Dictionary<PhotonView, LineRenderer>();
        public static void GetAllPhotonViews()
        {
            foreach (PhotonView view in GetViews())
            {
                if (!tp.TryGetValue(view, out LineRenderer line))
                {
                    GameObject holder = new GameObject();
                    line = holder.AddComponent<LineRenderer>();
                    line.useWorldSpace = true;
                    line.material = new Material(Shader.Find("GUI/Text Shader"));
                    line.positionCount = 2;
                    line.startWidth = 0.02f;
                    line.endWidth = 0.02f;
                    tp[view] = line;
                }
                line.startColor = Settings.backgroundColor.GetCurrentColor();
                line.endColor = Settings.backgroundColor.GetCurrentColor();
                line.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                line.SetPosition(1, view.transform.position);
            }
        }

        public static void DisableViewTracers()
        {
            foreach (var view in tp.Values)
            {
                GameObject.Destroy(view.gameObject);
            }
            tp.Clear();
        }

        public static void DestroyViewGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    if (PhotonNetwork.LocalPlayer.IsMasterClient)
                    {
                        PhotonView photonView = Ray.collider.GetComponentInParent<PhotonView>();
                        if (photonView != null)
                            PhotonNetwork.Destroy(photonView);
                    }
                    else
                    {
                        NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
                    }
                }
            }
        }

        public static void UnGuardianGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    if (PhotonNetwork.LocalPlayer.IsMasterClient)
                    {
                        VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                        if (!rig.IsLocal())
                        {
                            foreach (GorillaGuardianZoneManager zone in GuardianZMan())
                            {
                                if (zone.IsZoneValid() && zone.CurrentGuardian == rig.Creator)
                                    zone.SetGuardian(null);
                            }
                        }
                    }
                    else
                    {
                        NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
                    }
                }
            }
        }

        public static void UntagGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    if (PhotonNetwork.LocalPlayer.IsMasterClient)
                    {
                        VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                        if (!rig.IsLocal())
                        {
                            foreach (GorillaTagManager tag in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                            {
                                if (tag.currentInfected.Contains(rig.Creator))
                                {
                                    tag.currentInfected.Remove(rig.Creator);
                                    tag.InfectionRoundEnd();
                                }
                            }
                        }
                    }
                    else
                    {
                        NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
                    }
                }
            }
        }
        public static List<int> blockIds = new List<int> // NOT MINE CREDS TO SOME RANDOM KID IN A MODDING SERVER -sleepy
        {
            857098599, -2063561053, 1510110959, 1848143946, 866161220,
            -604999206, 1844542113, -1514335082, 868696147, -460092905,
            1000122295, 757678001, 1513669651, -1537067750, 944837962,
            -1499829961, -604288536, -709037470, -1724151965, -350300979,
            1858470402, 1794855203, -1326806786, 724396559,
            1755661147, 1459635109, -566818631, 1925587737, -1441835191,
            -1324502924, 111152940, 798264081, -1821684029, 1895524638,
            1961336659, -1059201160, 1051576141, 539529939, -1535427925,
            1210710592, 1228919111, 252298128, -648273975, 1120512569,
            532163265, -845420418, 1834228748, 1063967233, 1700948013,
            2059548340, -1447051713, 1134055607, 1700655257, -1724819324,
            -1218055069, 251444537, -1446121736, -1927069002, -385891195,
            -196038879, -993249117, 1145900217, 1859614656, 1821589092,
            661312857, 1701825380, -1621444201, 1924370326, -1193326485,
            -1194390666, -751675075, -933358727, 24270440,
        };
        public static System.Random random = new System.Random();
        public static int GetRandomId()
        {
            int index = random.Next(blockIds.Count);
            return blockIds[index];
        }
        private static BuilderTable GetBuilderTable()
        {
            BuilderTable.TryGetBuilderTableForZone(VRRig.LocalRig.zoneEntity.currentZone, out BuilderTable table);
            return table;
        }
        public static BuilderTable BuilderTable => GetBuilderTable();
        public static float blockDebounce = 0.1f;
        private static float blockDelay;
        public static int pieceId = -1;
        private static readonly List<BuilderPiece> pieceCache = new List<BuilderPiece>();
        private static float pieceCacheExpiry;
        private static readonly Dictionary<int, float> recentPieces = new Dictionary<int, float>();
        private static int grabsThisWindow;

        public static void SpawnBlock(int pieceType, Vector3 position, Quaternion rotation, int materialType, object target = null, bool overrideFreeze = false, bool forceGravity = false, Vector3? velocity = null, Vector3? angVelocity = null)
        {
            BuilderTable table = BuilderTable;
            BuilderTableNetworking network = table.builderNetworking;

            if (Time.time <= blockDelay)
                return;

            if (NetworkSystem.Instance.IsMasterClient)
            {
                blockDelay = Time.time + 0.02f;
                int id = table.CreatePieceId();
                object[] createArgs =
                {
                    pieceType,
                    id,
                    BitPackUtils.PackWorldPosForNetwork(position),
                    BitPackUtils.PackQuaternionForNetwork(rotation),
                    materialType,
                    (byte)4,
                    1,
                    PhotonNetwork.LocalPlayer
                };
                if (target is RpcTarget rpcCreate)
                    network.photonView.RPC("PieceCreatedByShelfRPC", rpcCreate, createArgs);
                else if (target is Player playerCreate)
                    network.photonView.RPC("PieceCreatedByShelfRPC", playerCreate, createArgs);
                else
                    network.photonView.RPC("PieceCreatedByShelfRPC", RpcTarget.All, createArgs);

                if (!overrideFreeze || forceGravity)
                {
                    object[] grabArgs =
                    {
                        network.CreateLocalCommandId(),
                        id,
                        true,
                        BitPackUtils.PackHandPosRotForNetwork(Vector3.zero, Quaternion.identity),
                        PhotonNetwork.LocalPlayer
                    };
                    if (target is RpcTarget rpcGrab)
                        network.photonView.RPC("PieceGrabbedRPC", rpcGrab, grabArgs);
                    else if (target is Player playerGrab)
                        network.photonView.RPC("PieceGrabbedRPC", playerGrab, grabArgs);
                    else
                        network.photonView.RPC("PieceGrabbedRPC", RpcTarget.All, grabArgs);

                    object[] dropArgs =
                    {
                        network.CreateLocalCommandId(),
                        id,
                        position,
                        rotation,
                        velocity ?? Vector3.zero,
                        angVelocity ?? Vector3.zero,
                        PhotonNetwork.LocalPlayer
                    };
                    if (target is RpcTarget rpcDrop)
                        network.photonView.RPC("PieceDroppedRPC", rpcDrop, dropArgs);
                    else if (target is Player playerDrop)
                        network.photonView.RPC("PieceDroppedRPC", playerDrop, dropArgs);
                    else
                        network.photonView.RPC("PieceDroppedRPC", RpcTarget.All, dropArgs);
                }
                return;
            }

            if (Time.time >= pieceCacheExpiry)
            {
                pieceCacheExpiry = Time.time + 0.1f; // 0.5f
                pieceCache.Clear();
                pieceCache.AddRange(GameObject.FindObjectsByType<BuilderPiece>(FindObjectsSortMode.None));
            }
            Vector3 handPos = ServerLeftHandPos;
            int actor = PhotonNetwork.LocalPlayer.ActorNumber;
            BuilderPiece piece = null, fallback = null;
            float bestDist = 6.25f, fallbackDist = 6.25f;
            foreach (BuilderPiece p in pieceCache)
            {
                if (p == null || !p.gameObject.activeInHierarchy || p.isBuiltIntoTable)
                    continue;
                if (recentPieces.TryGetValue(p.pieceId, out float until) && Time.time < until)
                    continue;
                float dist = (p.transform.position - handPos).sqrMagnitude;
                if (dist >= fallbackDist && dist >= bestDist)
                    continue;
                if (!p.CanPlayerGrabPiece(actor, p.transform.position))
                    continue;
                if (p.pieceType == pieceType && dist < bestDist)
                {
                    piece = p;
                    bestDist = dist;
                }
                if (dist < fallbackDist)
                {
                    fallback = p;
                    fallbackDist = dist;
                }
            }
            piece = piece ?? fallback;
            if (piece == null)
                return;
            if (++grabsThisWindow >= 3)
            {
                grabsThisWindow = 0;
                blockDelay = Time.time + blockDebounce;
            }
            if ((position - handPos).sqrMagnitude > 6.25f)
                position = handPos + (position - handPos).normalized * 2.5f;
            pieceId = piece.pieceId;
            recentPieces[piece.pieceId] = Time.time + 1f;
            network.RequestGrabPiece(piece, true, Vector3.zero, Quaternion.identity);
            network.RequestDropPiece(piece, position, rotation, velocity ?? Vector3.zero, angVelocity ?? Vector3.zero);
        }

        public static void SpawnBlockGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    SpawnBlock(GetRandomId(), NewPointer.gameObject.transform.position, RandomQuaternion(), 0, RpcTarget.All);
                }
            }
        }
        public static void BlockFreezeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (lockTarget != null && gunLocked)
                {
                    SpawnBlock(-566818631, lockTarget.transform.position + RandomVector3(0.4f), RandomQuaternion(), 0, lockTarget.Creator.GetPlayerRef());
                    SpawnBlock(-566818631, lockTarget.transform.position + RandomVector3(0.4f), RandomQuaternion(), 0, lockTarget.Creator.GetPlayerRef());
                    SpawnBlock(-566818631, lockTarget.transform.position + RandomVector3(0.4f), RandomQuaternion(), 0, lockTarget.Creator.GetPlayerRef());
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
        public static void ChangeLavaState(InfectionLavaController.RisingLavaState state)
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=cyan>ARRAKIS</color><color=grey>]</color> You are not master client this mod will not work.");
                return;
            }
            var lava = InfectionLavaController.ActiveControllers.FirstOrDefault();
            if (lava == null)
                return;
            double startTime = NetworkSystem.Instance.InRoom ? NetworkSystem.Instance.SimTime : Time.timeAsDouble;
            lava.JumpToState(state);
            lava.reliableState.stateStartTime = startTime;
        }
        public static void BlockCrashAll()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Toggle("Block Crash All");
                return;
            }
            SpawnBlock(1934114066, new Vector3(-127.6248f, 16.99441f, -217.2094f), Quaternion.identity, 0, RpcTarget.Others, false, true);
        }
        public static void BlockSphere(float radius = 2f, int density = 40)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
            Vector3 center = GorillaTagger.Instance.offlineVRRig.transform.position;
            for (int i = 0; i < density * 2; i++)
            {
                float theta = 2 * Mathf.PI * i / ((1 + Mathf.Sqrt(5)) / 2);
                float phi = Mathf.Acos(1 - 2 * (i + 0.5f) / (density * 2));
                Vector3 pos = center + new Vector3(radius * Mathf.Sin(phi) * Mathf.Cos(theta), radius * Mathf.Sin(phi) * Mathf.Sin(theta), radius * Mathf.Cos(phi));
                SpawnBlock(GetRandomId(), pos, Quaternion.identity, 0, RpcTarget.All);
            }

        }
        public static void VIMKickAll()
        {
            if (!SubscriptionManager.Instance.subData[VRRig.LocalRig.Creator].active)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not a VIM subscriber this mod will not work.");
                return;
            }
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master this mod will not work.");
                return;
            }
            if (!NetworkSystem.Instance.CurrentRoom.isPublic)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> Room is not private this mod will not work.");
                return;
            }
            NetworkSystem.Instance.PlayerListOthers.ForEach(p => RoomControls.KickPlayer(p.ActorNumber));
        }
        public static void DestroyLighting()
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master this mod will not work.");
                return;
            }
            else
            {
                if (BetterDayNightManager.instance.photonView == null)
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> BetterDayNightManager.instance.photonView is null.");
                    return;
                }
                PhotonNetwork.Destroy(BetterDayNightManager.instance.photonView);
            }
        }
        public static void DestroyCritters()
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master this mod will not work.");
                return;
            }
            else
            {
                if (CrittersManager.instance.GetView == null)
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> CrittersManager.instance.GetView is null.");
                    return;
                }
                PhotonNetwork.Destroy(CrittersManager.instance.GetView);
            }
        }
        /*public static async void SetTarget(HitTargetNetworkState target, int value, float delay = 0.70f)
        {
            if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
                return;
            Traverse traverse = Traverse.Create(target);
            float nextUpdateTime = traverse.Field("nextHittableTimestamp").GetValue<float>(); // nextHittableTimestamp or _maxHoldTime, i havent tested and it somehow pulled both -sleepy
            if (Time.time <= nextUpdateTime)
                return;
            WatchableIntSO watchable = traverse.Field("networkedScore").GetValue<WatchableIntSO>();
            int newValue = value == 6969 ? watchable.Value + 1 : value;
            watchable.Value = newValue;
            traverse.Field("nextHittableTimestamp").SetValue(Time.time + delay);
        } incase you want target mods -sleepy
        */

        private static HalloweenGhostChaser _lucy;
        public static HalloweenGhostChaser Lucy
        {
            get
            {
                if (_lucy == null)
                    _lucy = GameObject.FindObjectsOfType<HalloweenGhostChaser>()[0]; // Ez who needs a fucking path -nova
                return _lucy;
            }
        }

        public static void SpawnBlueLucy()
        {
            if (Lucy.IsMine)
            {
                Lucy.timeGongStarted = 0f;
                Lucy.timeRiseStarted = 0f;
                Lucy.isSummoned = false;
                Lucy.currentState = HalloweenGhostChaser.ChaseState.Gong;
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }
        public static void SpawnRedLucy()
        {
            if (Lucy.IsMine)
            {
                Lucy.timeGongStarted = 0f;
                Lucy.timeRiseStarted = 0f;
                Lucy.isSummoned = true;
                Lucy.currentState = HalloweenGhostChaser.ChaseState.Gong;
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }
        public static void DespawnLucy()
        {
            if (Lucy.IsMine)
            {
                Lucy.isSummoned = false;
                Lucy.currentState = HalloweenGhostChaser.ChaseState.Dormant;
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }
        public static void FastLucy()
        {
            if (Lucy.IsMine)
            {
                Lucy.currentSpeed = 3f;
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }
        public static void SlowLucy()
        {
            if (Lucy.IsMine)
            {
                Lucy.currentSpeed = 0.1f;
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }
        public static void BreakLucy()
        {
            if (Lucy.IsMine)
            {
                Lucy.currentSpeed = int.MinValue;
                Lucy.transform.position = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }

        private static float lucyspaztimething = 0f;
        private static bool lucyspazboolhing = false;
        public static void SpazLucy()
        {
            if (Lucy.IsMine)
            {
                if (Time.time < lucyspaztimething)
                {
                    Lucy.timeGongStarted = 0f;
                    Lucy.timeRiseStarted = 0f;
                    Lucy.isSummoned = true;
                    Lucy.currentState = HalloweenGhostChaser.ChaseState.Gong;
                    lucyspaztimething = Time.time + 0.1f;
                    lucyspazboolhing = !lucyspazboolhing;
                }
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }
        private static float lucyspaztarget = 0f;
        public static void SpazLucyTarget()
        {
            if (Lucy.IsMine)
            {
                if (Time.time < lucyspaztarget)
                {
                    Lucy.currentState = HalloweenGhostChaser.ChaseState.Chasing;
                    Lucy.targetPlayer = RigManager.GetRandomVRRig(true).Creator;
                    lucyspaztarget = Time.time + 0.5f;
                }
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }

        public static void MoveLucyGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    if (Lucy.IsMine)
                    {
                        Lucy.transform.position = NewPointer.transform.position;
                    }
                    else
                    {
                        NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
                    }
                }
            }
        }

        public static void GrabLucy()
        {
            if (Lucy.IsMine)
            {
                if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
                {
                    Lucy.targetPlayer = null;
                    Lucy.currentState = HalloweenGhostChaser.ChaseState.Chasing;
                    Lucy.transform.position = VRRig.LocalRig.rightHandTransform.position;
                }
                if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
                {
                    Lucy.targetPlayer = null;
                    Lucy.currentState = HalloweenGhostChaser.ChaseState.Chasing;
                    Lucy.transform.position = VRRig.LocalRig.leftHandTransform.position;
                }
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
            }
        }

        public static void LucyFloatGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Lucy.IsMine)
                    {
                        Lucy.targetPlayer = lockTarget.Creator;
                        Lucy.grabbedPlayer = lockTarget.Creator;
                        Lucy.followTarget = lockTarget.head.rigTarget;
                        Lucy.currentState = HalloweenGhostChaser.ChaseState.Grabbing;
                        Lucy.grabTime = Time.time;
                        Lucy.grabDuration = 999999f;
                        Lucy.isSummoned = true;
                        if (Lucy.ghostBody.activeSelf)
                            Lucy.ghostBody.SetActive(false);
                    }
                    else
                    {
                        NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
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
        public static void SpazPaintbrawl()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                foreach (GorillaPaintbrawlManager brawl in GameObject.FindObjectsByType<GorillaPaintbrawlManager>(FindObjectsSortMode.None))
                {
                    foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
                    {
                        brawl.playerLives[player.ActorNumber] = UnityEngine.Random.Range(0, 3);
                        brawl.playerStunTimes[player.ActorNumber] = float.PositiveInfinity;
                        if (brawl.OnRedTeam(player))
                            brawl.playerStatusDict[player.ActorNumber] = GorillaPaintbrawlManager.PaintbrawlStatus.BlueTeam;
                        else
                            brawl.playerStatusDict[player.ActorNumber] = GorillaPaintbrawlManager.PaintbrawlStatus.RedTeam;
                    }
                }
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master client this mod wont work.");
                Toggle("Spaz Paintbrawl");
                return;
            }
        }
        public static void WaterBallScore(int team)
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master this mod will not work.");
                return;
            }
            else
            {
                SportScoreboard.Instance.TeamScored(team);
                SportScoreboard.Instance.TeamScored(team);
            }
        }
        public static void WaterBallScoreSpaz()
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master this mod will not work.");
                return;
            }
            else
            {
                SportScoreboard.Instance.TeamScored(1);
                SportScoreboard.Instance.TeamScored(0);
            }
        }
        public static void WaterBallEndGame()
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master this mod will not work.");
                return;
            }
            else
            {
                CRunner.instance.StartCoroutine(SportScoreboard.Instance.MatchEndCoroutine(UnityEngine.Random.Range(0, 1)));
            }
        }
        public static void WaterBallResetScores()
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=yellow>ARRAKIS</color><color=grey>]</color> You are not master this mod will not work.");
                return;
            }
            else
            {
                SportScoreboard.Instance.ResetScores();
            }
        }
    }
}