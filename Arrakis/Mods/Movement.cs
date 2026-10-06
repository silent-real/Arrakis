/*
 * Arrakis | Mods/Movement.cs
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

using Arrakis.Extensions;
using Arrakis.Managers;
using Arrakis.Patches.Patchers;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static Arrakis.Menu.Main;

namespace Arrakis.Mods
{
    public class Movement
    {
        public static void Fly()
        {
            if (InputManager.GetInput(InputManager.InputType.Primary, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * Settings.flyspeed;
                GTPlayer.Instance.bodyCollider.attachedRigidbody.linearVelocity = Vector3.zero;
            }
        }
        public static void TriggerFly()
        {
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * Settings.flyspeed;
                GTPlayer.Instance.bodyCollider.attachedRigidbody.linearVelocity = Vector3.zero;
            }
        }
        public static void HandFly()
        {
            if (InputManager.GetInput(InputManager.InputType.Primary, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.RightHand.controllerTransform.transform.forward * Time.deltaTime * Settings.flyspeed;
                GTPlayer.Instance.bodyCollider.attachedRigidbody.linearVelocity = Vector3.zero;
            }
        }
        public static void NoclipFly()
        {
            if (InputManager.GetInput(InputManager.InputType.Primary, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * Settings.flyspeed;
                GTPlayer.Instance.bodyCollider.attachedRigidbody.linearVelocity = Vector3.zero;
                foreach (MeshCollider collider in GameObject.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None))
                    collider.enabled = !InputManager.GetInput(InputManager.InputType.Primary, InputManager.Hand.Right, !XRSettings.isDeviceActive);
            }
        }

        public static void MosaBoost() =>
            GTPlayer.Instance.maxJumpSpeed = 7.5f;
        public static void SpeedBoost() =>
            GTPlayer.Instance.maxJumpSpeed = 9.5f;
        public static void ExtremeSpeedBoost() =>
            GTPlayer.Instance.maxJumpSpeed = 15f;

        public static void SlideControl() =>
            GTPlayer.Instance.slideControl = 1.5f;
        public static void FixSlideControl() =>
            GTPlayer.Instance.slideControl = 0.0035429f;

        public static void PSA()
        {
            if (InputManager.GetInput(InputManager.InputType.Primary, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.bodyCollider.transform.forward * (Time.deltaTime * 5f);
                if (!GTPlayer.Instance.IsGroundedHand)
                {
                    if (GTPlayer.Instance.transform.position.y > 1f)
                    {
                        GorillaTagger.Instance.rigidbody.linearVelocity = new Vector3(GorillaTagger.Instance.rigidbody.linearVelocity.x,
                            -15f,GorillaTagger.Instance.rigidbody.linearVelocity.z);
                    }
                }
                else
                {
                    GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                }
            }
        }

        public static void ExcelFly()
        {
            if (InputManager.GetInput(InputManager.InputType.Primary, InputManager.Hand.Right, !XRSettings.isDeviceActive))
                GTPlayer.Instance.bodyCollider.attachedRigidbody.linearVelocity += GTPlayer.Instance.RightHand.controllerTransform.right / 2f;
            if (InputManager.GetInput(InputManager.InputType.Primary, InputManager.Hand.Left, !XRSettings.isDeviceActive))
                GTPlayer.Instance.bodyCollider.attachedRigidbody.linearVelocity += -GTPlayer.Instance.LeftHand.controllerTransform.right / 2f;
        }

        private static float y;
        private static float p;
        private static Vector3 pos = Vector3.zero;
        public static void WasdFly()
        {
            if (searching)
                return;

            bool w, a, s, d, space, control, shift;
            w = Keyboard.current.wKey.isPressed;
            a = Keyboard.current.aKey.isPressed;
            s = Keyboard.current.sKey.isPressed;
            d = Keyboard.current.dKey.isPressed;
            space = Keyboard.current.spaceKey.isPressed;
            control = Keyboard.current.leftCtrlKey.isPressed;
            shift = Keyboard.current.leftShiftKey.isPressed;
            GorillaTagger.Instance.rigidbody.useGravity = false;
            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            if (w)
                GorillaTagger.Instance.rigidbody.transform.position += GTPlayer.Instance.GetControllerTransform(false).parent.forward * Time.deltaTime * Settings.wasdflyspeed;
            if (s)
                GorillaTagger.Instance.rigidbody.transform.position += -GTPlayer.Instance.GetControllerTransform(false).parent.forward * Time.deltaTime * Settings.wasdflyspeed;
            if (a)
                GorillaTagger.Instance.rigidbody.transform.position += -GTPlayer.Instance.GetControllerTransform(false).parent.right * Time.deltaTime * Settings.wasdflyspeed;
            if (d)
                GorillaTagger.Instance.rigidbody.transform.position += GTPlayer.Instance.GetControllerTransform(false).parent.right * Time.deltaTime * Settings.wasdflyspeed;
            if (space)
                GorillaTagger.Instance.rigidbody.transform.position += GTPlayer.Instance.GetControllerTransform(false).parent.up * Time.deltaTime * Settings.wasdflyspeed;
            if (control)
                GorillaTagger.Instance.rigidbody.transform.position += -GTPlayer.Instance.GetControllerTransform(false).parent.up * Time.deltaTime * Settings.wasdflyspeed;
            if (shift)
                Settings.wasdflyspeed = 20f;
            else
                Settings.wasdflyspeed = 10f;
            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 m = Mouse.current.delta.ReadValue();
                y += m.x * 0.2f;
                p -= m.y * 0.2f;
                p = Mathf.Clamp(p, -89f, 89f);
                GTPlayer.Instance.GetControllerTransform(false).parent.rotation = Quaternion.Euler(p, y, 0f);
            }
            VRRig.LocalRig.head.rigTarget.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
            if (!w && !a && !s && !d && !space && !control && pos != Vector3.zero)
                GorillaTagger.Instance.rigidbody.transform.position = pos;
            else
                pos = GorillaTagger.Instance.rigidbody.transform.position;
        }

        private static GameObject platR = null;
        private static GameObject platL = null;
        public static void Platforms(bool trigger = false, bool invis = false)
        {
            if (trigger ? InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive) : InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                if (platR == null)
                {
                    platR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platR.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    platR.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                    platR.transform.localScale = new Vector3(0.0125f, 0.28f, 0.3825f);
                    platR.GetComponent<Renderer>().material.color = Settings.backgroundColor.GetCurrentColor();
                    platR.GetComponent<Renderer>().enabled = !invis;
                    if (Settings.stickyplats)
                        FixStickyColliders(platR);
                }
            }
            else
            {
                if (platR != null)
                {
                    GameObject.Destroy(platR);
                    platR = null;
                }
            }
            if (trigger ? InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Left, !XRSettings.isDeviceActive) : InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                if (platL == null)
                {
                    platL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platL.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    platL.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                    platL.transform.localScale = new Vector3(0.0125f, 0.28f, 0.3825f);
                    platL.GetComponent<Renderer>().material.color = Settings.backgroundColor.GetCurrentColor();
                    platL.GetComponent<Renderer>().enabled = !invis;
                    if (Settings.stickyplats)
                        FixStickyColliders(platL);
                }
            }
            else
            {
                if (platL != null)
                {
                    GameObject.Destroy(platL);
                    platL = null;
                }
            }
        }

        public static void NoClip()
        {
            foreach (MeshCollider collider in GameObject.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None))
                collider.enabled = !InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive);
        }

        public static void NoTagFreeze(bool notagfreeze) =>
            GTPlayer.Instance.disableMovement = !notagfreeze;

        public static void SteamLongArms() =>
            GTPlayer.Instance.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
        public static void LongArms() =>
            GTPlayer.Instance.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        public static void DisableLongArms() =>
            GTPlayer.Instance.transform.localScale = new Vector3(1f, 1f, 1f);

        private static bool tp = false;
        public static void TPGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                if (GetGunInput(true))
                {
                    if (!tp)
                    {
                        GTPlayer.Instance.TeleportTo(NewPointer.transform.position, Quaternion.identity, true);
                        tp = true;
                    }
                }
                else
                    tp = false;
            }
        }
        public static GameObject lp;
        public static GameObject rp;
        public static void CreatePredThingy()
        {
            lp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(lp.GetComponent<BoxCollider>());
            lp.GetComponent<Renderer>().enabled = false;
            lp.AddComponent<GorillaVelocityTracker>();
            rp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(rp.GetComponent<BoxCollider>());
            rp.GetComponent<Renderer>().enabled = false;
            rp.AddComponent<GorillaVelocityTracker>();
        }
        public static void RemovePredThingy()
        {
            Object.Destroy(lp);
            Object.Destroy(rp);
        }
        public static void Preds()
        {
            lp.transform.position = GorillaTagger.Instance.headCollider.transform.position - GorillaTagger.Instance.leftHandTransform.position;
            rp.transform.position = GorillaTagger.Instance.headCollider.transform.position - GorillaTagger.Instance.rightHandTransform.position;
            GTPlayer.Instance.leftHand.controllerTransform.position -= lp.GetComponent<GorillaVelocityTracker>().GetAverageVelocity(true, 0) * 0.025f;
            GTPlayer.Instance.rightHand.controllerTransform.transform.position -= rp.GetComponent<GorillaVelocityTracker>().GetAverageVelocity(true, 0) * 0.025f;
        }
        public static void RigGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                if (GetGunInput(true))
                {
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = NewPointer.transform.position;
                }
                else
                    VRRig.LocalRig.enabled = true;
            }
        }

        public static void GrabRig()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                VRRig.LocalRig.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
            }
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                VRRig.LocalRig.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
            }
            if (!InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive) && !InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
                VRRig.LocalRig.enabled = true;
        }


        public static void FixRig() =>
            VRRig.LocalRig.enabled = true;

        public static void GhostMonkey()
        {
            if (InputManager.GetInput(InputManager.InputType.Secondary, InputManager.Hand.Right, !XRSettings.isDeviceActive))
                VRRig.LocalRig.enabled = false;
            else
                VRRig.LocalRig.enabled = true;
        }

        private static bool toggled;
        private static bool ghostButtonLast;
        private static bool invisButtonLast;
        public static void ToggleGhostMonkey()
        {
            bool button = InputManager.GetInput(InputManager.InputType.Secondary, InputManager.Hand.Right, !XRSettings.isDeviceActive);
            if (button && !ghostButtonLast)
                VRRig.LocalRig.enabled = !VRRig.LocalRig.enabled;
            ghostButtonLast = button;
        }

        public static void InvisMonke()
        {
            if (InputManager.GetInput(InputManager.InputType.Primary, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = new Vector3(3423f, 32432f, 324324f);
            }
            else
                VRRig.LocalRig.enabled = true;
        }
        public static void ToggleInvisMonkey()
        {
            bool button = InputManager.GetInput(InputManager.InputType.Primary, InputManager.Hand.Right, !XRSettings.isDeviceActive);
            if (button && !invisButtonLast)
            {
                bool enabled = !VRRig.LocalRig.enabled;
                VRRig.LocalRig.enabled = enabled;
                if (!enabled)
                    VRRig.LocalRig.transform.position = new Vector3(3423f, 32432f, 324324f);
            }
            invisButtonLast = button;
        }

        public static void SpazRig()
        {
            VRRig.LocalRig.head.rigTarget.eulerAngles = new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
            VRRig.LocalRig.leftHand.rigTarget.eulerAngles = new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
            VRRig.LocalRig.rightHand.rigTarget.eulerAngles = new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
        }
        public static void SpazHands()
        {
            VRRig.LocalRig.leftHand.rigTarget.eulerAngles = new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
            VRRig.LocalRig.rightHand.rigTarget.eulerAngles = new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
        }
        public static void SpazHead() =>
            VRRig.LocalRig.head.rigTarget.eulerAngles = new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));

        public static void Rotate(Quaternion rot) =>
            VRRig.LocalRig.transform.rotation = rot;

        public static void FakeFBT()
        {
            Rotate(Camera.main.transform.rotation);
            VRRig.LocalRig.head.MapMine(VRRig.LocalRig.lastScaleFactor, VRRig.LocalRig.playerOffsetTransform);
            VRRig.LocalRig.leftHand.MapMine(VRRig.LocalRig.lastScaleFactor, VRRig.LocalRig.playerOffsetTransform);
            VRRig.LocalRig.rightHand.MapMine(VRRig.LocalRig.lastScaleFactor, VRRig.LocalRig.playerOffsetTransform);
        }
        public static void SpazBody() =>
            Rotate(Random.rotation);

        static Vector3 normal2;
        static Vector3 vel1;
        static Vector3 vel2;
        static float dist2;
        static int layers;
        static bool LeftClose2;
        static bool DoOnce2;
        static float maxD2;
        public static float WallWalkPower = 8.8f;
        public static void WallWalk() // this works i sent sleper a vid -nova
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                if (!DoOnce2)
                {
                    maxD2 = 1f;
                    layers = int.MaxValue;
                    DoOnce2 = true;
                }
                RaycastHit raycastHit;
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.right, out raycastHit, 1f, layers);
                RaycastHit raycastHit2;
                Physics.Raycast(GorillaTagger.Instance.leftHandTransform.position, GorillaTagger.Instance.leftHandTransform.right, out raycastHit2, 1f, layers);
                if (raycastHit2.distance > raycastHit.distance)
                {
                    normal2 = raycastHit.normal;
                    dist2 = raycastHit.distance;
                }
                else
                {
                    normal2 = raycastHit2.normal;
                    dist2 = raycastHit2.distance;
                    LeftClose2 = true;
                }
                if (dist2 < maxD2)
                {
                    vel2 = normal2 * (WallWalkPower * Time.deltaTime);
                    GorillaTagger.Instance.bodyCollider.attachedRigidbody.linearVelocity -= vel2;
                }
                else
                {
                    GorillaTagger.Instance.bodyCollider.attachedRigidbody.useGravity = true;
                }
            }
            else
            {
                GorillaTagger.Instance.bodyCollider.attachedRigidbody.useGravity = true;
            }
        }
        public static void AutoKayflock()
        {
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GorillaTagger.Instance.rightHandTransform.position = RandomVector3(310);
                GTPlayer.Instance.transform.position += GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * Settings.flyspeed;
            }
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                GorillaTagger.Instance.leftHandTransform.position = RandomVector3(310);
                GTPlayer.Instance.transform.position += GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * Settings.flyspeed;
            }
        }

        private static GameObject checkpoint = null;
        public static void CheckPoint()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                if (checkpoint == null)
                {
                    checkpoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    checkpoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    checkpoint.GetComponent<Renderer>().material.color = Color.white;
                    GameObject.Destroy(checkpoint.GetComponent<Collider>());
                }
                checkpoint.transform.position = VRRig.LocalRig.rightHandTransform.position;
                checkpoint.transform.rotation = VRRig.LocalRig.rightHandTransform.rotation;
            }
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GTPlayer.Instance.transform.position = checkpoint.transform.position + new Vector3(0f, 0.5f, 0f);
                GameObject.Destroy(checkpoint, 0.2f);
                checkpoint = null;
            }
        }
        public static void DisableCheckPoint()
        {
            GameObject.Destroy(checkpoint);
            checkpoint = null;
        }

        public static void PiggybackGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;

                if (lockTarget != null && gunLocked)
                {
                    GTPlayer.Instance.transform.position = lockTarget.transform.position + new Vector3(0f, 0.7f, 0f);
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

        public static void FollowPlayerGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;

                if (lockTarget != null && gunLocked)
                {
                    GTPlayer.Instance.transform.position = Vector3.Lerp(GTPlayer.Instance.transform.position, lockTarget.transform.position, Time.deltaTime * 2f);
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

        public static void RigFollowPlayerGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;

                if (lockTarget != null && gunLocked)
                {
                    if (!VRRig.LocalRig.enabled)
                    {
                        VRRig.LocalRig.transform.position = Vector3.Lerp(VRRig.LocalRig.transform.position, lockTarget.transform.position, Time.deltaTime * 2f);
                        VRRig.LocalRig.head.rigTarget.transform.LookAt(lockTarget.headMesh.transform.position);
                    }
                }

                if (GetGunInput(true))
                {
                    VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                    if (!rig.IsLocal())
                    {
                        VRRig.LocalRig.enabled = false;
                        lockTarget = rig;
                        gunLocked = true;
                    }
                }
            }
            else
            {
                VRRig.LocalRig.enabled = true;
                lockTarget = null;
                gunLocked = false;
            }
        }

        public static void PlatformSpam()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(platform.GetComponent<Collider>());
                platform.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                platform.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                platform.transform.localScale = new Vector3(0.0125f, 0.28f, 0.3825f);
                platform.GetComponent<Renderer>().material.color = Settings.backgroundColor.GetCurrentColor();
                GameObject.Destroy(platform, 2f);
            }
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(platform.GetComponent<Collider>());
                platform.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                platform.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                platform.transform.localScale = new Vector3(0.0125f, 0.28f, 0.3825f);
                platform.GetComponent<Renderer>().material.color = Settings.backgroundColor.GetCurrentColor();
                GameObject.Destroy(platform, 2f);
            }
        }

        public static void Frozone()
        {
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                platform.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                platform.transform.localScale = new Vector3(0.0125f, 0.28f, 0.3825f);
                platform.GetOrAddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                platform.GetComponent<Renderer>().material.color = Settings.backgroundColor.GetCurrentColor();
                GameObject.Destroy(platform, 2f);
            }
            if (InputManager.GetInput(InputManager.InputType.Grip, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                platform.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                platform.transform.localScale = new Vector3(0.0125f, 0.28f, 0.3825f);
                platform.GetOrAddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                platform.GetComponent<Renderer>().material.color = Settings.backgroundColor.GetCurrentColor();
                GameObject.Destroy(platform, 2f);
            }
        }
        public static void PullBoost()
        {
            if (!InputManager.GetInput(InputManager.InputType.Joystick, InputManager.Hand.Right, !XRSettings.isDeviceActive)) return;
            if (!GTPlayer.Instance.leftHand.wasColliding && !GTPlayer.Instance.rightHand.wasColliding) return;
            Vector3 moveDir = GTPlayer.Instance.bodyCollider.transform.forward;
            float currentStrength = 15f;
            RaycastHit groundHit;
            if (Physics.Raycast(GTPlayer.Instance.transform.position + Vector3.up * 0.5f, Vector3.down, out groundHit, 2f))
            {
                float angle = Vector3.Angle(groundHit.normal, Vector3.up);
                if (angle > 5f)
                {
                    Vector3 slopeDir = Vector3.ProjectOnPlane(moveDir, groundHit.normal).normalized;
                    bool goingUp = Vector3.Dot(moveDir, groundHit.normal) < 0;
                    if (goingUp)
                    {
                        currentStrength = 15f * 0.8f;
                        moveDir = slopeDir;
                    }
                    else
                    {
                        currentStrength = 15f * 1.5f;
                        moveDir = slopeDir;
                    }
                }
            }
            Rigidbody rb = GTPlayer.Instance.bodyCollider.attachedRigidbody;
            Vector3 originalVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector3.zero;
            GTPlayer.Instance.transform.position += moveDir * (Time.deltaTime * currentStrength);
            rb.linearVelocity = originalVelocity;
        }

        public static bool LastTouchL;
        public static bool LastTouchR;
        public static void PullMod()
        {
            if ((!GTPlayer.Instance.IsHandTouching(true) && LastTouchL) && InputManager.GetInput(InputManager.InputType.Joystick, InputManager.Hand.Left, !XRSettings.isDeviceActive) || (!GTPlayer.Instance.IsHandTouching(false) && LastTouchR) && InputManager.GetInput(InputManager.InputType.Joystick, InputManager.Hand.Right, !XRSettings.isDeviceActive))
            {
                Vector3 velocity = GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity;
                GTPlayer.Instance.transform.position += new Vector3(velocity.x * 0.08f, 0f, velocity.z * 0.08f);
            }
            LastTouchL = GTPlayer.Instance.IsHandTouching(true);
            LastTouchR = GTPlayer.Instance.IsHandTouching(false);
        }

        public static void StopFlip()
        {
            GTPlayer.Instance.UnsetGravityOverride(GTPlayer.Instance);
            GTPlayerTransform.ApplyRotationOverride(Quaternion.identity, Time.frameCount);
        }

        public static bool flipping;
        public static float flipStart;
        public static Quaternion flipFrom;
        public static Vector3 flipAxis;
        public const float flipDuration = 1f;
        public static void Flip()
        {
            if (!flipping && (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive) || InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Left, !XRSettings.isDeviceActive)) && VRRig.LocalRig.enabled)
            {
                if (GTPlayer.Instance.playerRigidBody)
                {
                    flipping = true;
                    flipStart = Time.time;
                    flipAxis = InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive) ? VRRig.LocalRig.transform.right : -VRRig.LocalRig.transform.right;
                    flipFrom = GTPlayer.Instance?.playerRigidBody?.rotation ?? Quaternion.identity;
                }
            }
            if (!flipping) return;

            float t = (Time.time - flipStart) / flipDuration;
            if (t >= 1f)
            {
                flipping = false;
                GTPlayerTransform.ApplyRotationOverride(flipFrom, Time.frameCount);

                return;
            }
            var rot = Quaternion.AngleAxis(-360f * t, flipAxis) * flipFrom;
            GTPlayerTransform.ApplyRotationOverride(rot, Time.frameCount);
        }
        public static void ToggleTorsoPatch(bool enabled, int mode = 0)
        {
            TorsoPatch.enabled = enabled;
            TorsoPatch.mode = mode;
            if (!enabled && VRRigTorso != null)
                Object.Destroy(VRRigTorso);
        }
        public static GameObject VRRigTorso;
        public static void SmoothBody()
        {
            ToggleTorsoPatch(true, 3);
            if (VRRigTorso == null)
                VRRigTorso = new GameObject("Arrakis_vrrigtorso");
            VRRigTorso.transform.rotation = Quaternion.Lerp(VRRigTorso.transform.rotation,
                Quaternion.Euler(0f, GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * 6.5f);
        }

        private static LineRenderer lineL;
        private static LineRenderer lineR;
        public static void SpiderMan()
        {
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right))
            {
                if (lineR == null)
                {
                    lineR = new GameObject().AddComponent<LineRenderer>();
                    lineR.useWorldSpace = true;
                    lineR.startWidth = 0.02f;
                    lineR.endWidth = 0.02f;
                    lineR.startColor = Settings.backgroundColor.GetCurrentColor();
                    lineR.endColor = Settings.backgroundColor.GetCurrentColor();
                    lineR.positionCount = 2;
                }
                RaycastHit hit;
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward, out hit, 512f, NoInvisLayerMask());
                lineR.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                lineR.SetPosition(1, hit.point);
            }

            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right))
            {
                if (lineL == null)
                {
                    lineL = new GameObject().AddComponent<LineRenderer>();
                    lineL.useWorldSpace = true;
                    lineL.startWidth = 0.02f;
                    lineL.endWidth = 0.02f;
                    lineL.startColor = Settings.backgroundColor.GetCurrentColor();
                    lineL.endColor = Settings.backgroundColor.GetCurrentColor();
                    lineL.positionCount = 2;
                }
                RaycastHit hit;
                Physics.Raycast(GorillaTagger.Instance.leftHandTransform.position, GorillaTagger.Instance.leftHandTransform.forward, out hit, 512f, NoInvisLayerMask());
                lineL.SetPosition(0, GorillaTagger.Instance.leftHandTransform.position);
                lineL.SetPosition(1, hit.point);
            }
        }

        public static void DisableSpiderMan()
        {
            if (lineR != null)
            {
                GameObject.Destroy(lineR.gameObject);
                lineR = null;
            }
            if (lineL != null)
            {
                GameObject.Destroy(lineL.gameObject);
                lineL = null;
            }
        }

        public static void TeleportToMap(string map)
        {
            Vector3 targetPos;
            switch (map.ToLower())
            {
                case "stump": targetPos = new Vector3(-66.97f, 12.48f, -83.02f); break;
                case "canyons": targetPos = new Vector3(-80.89f, 12.09f, -100.46f); break;
                case "caves": targetPos = new Vector3(-73.29f, -14.52f, -39.86f); break;
                case "city": targetPos = new Vector3(-62.49f, 17.04f, -100.95f); break;
                case "beach": targetPos = new Vector3(-13.83f, 30f, -19.3f); break;
                case "clouds": targetPos = new Vector3(-76.34f, 164f, -98.04f); break;
                case "mountains": targetPos = new Vector3(-19.8f, 18.8f, -106.72f); break;
                case "mall": targetPos = new Vector3(-64.81f, 6.88f, -102.04f); break;
                default: return;
            }
            GTPlayer.Instance.TeleportTo(targetPos, GTPlayer.Instance.transform.rotation, false, true);
        }
    }
}
