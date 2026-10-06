/*
 * Arrakis | Mods/Advantage.cs
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
using Arrakis.Extensions;
using Arrakis.Managers;
using Arrakis.Notifications;
using Arrakis.Patches.Patchers;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR;
using static Arrakis.Menu.Main;

namespace Arrakis.Mods
{
    public class Advantage
    {
        public static void TagAll()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (GorillaTagManager tag in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                {
                    foreach (NetPlayer plr in NetworkSystem.Instance.PlayerListOthers)
                    {
                        if (!tag.currentInfected.Contains(plr))
                                tag.AddInfectedPlayer(plr);
                        Toggle("Tag All");
                        ReloadMenu();
                    }
                }
            }
            else
            {
                bool tagged = true;
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (rig != null && rig != VRRig.LocalRig)
                    {
                        if (!rig.IsTagged())
                        {
                            TagPlayer(rig.Creator);
                            tagged = false;
                        }
                    }
                }
                if (tagged)
                {
                    Toggle("Tag All");
                    ReloadMenu();
                }
            }
        }

        public static float TagAuraRange = 1.2f;
        public static void TagAura()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (!rig.IsLocal())
                    {
                        if (rig.IsTagged() || !VRRig.LocalRig.IsTagged())
                            continue;
                        if (VRRig.LocalRig.IsTagged() && !rig.IsTagged())
                        {
                            float distance = Vector3.Distance(VRRig.LocalRig.transform.position, rig.transform.position);
                            if (distance < TagAuraRange)
                            {
                                GameMode.ReportTag(rig.Creator);
                            }
                        }
                    }
                }
            }
        }

        public static void TagSelf()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (GorillaTagManager tag in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                {
                    if (!tag.currentInfected.Contains(NetworkSystem.Instance.LocalPlayer))
                        tag.AddInfectedPlayer(NetworkSystem.Instance.LocalPlayer);
                    Toggle("Tag Self");
                    return;
                }
            }
            else
            {
                if (Settings.instanttag)
                {
                    VRRig rig = VRRigCache.ActiveRigs.Where(r => !r.IsLocal() && r.IsTagged()).OrderBy(r => r.Distance(VRRig.LocalRig) + r.LatestVelocity().magnitude).FirstOrDefault();
                    EventPatches.Override = () =>
                    {
                        if (VRRig.LocalRig.IsTagged())
                            return true;

                        Experimental.MultiSerialize(true, new[] { VRRig.LocalRig.netView.GetView });
                        Vector3 positionArchive = VRRig.LocalRig.transform.position;
                        VRRig.LocalRig.transform.position = rig.rightHandTransform.transform.position;
                        Experimental.SendSerialize(VRRig.LocalRig.netView.GetView,
                            new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber, rig.Creator.ActorNumber } });
                        Safety.RPCProc();
                        VRRig.LocalRig.transform.position = positionArchive;

                        return false;
                    };
                }
                else
                {
                    bool tagged = true;
                    foreach (VRRig rig in VRRigCache.ActiveRigs)
                    {
                        if (rig != null && rig != VRRig.LocalRig)
                        {
                            if (!VRRig.LocalRig.IsTagged() && rig.IsTagged())
                            {
                                VRRig.LocalRig.enabled = false;
                                VRRig.LocalRig.transform.position = rig.transform.position;
                                GameMode.ReportTag(NetworkSystem.Instance.LocalPlayer);
                                tagged = false;
                            }
                        }
                    }
                    if (tagged)
                    {
                        VRRig.LocalRig.enabled = true;
                        Toggle("Tag Self");
                        return;
                    }
                }
            }
        }

        public static void TagGun()
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
                        TagPlayer(rig.Creator);
                    }
                }
                else
                    VRRig.LocalRig.enabled = true;
            }
        }

        public static void InstantTagGun()
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
                        TagPlayer(rig.Creator);
                    }
                }
                else
                    VRRig.LocalRig.enabled = true;
            }
        }
        public static void InstantTagPlayer(NetPlayer Target)
        {
            if (!VRRig.LocalRig.IsTagged() || Target.VRRig2().IsTagged())
                return;
            Vector3 acriv = VRRig.LocalRig.transform.position;
            VRRig.LocalRig.transform.position = GorillaGameManager.StaticFindRigForPlayer(Target).transform.position;
            Experimental.SendSerialize(VRRig.LocalRig.netView.GetView, new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });
            GameMode.ReportTag(Target);
            VRRig.LocalRig.transform.position = acriv;
            Safety.RPCProc();
        }

        public static void FlickTagGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    GTPlayer.Instance.RightHand.controllerTransform.position = NewPointer.transform.position;
                }
            }
        }

        private static GameObject hitboxleft;
        private static GameObject hitboxright;
        public static float hitboxScale = 0.3f;
        public static void Hitboxes()
        {
            if (hitboxleft == null)
            {
                hitboxleft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hitboxleft.transform.parent = GorillaTagger.Instance.leftHandTransform;
                hitboxleft.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                hitboxleft.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                hitboxleft.transform.localScale = new Vector3(hitboxScale, hitboxScale, hitboxScale);
                GameObject.Destroy(hitboxleft.GetComponent<SphereCollider>());
                hitboxleft.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                hitboxleft.GetComponent<Renderer>().material.color = new Color(0.5f, 0f, 0f, 0.5f);
            }
            if (hitboxright == null)
            {
                hitboxright = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hitboxright.transform.parent = GorillaTagger.Instance.rightHandTransform;
                hitboxright.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                hitboxright.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                hitboxright.transform.localScale = new Vector3(hitboxScale, hitboxScale, hitboxScale);
                GameObject.Destroy(hitboxright.GetComponent<SphereCollider>());
                hitboxright.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            }

            hitboxleft.GetComponent<Renderer>().material.color = new Color(Settings.backgroundColor.GetCurrentColor().r,
                Settings.backgroundColor.GetCurrentColor().g, Settings.backgroundColor.GetCurrentColor().b, 0.2f);

            hitboxright.GetComponent<Renderer>().material.color = new Color(Settings.backgroundColor.GetCurrentColor().r,
                Settings.backgroundColor.GetCurrentColor().g, Settings.backgroundColor.GetCurrentColor().b, 0.2f);
        }

        public static void DestroyHitboxes()
        {
            if (hitboxleft != null)
            {
                GameObject.Destroy(hitboxleft);
                hitboxleft = null;
            }
            if (hitboxright != null)
            {
                GameObject.Destroy(hitboxright);
                hitboxright = null;
            }
        }

        public static void NoTagOnJoin()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("didTutorial", false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
            PlayerPrefs.SetString("didTutorial", "");
            PlayerPrefs.Save();
        }

        public static void TagOnJoin()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("didTutorial", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
            PlayerPrefs.SetString("didTutorial", "done");
            PlayerPrefs.Save();
        }
        public static void AntiTag()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (GorillaTagManager tag in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                {
                    if (!tag.currentInfected.Contains(NetworkSystem.Instance.LocalPlayer))
                    {
                        PhotonHandler handler = GameObject.Find("PhotonMono").GetComponent<PhotonHandler>();
                        Traverse.Create(handler).Field("nextSendTickCountOnSerialize").SetValue((int)(Time.realtimeSinceStartup * 9999));
                        List<int> Targets = new List<int>();
                        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerListOthers)
                        {
                            if (player.IsMasterClient)
                                continue;
                            Targets.Add(player.ActorNumber);
                        }
                        Experimental.SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = Targets.ToArray() });
                    }
                }
            }
            else
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (!rig.IsLocal() && rig.IsTagged() && !VRRig.LocalRig.IsTagged())
                    {
                        if (Vector3.Distance(VRRig.LocalRig.transform.position, rig.transform.position) < 3f)
                        {
                            VRRig.LocalRig.enabled = false;
                            VRRig.LocalRig.transform.position = new Vector3(999f, 999f, 999f);
                        }
                        else
                            VRRig.LocalRig.enabled = true;
                    }
                }
            }
        }
        public static void TagPlayer(NetPlayer player)
        {

            if (player == null || VRRig.LocalRig == null)
                return;
            if (!VRRig.LocalRig.IsTagged())
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=cyan>ARRAKIS</color><color=grey>]</color> You are not tagged.");
                VRRig.LocalRig.enabled = true;
                return;
            }
            if (Settings.instanttag)
            {
                InstantTagPlayer(player);
            }
            else
            {
                VRRig rig = VRRigCache.ActiveRigs.FirstOrDefault(x => x != null && x.Creator == player);
                if (rig == null || rig == VRRig.LocalRig)
                    return;
                if (rig.IsTagged())
                    return;
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = rig.transform.position;
                GameMode.ReportTag(player);
                VRRig.LocalRig.enabled = true;
            }
        }
        public static void Blink()
        {
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                Patches.Patchers.EventPatches.Override = () => false;
                Patches.Patchers.PlrSerializePatch.stopSerialization = true;
                if (!GetIndex("No Tag Limit").enabled)
                {
                    GorillaTagger.Instance.maxTagDistance = float.MaxValue;
                }
            }
            else
                StopBlinking();
        }
        public static void StopBlinking()
        {
            Patches.Patchers.EventPatches.Override = null;
            Patches.Patchers.PlrSerializePatch.stopSerialization = false;
            if (!GetIndex("No Tag Limit").enabled)
            {
                GorillaTagger.Instance.maxTagDistance = 2.2f;
            }
        }
    }
}