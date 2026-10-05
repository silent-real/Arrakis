/*
 * Arrakis | Mods/Fun.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Arrakis.Extensions;
using Arrakis.Managers;
using Arrakis.Notifications;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTagScripts;
using Liv.Lck.GorillaTag;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;
using Voxels;
using static Arrakis.Menu.Main;

namespace Arrakis.Mods
{
    public class Fun
    {
        public static async void UnlockAllCosmetics()
        {
            await Task.Delay(500);
            var cosmetics = CosmeticsController.instance;
            if (cosmetics == null) return;
            foreach (var item in cosmetics.allCosmetics)
            {
                if (!item.isNullItem && !cosmetics.unlockedCosmetics.Contains(item))
                {
                    cosmetics.unlockedCosmetics.Add(item);
                    switch (item.itemCategory)
                    {
                        case CosmeticsController.CosmeticCategory.Hat:
                            if (!cosmetics.unlockedHats.Contains(item))
                                cosmetics.unlockedHats.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Face:
                            if (!cosmetics.unlockedFaces.Contains(item))
                                cosmetics.unlockedFaces.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Badge:
                            if (!cosmetics.unlockedBadges.Contains(item))
                                cosmetics.unlockedBadges.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Paw:
                            if (!item.isThrowable)
                            {
                                if (!cosmetics.unlockedPaws.Contains(item))
                                    cosmetics.unlockedPaws.Add(item);
                            }
                            else
                            {
                                if (!cosmetics.unlockedThrowables.Contains(item))
                                    cosmetics.unlockedThrowables.Add(item);
                            }
                            break;
                        case CosmeticsController.CosmeticCategory.Fur:
                            if (!cosmetics.unlockedFurs.Contains(item))
                                cosmetics.unlockedFurs.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Shirt:
                            if (!cosmetics.unlockedShirts.Contains(item))
                                cosmetics.unlockedShirts.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Back:
                            if (!cosmetics.unlockedBacks.Contains(item))
                                cosmetics.unlockedBacks.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Arms:
                            if (!cosmetics.unlockedArms.Contains(item))
                                cosmetics.unlockedArms.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Chest:
                            if (!cosmetics.unlockedChests.Contains(item))
                                cosmetics.unlockedChests.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Pants:
                            if (!cosmetics.unlockedPants.Contains(item))
                                cosmetics.unlockedPants.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.TagEffect:
                            if (!cosmetics.unlockedTagFX.Contains(item))
                                cosmetics.unlockedTagFX.Add(item);
                            break;
                    }
                }
            }
            cosmetics.UpdateWardrobeModelsAndButtons();
            cosmetics.OnCosmeticsUpdated?.Invoke();
        }

        public static void FixHead()
        {
            VRRig.LocalRig.head.trackingRotationOffset.x = 0f;
            VRRig.LocalRig.head.trackingRotationOffset.y = 0f;
            VRRig.LocalRig.head.trackingRotationOffset.z = 0f;
        }
        public static void UpsidedownHead() =>
            VRRig.LocalRig.head.trackingRotationOffset.z = 180f;

        public static void GrabBug()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GameObject.Find("Floating Bug Holdable").transform.position = GorillaTagger.Instance.rightHandTransform.position;
                GameObject.Find("Floating Bug Holdable").transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
            }
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                GameObject.Find("Floating Bug Holdable").transform.position = GorillaTagger.Instance.leftHandTransform.position;
                GameObject.Find("Floating Bug Holdable").transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
            }
        }
        public static void GrabBat()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GameObject.Find("Cave Bat Holdable").transform.position = GorillaTagger.Instance.rightHandTransform.position;
                GameObject.Find("Cave Bat Holdable").transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
            }
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                GameObject.Find("Cave Bat Holdable").transform.position = GorillaTagger.Instance.leftHandTransform.position;
                GameObject.Find("Cave Bat Holdable").transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
            }
        }

        private static void SpawnWater(Vector3 pos, Quaternion rot, float scale, float radius, bool big, bool enter) =>
            GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", Photon.Pun.RpcTarget.All, new object[] { pos, rot, scale, radius, big, enter });

        public static float Radius = 1.5f; // nova please name this better then me -sleepy
        public static float AngleStep = 25f; // nova please name this better then me -sleepy
        public static float HeightStep = 0.12f; // nova please name this better then me -sleepy
        public static float MaximumHeight = 2.5f; // nova please name this better then me -sleepy
        public static float MaximumDistanceSquared = 9f; // nova please name this better then me -sleepy
        public static float _lastUpdateTime; // nova please name this better then me -sleepy
        public static float _angleDegrees; // nova please name this better then me -sleepy
        public static float _height; // nova please name this better then me -sleepy
        public static void WaterHelixSplash()
        {
            if (Time.time > _lastUpdateTime)
            {
                _lastUpdateTime = Time.time + 0.06f;
                Vector3 origin = VRRig.LocalRig.transform.position;
                SpawnWater(PointOnHelix(origin, _angleDegrees), VRRig.LocalRig.transform.rotation, 5, 100f, true, false);
                SpawnWater(PointOnHelix(origin, _angleDegrees + 180f), VRRig.LocalRig.transform.rotation, 5, 100f, true, false);
                Safety.RPCProc();

                _angleDegrees = (_angleDegrees + AngleStep) % 360f;
                _height += HeightStep;
                if (_height > MaximumHeight)
                {
                    _height = 0f;
                }
            }
        }
        public static Vector3 PointOnHelix(Vector3 origin, float angleDegrees) // fuck math atp -sleepy || nova please name this better then me -sleepy
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            return origin + new Vector3(Mathf.Cos(angleRadians) * Radius, _height, Mathf.Sin(angleRadians) * Radius);
        }

        public static void WaterSplashSelf()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
                SpawnWater(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.rotation, 5f, 100f, true, false);
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
                SpawnWater(GorillaTagger.Instance.leftHandTransform.position, GorillaTagger.Instance.leftHandTransform.rotation, 5f, 100f, true, false);
        }

        public static void WaterGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                if (GetGunInput(true))
                {
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = NewPointer.transform.position;
                    SpawnWater(VRRig.LocalRig.transform.position, VRRig.LocalRig.transform.rotation, 5f, 100f, true, false);
                }
                else
                {
                    VRRig.LocalRig.enabled = true;
                }
            }
        }
        public static void MaxQuestScore() =>
            VRRig.LocalRig.SetQuestScore(int.MaxValue);

        public static void OpenBasementDoor() =>
            GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/DungeonDoor_Prefab").GetComponent<PhotonView>().RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Opening);
        public static void CloseBasementDoor() =>
            GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/DungeonDoor_Prefab").GetComponent<PhotonView>().RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Closing);
        public static void OpenElevatorDoor() =>
            GRElevatorManager.ElevatorButtonPressed(GRElevator.ButtonType.Open, GRElevatorManager._instance.currentLocation);
        public static void CloseElevatorDoor() =>
            GRElevatorManager.ElevatorButtonPressed(GRElevator.ButtonType.Close, GRElevatorManager._instance.currentLocation);


        public static void HoldGlider()
        {
            foreach (GliderHoldable glider in GetGliders())
            {
                if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
                {
                    if (!glider.IsMine)
                        glider.OnHover(null, null);
                    else
                        glider.gameObject.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                }
                if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
                {
                    if (!glider.IsMine)
                        glider.OnHover(null, null);
                    else
                        glider.gameObject.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                }
            }
        }

        public static void GliderGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                if (GetGunInput(true))
                {
                    foreach (GliderHoldable glider in GetGliders())
                    {
                        if (!glider.IsMine)
                            glider.OnHover(null, null);
                        else
                            glider.gameObject.transform.position = NewPointer.transform.position;
                    }
                }
            }
        }

        public static void SpawnHoverboard()
        {
            FreeHoverboardManager.instance.SendDropBoardRPC(GorillaTagger.Instance.rightHandTransform.transform.position, Quaternion.identity, Vector3.zero, Vector3.zero, VRRig.LocalRig.playerColor);
            GTPlayer.Instance.isHoverAllowed = true;
            GTPlayer.Instance.SetHoverActive(true);
            Safety.RPCProc();
        }
        public static void SpawnHoverboardSpam()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                FreeHoverboardManager.instance.SendDropBoardRPC(GorillaTagger.Instance.rightHandTransform.transform.position, Quaternion.identity, Vector3.zero, Vector3.zero, VRRig.LocalRig.playerColor);
                Safety.RPCProc();
            }
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                FreeHoverboardManager.instance.SendDropBoardRPC(GorillaTagger.Instance.leftHandTransform.transform.position, Quaternion.identity, Vector3.zero, Vector3.zero, VRRig.LocalRig.playerColor);
                Safety.RPCProc();
            }
        }

        public static void BoardGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                if (GetGunInput(true))
                {
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = NewPointer.transform.position;
                    FreeHoverboardManager.instance.SendDropBoardRPC(NewPointer.transform.position, Quaternion.identity, Vector3.zero, Vector3.zero, VRRig.LocalRig.playerColor);
                    Safety.RPCProc();
                }
                else
                {
                    VRRig.LocalRig.enabled = true;
                }
            }
        }
        public static void GiveAllResources()
        {
            foreach (SIResource.ResourceType type in Enum.GetValues(typeof(SIResource.ResourceType)))
            {
                SIProgression.Instance.resourceDict[type] = 999999;
            }
            SIPlayer.SetAndBroadcastProgression();
        }
        public static void SIUnlockAll()
        {
            foreach (bool[] gadget in SIProgression.Instance.unlockedTechTreeData)
                Array.Fill(gadget, true);
        }
        public static float flashTimer = 0f;

        public static void FlashVIMNameTag()
        {
            if (Time.time > flashTimer)
            {
                flashTimer = Time.time + 0.1f;
                if (VRRig.LocalRig.ShowGoldNameTag)
                {
                    VRRig.LocalRig.ShowGoldNameTag = false;
                    VRRig.LocalRig.playerText1.color = Color.white;
                }
                else
                {
                    VRRig.LocalRig.ShowGoldNameTag = true;
                    VRRig.LocalRig.playerText1.color = SubscriptionManager.SUBSCRIBER_NAME_COLOR;
                }
            }
        }

        private static List<TransferrableObject> cachedHoldables = new List<TransferrableObject>();
        private static float lastCacheTime = 0f;
        private static float cacheInterval = 0.5f;

        private static void RHC()
        {
            if (Time.time - lastCacheTime > cacheInterval)
            {
                cachedHoldables.Clear();
                var found = Resources.FindObjectsOfTypeAll<TransferrableObject>();
                foreach (var obj in found)
                {
                    if (obj != null)
                    {
                        cachedHoldables.Add(obj);
                    }
                }
                lastCacheTime = Time.time;
            }
        }
        public static void StickyHoldables()
        {
            try
            {
                foreach (TransferrableObject tobj in GetHoldables())
                {
                    if (tobj.IsMyItem())
                    {
                        if (tobj.InRightHand())
                        {
                            tobj.currentState = TransferrableObject.PositionState.InRightHand;
                            tobj.transform.position = GTPlayer.Instance.RightHand.controllerTransform.position;
                        }
                        if (tobj.InLeftHand())
                        {
                            tobj.currentState = TransferrableObject.PositionState.InLeftHand;
                            tobj.transform.position = GTPlayer.Instance.LeftHand.controllerTransform.position;
                        }
                    }
                }
            }
            catch { }
        }
        public static void SpinHoldables()
        {
            try
            {
                RHC();
                foreach (var holdable in cachedHoldables)
                {
                    try
                    {
                        if (holdable == null || holdable.transform == null)
                            continue;
                        if (holdable.currentState == TransferrableObject.PositionState.InLeftHand || holdable.currentState == TransferrableObject.PositionState.InRightHand)
                        {
                            holdable.transform.rotation = RandomQuaternion(360);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static float nextJuggleTime = 0f;
        private static float juggleInterval = 0.3f;
        private static int positionIndex = 0;
        private static readonly TransferrableObject.PositionState[] allPositions = new TransferrableObject.PositionState[]
        {
            TransferrableObject.PositionState.InLeftHand,
            TransferrableObject.PositionState.InRightHand,
            TransferrableObject.PositionState.OnLeftArm,
            TransferrableObject.PositionState.OnRightArm,
            TransferrableObject.PositionState.OnLeftShoulder,
            TransferrableObject.PositionState.OnRightShoulder,
            TransferrableObject.PositionState.OnChest,
            TransferrableObject.PositionState.Dropped
        };
        public static void JuggleHoldables()
        {
            try
            {
                if (Time.time < nextJuggleTime) return;
                nextJuggleTime = Time.time + juggleInterval;
                RHC();
                positionIndex = (positionIndex + 1) % allPositions.Length;
                foreach (var holdable in cachedHoldables)
                {
                    try
                    {
                        if (holdable == null || !holdable.gameObject.activeInHierarchy) continue;
                        if (holdable.currentState != TransferrableObject.PositionState.None)
                        {
                            holdable.currentState = allPositions[positionIndex];
                            if (allPositions[positionIndex] == TransferrableObject.PositionState.InLeftHand && holdable.canAutoGrabLeft)
                            {
                                holdable.OnGrab(holdable.gripInteractor, EquipmentInteractor.instance.leftHand);
                            }
                            else if (allPositions[positionIndex] == TransferrableObject.PositionState.InRightHand && holdable.canAutoGrabRight)
                            {
                                holdable.OnGrab(holdable.gripInteractor, EquipmentInteractor.instance.rightHand);
                            }
                            else if (allPositions[positionIndex] == TransferrableObject.PositionState.Dropped)
                            {
                                holdable.DropItem();
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
        public static void OrbitHoldables()
        {
            try
            {
                if (cachedHoldables == null || cachedHoldables.Count == 0)
                    RHC();
                int count = cachedHoldables.Count;
                float time = Time.time * 2f;
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        var holdable = cachedHoldables[i];
                        if (holdable == null || !holdable.gameObject.activeInHierarchy)
                            continue;
                        float angle = (360f / count) * i + time * Mathf.Rad2Deg;
                        float rad = angle * Mathf.Deg2Rad;
                        Vector3 offset = new Vector3(Mathf.Cos(rad) * 1.2f, 0.2f, Mathf.Sin(rad) * 1.2f);
                        holdable.DropItem();
                        holdable.transform.position = VRRig.LocalRig.transform.position + offset;
                        holdable.transform.rotation = Quaternion.LookRotation((holdable.transform.position - VRRig.LocalRig.transform.position).normalized, Vector3.up );
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static IEnumerator ProcessCosmetics(int mode = 0)
        {
            bool inCity = GameObject.Find("City_Pretty") != null;
            bool inMountain = GameObject.Find("Mountain") != null;
            if (!inCity && !inMountain)
            {
                yield break;
            }
            Vector3 targetPosition = inCity ? new Vector3(-52f, 17.50f, -120f) : new Vector3(-14.62f, 18.11f, -111.72f);
            if (!VRRig.LocalRig.inTryOnRoom)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = targetPosition;
            }
            CosmeticsController cosmetics = CosmeticsController.instance;
            string cosmeticId = (mode == -1 || mode == 2) ? "NOTHING" : "LBAAI.";
            string[] cosmeticArray = Enumerable.Repeat(cosmeticId, 16).ToArray();
            CosmeticsController.CosmeticSet cosmeticSet = new CosmeticsController.CosmeticSet(cosmeticArray, cosmetics);
            cosmetics.currentWornSet = cosmeticSet;
            VRRig.LocalRig.cosmeticSet = cosmeticSet;
            if (!PhotonNetwork.InRoom)
            {
                VRRig.LocalRig.LocalUpdateCosmeticsWithTryon(cosmeticSet, cosmetics.tryOnSet, false);
            }
            else
            {
                GorillaTagger.Instance.myVRRig.SendRPC( "RPC_UpdateCosmeticsWithTryonPacked", RpcTarget.All,
                    new object[] { cosmeticSet.ToPackedIDArray(), cosmetics.tryOnSet.ToPackedIDArray(), false });
            }
            yield return new WaitForSeconds(0.1f);
            if (mode == -1)
            {
                VRRig.LocalRig.enabled = true;
                yield break;
            }
            if (mode != 2)
            {
                VRRig.LocalRig.enabled = false;
                Vector3 abovePosition = targetPosition + Vector3.up * 7f;
                VRRig.LocalRig.transform.position = abovePosition;
                yield return new WaitForSeconds(0.1f);
                VRRig.LocalRig.transform.position = targetPosition;
                yield return new WaitForSeconds(0.1f);
                VRRig.LocalRig.transform.position = abovePosition;
                yield return new WaitForSeconds(0.1f);
                VRRig.LocalRig.transform.position = targetPosition;
            }
            if (mode > 0)
            {
                List<CosmeticsController.CosmeticItem> cosmeticsList = new List<CosmeticsController.CosmeticItem>();
                if (mode == 1 || mode == 2)
                {
                    cosmeticsList = cosmetics.allCosmetics.Where(item => item.canTryOn && (int)item.itemCategory != 3 && !item.isHoldable
                    && !item.isThrowable && (int)item.itemCategory != 6 && (int)item.itemCategory != 2 && (int)item.itemCategory != 11).ToList();
                }
                else if (mode == 3)
                {
                   cosmeticsList = cosmetics.allCosmetics.Where(item => item.canTryOn && item.isHoldable).ToList();
                }
                foreach (var item in cosmeticsList)
                {
                    cosmetics.ApplyCosmeticItemToSet(cosmetics.tryOnSet, item, false, false);
                    cosmetics.UpdateWornCosmetics(true);
                    yield return new WaitForSeconds(0.03f);
                    if (mode != 2)
                    {
                        cosmetics.ApplyCosmeticItemToSet(cosmetics.tryOnSet, item, false, false);
                        cosmetics.UpdateWornCosmetics(true);
                    }
                }
            }
            VRRig.LocalRig.enabled = true;
        }

        private static int chi = 0;
        private static float hatTryOnDelay = 0f;
        public static void SpazTryonHats()
        {
            if (Time.time > hatTryOnDelay)
            {
                List<CosmeticsController.CosmeticItem> hats = CosmeticsController.instance.allCosmetics.Where(x =>
                x.itemCategory == CosmeticsController.CosmeticCategory.Hat && x.canTryOn).ToList();
                CosmeticsController.CosmeticItem item = hats[chi];
                foreach (FittingRoomButton button in GameObject.FindObjectsByType<FittingRoomButton>(FindObjectsSortMode.None))
                {
                    button.currentCosmeticItem = item;
                    button.ButtonActivationWithHand(false);
                }
                chi = (chi + 1) % hats.Count;
                hatTryOnDelay = Time.time + 0.5f;
            }
        }

        public static void SpazTryonBadges()
        {
            if (Time.time > hatTryOnDelay)
            {
                List<CosmeticsController.CosmeticItem> hats = CosmeticsController.instance.allCosmetics.Where(x =>
                x.itemCategory == CosmeticsController.CosmeticCategory.Badge && x.canTryOn).ToList();
                CosmeticsController.CosmeticItem item = hats[chi];
                foreach (FittingRoomButton button in GameObject.FindObjectsByType<FittingRoomButton>(FindObjectsSortMode.None))
                {
                    button.currentCosmeticItem = item;
                    button.ButtonActivationWithHand(false);
                }
                chi = (chi + 1) % hats.Count;
                hatTryOnDelay = Time.time + 0.5f;
            }
        }

        public static void SpazTryonFace()
        {
            if (Time.time > hatTryOnDelay)
            {
                List<CosmeticsController.CosmeticItem> hats = CosmeticsController.instance.allCosmetics.Where(x =>
                x.itemCategory == CosmeticsController.CosmeticCategory.Face && x.canTryOn).ToList();
                CosmeticsController.CosmeticItem item = hats[chi];
                foreach (FittingRoomButton button in GameObject.FindObjectsByType<FittingRoomButton>(FindObjectsSortMode.None))
                {
                    button.currentCosmeticItem = item;
                    button.ButtonActivationWithHand(false);
                }
                chi = (chi + 1) % hats.Count;
                hatTryOnDelay = Time.time + 0.5f;
            }
        }

        public static void SpazTryonHoldables()
        {
            if (Time.time > hatTryOnDelay)
            {
                List<CosmeticsController.CosmeticItem> hats = CosmeticsController.instance.allCosmetics.Where(x =>
                x.itemCategory == CosmeticsController.CosmeticCategory.Arms && x.canTryOn).ToList();
                CosmeticsController.CosmeticItem item = hats[chi];
                foreach (FittingRoomButton button in GameObject.FindObjectsByType<FittingRoomButton>(FindObjectsSortMode.None))
                {
                    button.currentCosmeticItem = item;
                    button.ButtonActivationWithHand(false);
                }
                chi = (chi + 1) % hats.Count;
                hatTryOnDelay = Time.time + 0.5f;
            }
        }

        public static void UnlockSubscription(bool active) // By sleepy
        {
            if (SubscriptionManager.Instance == null) return;
            Type subscriptionDetailsType = typeof(SubscriptionManager.SubscriptionDetails);
            object details = Activator.CreateInstance(subscriptionDetailsType);
            subscriptionDetailsType.GetField("active").SetValue(details, active);
            subscriptionDetailsType.GetField("daysAccrued").SetValue(details, int.MaxValue);
            subscriptionDetailsType.GetField("tier").SetValue(details, int.MaxValue);
            subscriptionDetailsType.GetField("autoRenew").SetValue(details, active);
            subscriptionDetailsType.GetField("autoRenewMonths").SetValue(details, int.MaxValue);
            subscriptionDetailsType.GetField("subscriptionActiveUntilDate").SetValue(details, DateTime.MaxValue);
            FieldInfo subscriptionFeatureSettingsField = subscriptionDetailsType.GetField("subscriptionFeatureSettings");
            if (subscriptionFeatureSettingsField != null)
            {
                subscriptionFeatureSettingsField.SetValue(details, new[] { active, active });
            }
            typeof(SubscriptionManager).GetField("localSubscriptionDetails", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, details);
            typeof(SubscriptionManager).GetField("_localSubscriptionDataInitialized", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, active);
            if (NetworkSystem.Instance != null && NetworkSystem.Instance.LocalPlayer != null)
            {
                MethodInfo updateMethod = typeof(SubscriptionManager).GetMethod("UpdatePlayerSubsDetails", BindingFlags.NonPublic | BindingFlags.Instance);
                if (updateMethod != null)
                {
                    updateMethod.Invoke(SubscriptionManager.Instance, new object[] { NetworkSystem.Instance.LocalPlayer, active, int.MaxValue });
                }
            }
        }
        private static float cameraDelay = 0f;

        public static void GrabCamera()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                LckSocialCamera camera = LckSocialCameraManager.Instance._networkedCococam;
                camera.visible = true;
                camera.recording = true;
                camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
                camera.m_CameraVisuals.SetRecordingState(true);
                camera.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                camera.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
            }
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                LckSocialCamera camera = LckSocialCameraManager.Instance._networkedCococam;
                camera.visible = true;
                camera.recording = true;
                camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
                camera.m_CameraVisuals.SetRecordingState(true);
                camera.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                camera.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
            }
        }
        public static void OrbitCamera()
        {
            float angle = Time.time * 10f;
            Vector3 orbitPos = GorillaTagger.Instance.headCollider.transform.position +
                new Vector3(Mathf.Cos(angle) * 2f, 1f, Mathf.Sin(angle) * 2f);
            LckSocialCamera camera = LckSocialCameraManager.Instance._networkedCococam;
            camera.visible = true;
            camera.recording = true;
            camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
            camera.m_CameraVisuals.SetRecordingState(true);
            camera.transform.position = orbitPos;
            camera.transform.LookAt(GTPlayer.Instance.transform.position);
        }
        public static void DestroyCamera()
        {
            LckSocialCamera camera = LckSocialCameraManager.Instance._networkedCococam;
            camera.visible = false;
            camera.recording = false;
            camera.m_CameraVisuals.SetNetworkedVisualsActive(false);
            camera.m_CameraVisuals.SetRecordingState(false);
            camera.transform.position = new Vector3(999f, 999f, 999f);
            camera.transform.SetParent(null);
        }
        public static void FlashCameraRecording()
        {
            LckSocialCamera camera = LckSocialCameraManager.Instance._networkedCococam;

            if (Time.time > cameraDelay)
            {
                cameraDelay = Time.time + 0.5f;
                var isRecordingField = typeof(SocialCoconutCamera).GetField("_isActive", BindingFlags.NonPublic | BindingFlags.Instance);
                if (isRecordingField != null)
                {
                    bool current = (bool)isRecordingField.GetValue(camera);
                    camera.recording = !current;
                    camera.visible = true;
                }
            }
        }

        public static void GrabTablet()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                LckSocialCamera camera = LckSocialCameraManager.Instance._networkedTablet;
                if (camera != null)
                {
                    camera.visible = true;
                    camera.recording = true;
                    camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
                    camera.m_CameraVisuals.SetRecordingState(true);
                    camera.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    camera.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                    camera.transform.SetParent(GorillaTagger.Instance.rightHandTransform);
                }
            }
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                LckSocialCamera camera = LckSocialCameraManager.Instance._networkedTablet;
                if (camera != null)
                {
                    camera.visible = true;
                    camera.recording = true;
                    camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
                    camera.m_CameraVisuals.SetRecordingState(true);
                    camera.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    camera.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                    camera.transform.SetParent(GorillaTagger.Instance.leftHandTransform);
                }
            }
        }
        public static void OrbitTablet()
        {
            LckSocialCamera tablet = LckSocialCameraManager.Instance._networkedTablet;
            if (tablet != null)
            {
                tablet.visible = true;
                tablet.recording = true;
                tablet.m_CameraVisuals.SetNetworkedVisualsActive(true);
                tablet.m_CameraVisuals.SetRecordingState(true);
                float angle = Time.time * 10f;
                Vector3 orbitPos = GTPlayer.Instance.transform.position +
                new Vector3(Mathf.Cos(angle) * 2f, 1f, Mathf.Sin(angle) * 2f);
                tablet.transform.position = orbitPos;
                tablet.transform.LookAt(GTPlayer.Instance.transform.position);
            }
        }

        public static void DestroyTablet()
        {
            LckSocialCamera tablet = LckSocialCameraManager.Instance._networkedTablet;
            if (tablet != null)
            {
                tablet.visible = false;
                tablet.TurnOff();
                GameObject.Destroy(tablet);
            }
        }
        public static void VIMDimGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    ChunkComponent c = Ray.collider.GetComponent<ChunkComponent>();
                    if (c != null)
                    {
                        VoxelWorld world = c.World;
                        VoxelAction action = default;
                        action.strength = Settings.digsize;
						action.radius = Settings.digsize;
                        action.operation = 0;
                        VoxelExtensions.Mine(world, Ray, action);
                    }
                }
            }
        }

        public static void ForestSnowGround(bool growing) =>
            GameObject.Find("pit ground bottom").GetComponent<GorillaSurfaceOverride>().overrideIndex = !growing ? 32 : 339;
        public static void DisableForestSnowGround() =>
            GameObject.Find("pit ground bottom").GetComponent<GorillaSurfaceOverride>().overrideIndex = 7;

        public static void RandomColorSnowballs()
        {
            var throwableL = SnowballMaker.leftHandInstance.snowballs.FirstOrDefault();
            var throwableR = SnowballMaker.rightHandInstance.snowballs.FirstOrDefault();

            GrowingSnowballThrowable bigSnowballL = throwableL as GrowingSnowballThrowable;
            GrowingSnowballThrowable bigSnowballR = throwableR as GrowingSnowballThrowable;
            if (bigSnowballL != null || bigSnowballR != null)
            {
                bigSnowballL.randomizeColor = true;
                bigSnowballL.ApplyColor(UnityEngine.Random.ColorHSV());

                bigSnowballR.randomizeColor = true;
                bigSnowballR.ApplyColor(UnityEngine.Random.ColorHSV());
            }
            else
            {
                throwableL.randomizeColor = true;
                throwableL.ApplyColor(UnityEngine.Random.ColorHSV());

                throwableR.randomizeColor = true;
                throwableR.ApplyColor(UnityEngine.Random.ColorHSV());
            }
        }

        public static void ResetSnowball()
        {
            var throwableL = SnowballMaker.leftHandInstance.snowballs.FirstOrDefault();
            var throwableR = SnowballMaker.rightHandInstance.snowballs.FirstOrDefault();

            GrowingSnowballThrowable bigSnowballL = throwableL as GrowingSnowballThrowable;
            GrowingSnowballThrowable bigSnowballR = throwableR as GrowingSnowballThrowable;
            if (bigSnowballL != null || bigSnowballR != null)
            {
                bigSnowballL.randomizeColor = false;

                bigSnowballR.randomizeColor = false;
            }
            else
            {
                throwableL.randomizeColor = false;

                throwableR.randomizeColor = false;
            }
        }

        public static void BraceletToggle(bool enable, bool Lefthand)
        {
            if (!PhotonNetwork.InRoom)
                return;
            GorillaTagger.Instance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, enable, Lefthand);
            Safety.RPCProc();
        }
        public static void RainbowMonkey()
        {
            Vector3 color = new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            CosmeticsController.instance.UpdateMonkeColor(color, false);
        }
        public static void MonkeyBlocksSizeChanger()
        {
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive))
                VRRig.LocalRig.sizeManager.currentSizeLayerMaskValue = 13;
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Left, !XRSettings.isDeviceActive))
                VRRig.LocalRig.sizeManager.currentSizeLayerMaskValue = 2;
        }
        public static void MultiBlock()
        {
            BuilderPieceInteractor.instance.handState[1] = BuilderPieceInteractor.HandState.Empty;
            BuilderPieceInteractor.instance.heldPiece[1] = null;
        }
        public static void BuyAllFree()
        {
            string mapName = GetCurrentMapName();
            if (mapName == "City" || mapName == "Mountain")
            {
                foreach (CosmeticsController.CosmeticItem controller in CosmeticsController.instance.allCosmetics)
                {
                    if (controller.canTryOn && controller.cost == 0 && !CosmeticsController.instance.unlockedCosmetics.Contains(controller))
                    {
                        CosmeticsController.instance.itemToBuy = controller;
                        CosmeticsController.instance.PurchaseItem();
                    }
                }
            }
            else
            {
                NotificationManager.SendNotification("<color=red>[ERROR]</color> You are not in city or mountains.");
            }
        }

        public static void RequestFortune()
        {
            PhotonView fortuneView = GameObject.Find("Environment Objects/05Maze_PersistentObjects/FortuneTeller_Persistent/FortuneTeller_Atrium").GetComponent<PhotonView>();
            fortuneView.RPC("RequestFortuneRPC", RpcTarget.All, new object[] { });
        }

        public static string name;
        public static void AnimatedName()
        {
            if (string.IsNullOrEmpty(name)) name = PhotonNetwork.LocalPlayer.NickName;
            if (!NetworkSystem.Instance.InRoom)
            {
                GorillaComputer.instance.currentName = name;
                GorillaComputer.instance.SetLocalNameTagText(GorillaComputer.instance.currentName);
                GorillaComputer.instance.savedName = GorillaComputer.instance.currentName;
                PlayerPrefs.SetString("playerName", GorillaComputer.instance.currentName);
                PlayerPrefs.Save();
                PhotonNetwork.LocalPlayer.NickName = name;
                return;
            }
            int length = Mathf.Clamp((int)Mathf.PingPong(Time.time / 0.25f, name.Length) + 1, 1, name.Length);
            GorillaComputer.instance.currentName = name[..length];
            GorillaComputer.instance.SetLocalNameTagText(GorillaComputer.instance.currentName);
            GorillaComputer.instance.savedName = GorillaComputer.instance.currentName;
            PlayerPrefs.SetString("playerName", GorillaComputer.instance.currentName);
            PlayerPrefs.Save();
            PhotonNetwork.LocalPlayer.NickName = name[..length];
            Safety.RPCProc();
        }


        public static void SpamCritters()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(c => c != null).ToList();
                    CrittersPawn target = critters[UnityEngine.Random.Range(0, critters.Count)];
                    target.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    target.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                }
                else
                {
                    CrittersGrabber lg = GameObject.FindObjectsOfType<CrittersGrabber>().FirstOrDefault(g => g.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && g.isLeft);
                    var bodyPos = GorillaTagger.Instance.bodyCollider.transform.position;
                    List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(c => c != null).OrderBy(c => Vector3.Distance(c.transform.position, bodyPos) switch
                    {
                        float d when d > 3f && d < 25f => 0,
                        float d when d < 25f => 1,
                        _ => 2
                    }).ToList();
                    if (critters.Count == 0) return;
                    CrittersPawn critter = critters[UnityEngine.Random.Range(0, critters.Count)];
                    critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    critter.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                    if (lg != null)
                        CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby", CrittersManager.instance.guard.currentOwner, critter.actorId, lg.actorId, Quaternion.identity, Vector3.zero, false);
                    CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, Vector3.zero, Vector3.zero);
                }
            }
        }

        public static void BreakAudioGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    BreakAudio(lockTarget.Creator, 111);
                }

                if (GetGunInput(true))
                {
                    VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                    if (!rig.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = rig;
                    }
                }
            }
            else
            {
                gunLocked = false;
                lockTarget = null;
            }
        }

        public static void BreakAudioAll()
        {
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                BreakAudio(RpcTarget.All, 111);
            }
        }

        private static void BreakAudio(NetPlayer player, int soundIndex)
        {
            if (NetworkSystem.Instance.InRoom)
            {
                if (player != null)
                {
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", player, new object[] { soundIndex, false, 999999999f });
                }
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(soundIndex, false, 99999999f);
            }
        }
        private static void BreakAudio(RpcTarget target, int soundIndex)
        {
            if (NetworkSystem.Instance.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", target, new object[] { soundIndex, false, 999999999f });
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(soundIndex, false, 99999999f);
            }
        }
    }
}