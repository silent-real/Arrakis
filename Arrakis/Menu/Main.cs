/*
 * Arrakis | Menu/Main.cs
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
using Arrakis.Managers;
using Arrakis.Managers.CustomMaps;
using Arrakis.Mods;
using Arrakis.Notifications;
using BepInEx;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using static Arrakis.Extensions.VRRigExtensions;
using static Arrakis.Menu.Buttons;
using static Arrakis.Settings;

namespace Arrakis.Menu
{
    [HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
    public class Main : MonoBehaviour
    {
        public static void OnLoad()
        {
            Patches.Patchers.EventPatches.OnSerialize += OnSerialize;
            Patches.Patchers.PlrSerializePatch.OnPlayerSerialize += OnPlayerSerialize;

            //MenuNetwork.Initialize();
            SceneMapLoader.Init();
            AudioManager.Init();

            try
            {
                PluginManager.LoadPlugins();
            }
            catch { }

            try
            {
                if (TPC == null)
                {
                    try
                    {
                        TPC = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>();
                    }
                    catch { TPC = GameObject.Find("Shoulder Camera").GetComponent<Camera>(); }
                }
            }
            catch { }
        }

        public static void Prefix()
        {
            if (!loadonce)
            {
                OnLoad();
                loadonce = true;
            }

            if (ServerData.lockdown)
            {
                NotificationManager.SendNotification("<color=red>[LOCKDOWN]</color> Arrakis has been locked down to prevent any bans etc.");
                if (menu != null)
                    GameObject.Destroy(menu);
            }

            UpdateKeyboard();

            try
            {
                bool toOpen = (!rightHanded && ControllerInputPoller.instance.leftControllerSecondaryButton) || (rightHanded && ControllerInputPoller.instance.rightControllerSecondaryButton);
                bool keyboardOpen = UnityInput.Current.GetKey(keyboardButton) || searching;

                if (menu == null)
                {
                    if (toOpen || keyboardOpen)
                    {
                        if (menusounds)
                            AudioManager.MenuSound("menuopen");
                        CreateMenu();
                        //MenuNetwork.SetMenuOpen(true);
                        if (menuanimation)
                        {
                            CRunner.instance.StartCoroutine(OpenMenu());
                        }
                        RecenterMenu(rightHanded, keyboardOpen);
                        if (reference == null)
                            CreateReference();
                    }
                }
                else
                {
                    if (toOpen || keyboardOpen)
                        RecenterMenu(rightHanded, keyboardOpen);
                    else
                    {
                        GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(true);

                        Rigidbody comp = menu.GetOrAddComponent<Rigidbody>();

                        //MenuNetwork.SetMenuOpen(false);

                        if (menusounds)
                            AudioManager.MenuSound("menuclose");

                        if (!disablemenudrop)
                        {
                            if (rightHanded)
                            {
                                comp.useGravity = !lowgravitymenu;
                                comp.linearVelocity = GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);
                                comp.angularVelocity = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/RightHand Controller").GetOrAddComponent<GorillaVelocityEstimator>().angularVelocity;
                            }
                            else
                            {
                                comp.useGravity = !lowgravitymenu;
                                comp.linearVelocity = GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0);
                                comp.angularVelocity = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/LeftHand Controller").GetOrAddComponent<GorillaVelocityEstimator>().angularVelocity;
                            }

                            if (menuanimation)
                            {
                                CRunner.instance.StartCoroutine(CloseMenu());
                            }
                            else
                            {
                                Destroy(menu, 2f);
                                menu = null;
                            }
                        }
                        else
                        {
                            Destroy(menu);
                            menu = null;
                        }

                        Destroy(reference);
                        reference = null;

                        Destroy(leftReference);
                        leftReference = null;
                        Destroy(rightReference);
                        rightReference = null;
                    }
                }
            }
            catch (Exception exc)
            {
                CustomConsole.Log(string.Format("{0} // Error initializing at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message), CustomConsole.LogType.Error);
            }

            try
            {
                if (GunPointer != null)
                {
                    if (!GunPointer.activeSelf)
                        Destroy(GunPointer);
                    else
                        GunPointer.SetActive(false);
                }

                if (GunLine != null)
                {
                    if (!GunLine.gameObject.activeSelf)
                    {
                        Destroy(GunLine.gameObject);
                        GunLine = null;
                    }
                    else
                        GunLine.gameObject.SetActive(false);
                }
            }
            catch { }

            try
            {
                if (!stumptextonce)
                {
                    CreateStumpText();
                    stumptextonce = true;
                }

                stumpText.gameObject.SetActive(stumptext);
            }
            catch { }

            try
            {
                if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(Experimental.prop))
                {
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { Experimental.prop, true } });
                }

                var currentColor = backgroundColor.GetCurrentColor();
                if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("NMenuColor", out object storedColor) || !Equals(storedColor, currentColor))
                {
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                    {
                        { "NMenuColor", new float[] { currentColor.r, currentColor.g, currentColor.b, currentColor.a } }
                    });
                }

                if (PhotonNetwork.InRoom)
                {
                    foreach (Photon.Realtime.Player plr in PhotonNetwork.PlayerList)
                    {
                        if (plr.CustomProperties.ContainsKey(Experimental.prop))
                        {
                            RigManager.GetVRRigFromPlayer(plr).playerText1.text = "[ARRAKIS] " + plr.NickName;
                            if (plr.CustomProperties.TryGetValue("NMenuColor", out object colorObj))
                            {
                                if (colorObj is float[] rgba && rgba.Length >= 4)
                                {
                                    RigManager.GetVRRigFromPlayer(plr).playerText1.color = new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CustomConsole.Log(e.ToString(), CustomConsole.LogType.Error);
            }

            if (menutrail)
            {
                try
                {
                    TrailRenderer trail = menu.GetOrAddComponent<TrailRenderer>();
                    trail.startColor = backgroundColor.GetColor(0);
                    trail.endColor = backgroundColor.GetColor(1);
                    trail.startWidth = 0.025f;
                    trail.endWidth = 0f;
                    trail.minVertexDistance = 0.05f;
                    trail.material.shader = Shader.Find("Sprites/Default");
                    trail.time = 2f;
                }
                catch { }
            }

            if (pointertrail)
            {
                try
                {
                    TrailRenderer trail = reference.GetOrAddComponent<TrailRenderer>();
                    trail.startColor = backgroundColor.GetColor(0);
                    trail.endColor = backgroundColor.GetColor(1);
                    trail.startWidth = 0.025f;
                    trail.endWidth = 0f;
                    trail.minVertexDistance = 0.05f;
                    trail.material.shader = Shader.Find("Sprites/Default");
                    trail.time = 2f;
                }
                catch { }
            }

            try
            {
                // FPS Counter
                if (fpsObject != null)
                    fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();

                // Run mods
                foreach (ButtonInfo button in buttons.SelectMany(list => list).Where(button => button.enabled && button.method != null))
                {
                    try
                    {
                        button.method.Invoke();
                    }
                    catch (Exception exc)
                    {
                        CustomConsole.Log(string.Format("{0} // Error with mod {1} at {2}: {3}", PluginInfo.Name, button.buttonText, exc.StackTrace, exc.Message), CustomConsole.LogType.Error);
                    }
                }
            }
            catch (Exception exc)
            {
                CustomConsole.Log(string.Format("{0} // Error with executing mods at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message), CustomConsole.LogType.Error);
            }

            /*try
            {
                MenuNetwork.Update();
            }
            catch { }*/

            if (NetworkSystem.Instance.InRoom)
            {
                if (currentRoomName != PhotonNetwork.CurrentRoom.Name)
                {
                    currentRoomName = PhotonNetwork.CurrentRoom.Name;
                    if (!disableroomnotifications)
                        NotificationManager.SendNotification($"<color=grey>[</color><color=cyan>ARRAKIS</color><color=grey>]</color> Joined room {currentRoomName}");
                }
                if (reconnectingRoomName != PhotonNetwork.CurrentRoom.Name)
                    reconnectingRoomName = PhotonNetwork.CurrentRoom.Name;
            }
            else
            {
                if (currentRoomName != "")
                {
                    if (!disableroomnotifications)
                        NotificationManager.SendNotification($"<color=grey>[</color><color=cyan>ARRAKIS</color><color=grey>]</color> Left room {currentRoomName}");
                    if (clearNotificationsOnDisconnect)
                        NotificationManager.ClearAllNotifications();
                    currentRoomName = "";
                }
            }

            shouldBePC = !XRSettings.isDeviceActive;

            PluginManager.ExecuteUpdate();

            try
            {
                if (!disableautosave && Time.time > lastsavesprefstime)
                {
                    lastsavesprefstime = Time.time + 120f;
                    SaveSettings();
                }
            }
            catch { }

            try
            {
                if (PhotonNetwork.InRoom && saveRoomDelay < Time.time)
                {
                    saveRoomDelay = Time.time + 5;
                    string[] roomdata = new string[4];
                    roomdata[0] = PhotonNetwork.CurrentRoom.IsOpen.ToString();
                    roomdata[1] = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
                    roomdata[2] = GorillaComputer.instance.GetSelectedMapJoinTrigger().networkZone;
                    roomdata[3] = GorillaComputer.instance.lastPressedGameMode;
                    File.WriteAllLines($"{PluginInfo.BaseDirectory}\\Rooms\\" + PhotonNetwork.CurrentRoom.name + ".txt", roomdata);
                }
            }
            catch { }

            try
            {
                if (!VRRig.LocalRig.enabled)
                {
                    if (ghostView)
                    {
                        if (ghostSphereR == null)
                        {
                            ghostSphereR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            GameObject.Destroy(ghostSphereR.GetComponent<SphereCollider>());
                            ghostSphereR.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                            ghostSphereR.GetComponent<Renderer>().material.color = backgroundColor.GetCurrentColor();
                            ghostSphereR.transform.parent = GorillaTagger.Instance.rightHandTransform;
                            ghostSphereR.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                            ghostSphereR.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                        }

                        if (ghostSphereL == null)
                        {
                            ghostSphereL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            GameObject.Destroy(ghostSphereL.GetComponent<SphereCollider>());
                            ghostSphereL.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                            ghostSphereL.GetComponent<Renderer>().material.color = backgroundColor.GetCurrentColor();
                            ghostSphereL.transform.parent = GorillaTagger.Instance.leftHandTransform;
                            ghostSphereL.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                            ghostSphereL.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                        }
                    }
                }
                else
                {
                    if (ghostView)
                    {
                        if (ghostSphereR != null)
                        {
                            GameObject.Destroy(ghostSphereR);
                            ghostSphereR = null;
                        }

                        if (ghostSphereL != null)
                        {
                            GameObject.Destroy(ghostSphereL);
                            ghostSphereL = null;
                        }
                    }
                }
            }
            catch { }

            try
            {
                if (lockTarget != null)
                {
                    if (gunTargetESP)
                    {
                        if (GetGunInput(true))
                            Visual.ChamRig(lockTarget, true, new Color(0.4f, 0f, 0f, 0.6f));
                        else
                            Visual.ChamRig(lockTarget, false, lockTarget.playerColor);
                    }
                    else
                    {
                        Visual.ChamRig(lockTarget, false, lockTarget.playerColor);
                    }
                }
            }
            catch { }

            try
            {
                if (stumpText != null)
                {
                    stumpText.transform.LookAt(Camera.main.transform);
                    stumpText.transform.Rotate(0, 180f, 0);
                }
            }
            catch { }

            ServerPos = ServerPos == Vector3.zero ? ServerSyncPos : Vector3.Lerp(ServerPos, VRRig.LocalRig.SanitizeVector3(ServerSyncPos), VRRig.LocalRig.lerpValueBody * 0.66f);
            ServerLeftHandPos = ServerLeftHandPos == Vector3.zero ? ServerSyncLeftHandPos : Vector3.Lerp(ServerLeftHandPos, VRRig.LocalRig.SanitizeVector3(ServerSyncLeftHandPos), VRRig.LocalRig.lerpValueBody);
            ServerRightHandPos = ServerRightHandPos == Vector3.zero ? ServerSyncRightHandPos : Vector3.Lerp(ServerRightHandPos, VRRig.LocalRig.SanitizeVector3(ServerSyncRightHandPos), VRRig.LocalRig.lerpValueBody);
        }

        private static IEnumerator OpenMenu()
        {
            GameObject menuObject = menu;
            float elapsedTime = 0f;
            Vector3 target = menu.transform.localScale;
            while (elapsedTime < 0.05f)
            {
                if (menuObject == null)
                    yield break;
                menuObject.transform.localScale = Vector3.Lerp(Vector3.zero, target, elapsedTime / 0.05f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            if (menuObject == null)
                yield break;
            menuObject.transform.localScale = target;
        }

        private static IEnumerator CloseMenu()
        {
            Transform menuTransform = menu.transform;
            menu = null;
            Vector3 before = menuTransform.localScale;
            float elapsedTime = 0f;
            while (elapsedTime < 0.05f)
            {
                menuTransform.localScale = Vector3.Lerp(before, Vector3.zero, elapsedTime / 0.05f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            Destroy(menuTransform.gameObject);
        }

        public static ButtonInfo[] StringsToInfos(string[] array) =>
            array.Select(GetIndex).ToArray();

        public static void ReloadMenu()
        {
            if (menu != null)
            {
                Destroy(menu);
                menu = null;
                CreateMenu();
            }
            if (reference == null) return;
            Destroy(reference);
            reference = null;
            CreateReference();
        }

        public static void SetUpAdminPanel(string adminname)
        {
            List<ButtonInfo> buttons = Buttons.buttons[GetCategory("Main")].ToList();
            buttons.Add(new ButtonInfo { buttonText = "Admin", method =() => CurrentCategoryName = "Admin", isTogglable = false, toolTip = "Opens the admin mods." });
            Buttons.buttons[GetCategory("Main")] = buttons.ToArray();
            string[] SpecialCases = new string[] { "Rawr X3", "The Lesbian", "Toys :3", "UwU", "Sekkusu", "Silly", "Bunny.LOL", "Sleep.ez", "Golden Menu", "Golden Menu Reborn", "﷽", };
            NotificationManager.SendNotification($"<color=grey>[</color><color=yellow>{(adminname == "NOVA" ? "OWNER" : (adminname == "SLEEPY" ? SpecialCases[UnityEngine.Random.Range(0, SpecialCases.Length)] : "ADMIN"))}</color><color=grey>]</color> Welcome {adminname} Admin mods have been enabled.", 2f);
        }

        public static void CreateMenu()
        {
            // Menu Holder
            menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(menu.GetComponent<Rigidbody>());
            Destroy(menu.GetComponent<BoxCollider>());
            Destroy(menu.GetComponent<Renderer>());
            menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);

            // Menu Background
            menuBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(menuBackground.GetComponent<Rigidbody>());
            Destroy(menuBackground.GetComponent<BoxCollider>());
            menuBackground.transform.parent = menu.transform;
            menuBackground.transform.rotation = Quaternion.identity;
            menuBackground.transform.localScale = menuSize;
            menuBackground.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
            menuBackground.transform.position = new Vector3(0.05f, 0f, 0f);

            ColorChanger colorChanger = menuBackground.AddComponent<ColorChanger>();
            colorChanger.colors = backgroundColor;

            if (outlineMenu)
            {
                GameObject outline = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(outline.GetComponent<Rigidbody>());
                Destroy(outline.GetComponent<BoxCollider>());
                outline.transform.parent = menu.transform;
                outline.transform.rotation = Quaternion.identity;
                outline.transform.localScale = new Vector3(0.09f, 1.04f, 1.04f);
                outline.GetComponent<Renderer>().material.color = buttonColors[0].GetCurrentColor();
                outline.transform.position = new Vector3(0.05f, 0f, 0f);
            }

            // Canvas
            canvasObject = new GameObject();
            canvasObject.transform.parent = menu.transform;
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = highqualitytext ? 2000f : 1000f;

            // Title
            title = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            title.font = currentFont;
            if (searching)
            {
                if (string.IsNullOrEmpty(keyboardInput))
                {
                    title.text = "Search";
                }
                else
                {
                    title.text = keyboardInput;
                }
            }
            else
            {
                if (!disablemenutitle)
                {
                    if (custommenutitle)
                    {
                        if (!File.Exists($"{PluginInfo.BaseDirectory}/CustomTitle.txt"))
                            File.WriteAllText($"{PluginInfo.BaseDirectory}/CustomTitle.txt", "your title");
                        else
                            title.text = File.ReadAllText($"{PluginInfo.BaseDirectory}/CustomTitle.txt") + (disablepagenumber ? "" : " <color=grey>[</color><color=white>" + (pageNumber + 1).ToString() + "</color><color=grey>]</color>");
                    }
                    else // sussy - ccmv2 moment | why did i spell movement 😭 - nova
                        title.text = ((Sussy ? "Sussy" : RareChance ? "Awwakis uwu 🥺✨" : PluginInfo.Name) + (disablepagenumber ? "" : " <color=grey>[</color><color=white>" + (pageNumber + 1).ToString() + "</color><color=grey>]</color>"));
                }
                else
                    title.text = "";
            }

            if (lowercasetext)
                title.text = title.text.ToLower();

            if (uppercasetext)
                title.text = title.text.ToUpper();

            title.fontSize = 1;
            title.color = textColors[0];
            title.supportRichText = true;
            title.fontStyle = currentStyle;
            title.alignment = TextAnchor.MiddleCenter;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 0;
            RectTransform component = title.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.28f, 0.05f);
            component.position = new Vector3(0.06f, 0f, 0.165f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            // FPS
            if (fpsCounter)
            {
                fpsObject = new GameObject
                {
                    transform =
                    {
                        parent = canvasObject.transform
                    }
                }.AddComponent<Text>();
                fpsObject.font = currentFont;
                fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();

                if (lowercasetext)
                    fpsObject.text = fpsObject.text.ToLower();

                if (uppercasetext)
                    fpsObject.text = fpsObject.text.ToUpper();

                fpsObject.color = textColors[0];
                fpsObject.fontSize = 1;
                fpsObject.supportRichText = true;
                fpsObject.fontStyle = currentStyle;
                fpsObject.alignment = TextAnchor.MiddleCenter;
                fpsObject.horizontalOverflow = HorizontalWrapMode.Overflow;
                fpsObject.resizeTextForBestFit = true;
                fpsObject.resizeTextMinSize = 0;
                RectTransform component2 = fpsObject.GetComponent<RectTransform>();
                component2.localPosition = Vector3.zero;
                component2.sizeDelta = new Vector2(0.28f, 0.02f);
                component2.position = new Vector3(0.06f, 0f, 0.135f);
                component2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }

            // Disconnect
            float thing = -0.3f;
            if (disconnectButton)
            {
                AddButton(-0.3f, -1, GetIndex("Disconnect"));
                thing -= 0.1f;
            }

            if (quickactions.Count > 0)
            {
                foreach (string action in quickactions.ToList())
                {
                    ButtonInfo button = GetIndex(action);
                    if (button == null)
                    {
                        quickactions.Remove(action);
                        continue;
                    }
                    AddButton(thing, -1, button);
                    thing -= 0.1f;
                }
            }

            if (!disableSearchButton)
            {
                SearchButton();
                if (!disableReturnButton && CurrentCategoryName != "Main")
                    ReturnButton(true);
            }
            else
            {
                if (!disableReturnButton && CurrentCategoryName != "Main")
                    ReturnButton(false);
            }

            // Page Buttons
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(keyboardButton))
                gameObject.layer = 2;
            Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0.65f, 0);
            gameObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
            gameObject.AddComponent<ButtonCollider>().relatedText = "PreviousPage";

            colorChanger = gameObject.AddComponent<ColorChanger>();
            colorChanger.colors = buttonColors[0];

            Text text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = "<";
            text.fontSize = 1;
            text.color = textColors[0];
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.064f, 0.195f, 0f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(keyboardButton))
            {
                gameObject.layer = 2;
            }
            Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
            gameObject.transform.localPosition = new Vector3(0.56f, -0.65f, 0);
            gameObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
            gameObject.AddComponent<ButtonCollider>().relatedText = "NextPage";

            colorChanger = gameObject.AddComponent<ColorChanger>();
            colorChanger.colors = buttonColors[0];

            text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = ">";
            text.fontSize = 1;
            text.color = textColors[0];
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.064f, -0.195f, 0f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            if (lowercasetext)
                text.text = text.text.ToLower();

            if (uppercasetext)
                text.text = text.text.ToUpper();

            // Button Creation
            int buttonIndexOffset = 0;
            ButtonInfo[] renderButtons = new ButtonInfo[] { };
            if (searching)
            {
                List<ButtonInfo> searchResults = new List<ButtonInfo>();
                string search = keyboardInput.Replace(" ", "").ToLower();
                foreach (ButtonInfo[] category in Buttons.buttons)
                {
                    foreach (ButtonInfo button in category)
                    {
                        if (button == null)
                            continue;
                        if (button.label)
                            continue;
                        string buttonName = (button.buttonText ?? "").Replace(" ", "").ToLower();
                        if (buttonName.Contains(search))
                        {
                            searchResults.Add(button);
                        }
                    }
                }
                renderButtons = searchResults.OrderBy(x => x.buttonText).ToArray();
            }
            else
            {
                if (CurrentPrompt != null)
                    ShowPrompt();
                else
                {
                    switch (CurrentCategoryName)
                    {
                        case "Enabled":
                            enabledMods = new List<ButtonInfo>() { GetIndex("Exit Enabled") };
                            enabledMods.AddRange(Buttons.buttons.SelectMany(buttonlist => buttonlist).Where(v => v.enabled));
                            if (enabledMods.Count == 1)
                                enabledMods.Add(new ButtonInfo { buttonText = "you have no enabled mods", label = true });
                            renderButtons = enabledMods.ToArray();
                            break;
                        case "Favorites":
                            foreach (var favoriteMod in favorites.Where(favoriteMod => GetIndex(favoriteMod) == null).ToList())
                                favorites.Remove(favoriteMod);
                            renderButtons = StringsToInfos(favorites.ToArray());
                            break;
                        default:
                            renderButtons = Buttons.buttons[currentCategoryIndex];
                            break;
                    }
                    renderButtons = renderButtons.Skip(pageNumber * (buttonsPerPage - buttonIndexOffset)).Take(buttonsPerPage - buttonIndexOffset).ToArray();
                }
            }
            for (int i = 0; i < renderButtons.Length; i++)
                AddButton((i + buttonIndexOffset) * 0.1f + (0.1f / 10), i, renderButtons[i]);
            RecenterMenu(rightHanded, false);
        }

        public static void AddButton(float offset, int buttonIndex, ButtonInfo method)
        {
            if (!method.label)
            {
                if (method.isIncremental)
                {
                    GameObject decrement = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(decrement.GetComponent<Rigidbody>());
                    decrement.GetComponent<BoxCollider>().isTrigger = true;
                    decrement.transform.parent = menu.transform;
                    decrement.transform.rotation = Quaternion.identity;
                    decrement.transform.localScale = new Vector3(0.09f, 0.102f, 0.08f);
                    decrement.transform.localPosition = new Vector3(0.56f, 0.399f, 0.28f - offset);
                    ButtonCollider decrementCollider = decrement.AddComponent<ButtonCollider>();
                    decrementCollider.relatedText = "Decrement_" + method.buttonText;
                    decrementCollider.incremental = true;
                    decrementCollider.positive = false;

                    ColorChanger decrementColor = decrement.AddComponent<ColorChanger>();
                    decrementColor.colors = buttonColors[0];

                    GameObject increment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(increment.GetComponent<Rigidbody>());
                    increment.GetComponent<BoxCollider>().isTrigger = true;
                    increment.transform.parent = menu.transform;
                    increment.transform.rotation = Quaternion.identity;
                    increment.transform.localScale = new Vector3(0.09f, 0.102f, 0.08f);
                    increment.transform.localPosition = new Vector3(0.56f, -0.399f, 0.28f - offset);
                    ButtonCollider incrementCollider = increment.AddComponent<ButtonCollider>();
                    incrementCollider.relatedText = "Increment_" + method.buttonText;
                    incrementCollider.incremental = true;
                    incrementCollider.positive = true;

                    ColorChanger incrementColor = increment.AddComponent<ColorChanger>();
                    incrementColor.colors = buttonColors[0];

                    Text valueText = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    valueText.font = currentFont;
                    valueText.text = method.overlapText ?? method.buttonText;
                    valueText.supportRichText = true;
                    valueText.fontSize = 1;
                    valueText.color = textColors[0];
                    valueText.alignment = TextAnchor.MiddleCenter;
                    valueText.fontStyle = currentStyle;
                    valueText.resizeTextForBestFit = true;
                    valueText.resizeTextMinSize = 0;
                    RectTransform valueTransform = valueText.GetComponent<RectTransform>();
                    valueTransform.localPosition = Vector3.zero;
                    valueTransform.sizeDelta = new Vector2(0.2f, 0.03f);
                    valueTransform.localPosition = new Vector3(0.064f, 0f, 0.111f - offset / 2.6f);
                    valueTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                    if (lowercasetext)
                        valueText.text = valueText.text.ToLower();

                    if (uppercasetext)
                        valueText.text = valueText.text.ToUpper();

                    Text decrementText = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    decrementText.font = currentFont;
                    decrementText.text = "<";
                    decrementText.fontSize = 1;
                    decrementText.color = textColors[0];
                    decrementText.alignment = TextAnchor.MiddleCenter;
                    decrementText.fontStyle = currentStyle;
                    decrementText.resizeTextForBestFit = true;
                    decrementText.resizeTextMinSize = 0;
                    RectTransform decrementTransform = decrementText.GetComponent<RectTransform>();
                    decrementTransform.localPosition = Vector3.zero;
                    decrementTransform.sizeDelta = new Vector2(0.1f, 0.03f);
                    decrementTransform.localPosition =
                    new Vector3(0.064f, 0.12f, 0.111f - offset / 2.6f);
                    decrementTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                    if (lowercasetext)
                        decrementText.text = decrementText.text.ToLower();

                    if (uppercasetext)
                        decrementText.text = decrementText.text.ToUpper();

                    Text incrementText = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    incrementText.font = currentFont;
                    incrementText.text = ">";
                    incrementText.fontSize = 1;
                    incrementText.color = textColors[0];
                    incrementText.alignment = TextAnchor.MiddleCenter;
                    incrementText.fontStyle = currentStyle;
                    incrementText.resizeTextForBestFit = true;
                    incrementText.resizeTextMinSize = 0;
                    RectTransform incrementTransform = incrementText.GetComponent<RectTransform>();
                    incrementTransform.localPosition = Vector3.zero;
                    incrementTransform.sizeDelta = new Vector2(0.1f, 0.03f);
                    incrementTransform.localPosition = new Vector3(0.064f, -0.12f, 0.111f - offset / 2.6f);
                    incrementTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                    if (lowercasetext)
                        incrementText.text = incrementText.text.ToLower();

                    if (uppercasetext)
                        incrementText.text = incrementText.text.ToUpper();

                    return;
                }

                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (!UnityInput.Current.GetKey(keyboardButton) && !searching)
                    gameObject.layer = 2;
                Destroy(gameObject.GetComponent<Rigidbody>());
                gameObject.GetComponent<BoxCollider>().isTrigger = true;
                gameObject.transform.parent = menu.transform;
                gameObject.transform.rotation = Quaternion.identity;
                gameObject.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
                gameObject.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - offset);
                gameObject.AddComponent<ButtonCollider>().relatedText = method.buttonText;
                ColorChanger colorChanger = gameObject.AddComponent<ColorChanger>();
                colorChanger.colors = method.enabled ? buttonColors[1] : buttonColors[0];
            }

            Text text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = method.overlapText ?? method.buttonText;
            text.supportRichText = true;
            text.fontSize = 1;
            text.color = method.enabled ? textColors[1] : textColors[0];
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = currentStyle;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.064f, 0f, 0.111f - offset / 2.6f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            if (lowercasetext)
                text.text = text.text.ToLower();

            if (uppercasetext)
                text.text = text.text.ToUpper();
        }

        public static void RecreateMenu()
        {
            if (menu != null)
            {
                Destroy(menu);
                menu = null;
                CreateMenu();
                RecenterMenu(rightHanded, UnityInput.Current.GetKey(keyboardButton));
            }
        }

        public static void RecenterMenu(bool isRightHanded, bool isKeyboardCondition)
        {
            if (!isKeyboardCondition)
            {
                if (FloatMenu)
                {
                    menu.transform.position = GorillaTagger.Instance.headCollider.transform.TransformPoint(new Vector3(0f, -0.1f, 0.5f));
                    menu.transform.LookAt(GorillaTagger.Instance.headCollider.transform);
                    Vector3 rottationings = menu.transform.rotation.eulerAngles;
                    rottationings += new Vector3(-90f, 0f, -90f);
                    menu.transform.rotation = Quaternion.Euler(rottationings);
                }
                else if (!isRightHanded)
                {
                    menu.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    menu.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                }
                else
                {
                    menu.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    Vector3 rotation = GorillaTagger.Instance.rightHandTransform.rotation.eulerAngles;
                    rotation += new Vector3(0f, 0f, 180f);
                    menu.transform.rotation = Quaternion.Euler(rotation);
                }
            }
            else
            {
                try
                {
                    if (TPC == null)
                    {
                        try
                        {
                            TPC = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>();
                        }
                        catch { TPC = GameObject.Find("Shoulder Camera").GetComponent<Camera>(); }
                    }
                }
                catch { }

                GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(false);

                if (TPC != null)
                {
                    TPC.transform.position = new Vector3(-999f, -999f, -999f);
                    TPC.transform.rotation = Quaternion.identity;
                    GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bg.transform.localScale = new Vector3(10f, 10f, 0.01f);
                    bg.transform.transform.position = TPC.transform.position + TPC.transform.forward;
                    Color realcolor = backgroundColor.GetCurrentColor();
                    bg.GetComponent<Renderer>().material.color = new Color32((byte)(realcolor.r * 50), (byte)(realcolor.g * 50), (byte)(realcolor.b * 50), 255);
                    Destroy(bg, 0.05f);
                    menu.transform.parent = TPC.transform;
                    menu.transform.position = TPC.transform.position + (TPC.transform.forward * 0.5f) + (TPC.transform.up * -0.02f);
                    menu.transform.rotation = TPC.transform.rotation * Quaternion.Euler(-90f, 90f, 0f);
                    if (reference != null)
                    {
                        if (Mouse.current.leftButton.isPressed)
                        {
                            Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                            bool hitButton = Physics.Raycast(ray, out RaycastHit hit, 100);
                            if (hitButton)
                            {
                                ButtonCollider collide = hit.transform.gameObject.GetComponent<ButtonCollider>();
                                collide?.OnTriggerEnter(buttonCollider);
                            }
                        }
                        else
                            reference.transform.position = new Vector3(999f, -999f, -999f);
                    }
                }
            }

            /*if (menu != null)
            {
                MenuNetwork.SendTransform(menu.transform.position, menu.transform.rotation);
            }*/
        }
        public static GameObject leftReference;
        public static GameObject rightReference;
        public static SphereCollider leftButtonCollider;
        public static SphereCollider rightButtonCollider;
        public static void CreateReference()
        {
            if (FloatMenu)
            {
                leftReference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leftReference.transform.parent = GorillaTagger.Instance.leftHandTransform;
                leftReference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                leftReference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                leftReference.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
                leftReference.AddComponent<ColorChanger>().colors = backgroundColor;
                rightReference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rightReference.transform.parent = GorillaTagger.Instance.rightHandTransform;
                rightReference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                rightReference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                rightReference.GetComponent<Renderer>().enabled = !disablepointer;
                leftReference.GetComponent<Renderer>().enabled = !disablepointer;
                rightReference.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
                rightReference.AddComponent<ColorChanger>().colors = backgroundColor;
                leftButtonCollider = leftReference.GetComponent<SphereCollider>();
                rightButtonCollider = rightReference.GetComponent<SphereCollider>();
            }
            else
            {
                reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                reference.transform.parent = rightHanded ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
                reference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                reference.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
                reference.GetComponent<Renderer>().enabled = !disablepointer;
                buttonCollider = reference.GetComponent<SphereCollider>();
                ColorChanger colorChanger = reference.AddComponent<ColorChanger>();
                colorChanger.colors = backgroundColor;
            }
        }

        public static void Toggle(string buttonText)
        {
            ButtonInfo button = GetIndex(buttonText);
            if (button != null)
            {
                if (button.detected)
                {
                    NotificationManager.SendNotification($"This mod is <color=red>detected</color> and has been disabled.", NotificationDelay);
                }
                if (button.patched)
                {
                    NotificationManager.SendNotification($"This mod is <color=yellow>detected</color> and has been disabled.", NotificationDelay);
                }
            }

            int lastPage = ((buttons[currentCategoryIndex].Length + buttonsPerPage - 1) / buttonsPerPage) - 1;
            if (CurrentCategoryName == "Enabled")
            {
                List<string> enabledMods = new List<string>() { "Exit Enabled" };
                foreach (ButtonInfo[] buttonlist in Buttons.buttons)
                {
                    foreach (ButtonInfo v in buttonlist)
                    {
                        if (v.enabled)
                            enabledMods.Add(v.buttonText);
                    }
                }
                lastPage = ((enabledMods.Count + buttonsPerPage - 1) / buttonsPerPage) - 1;
            }
            else if (CurrentCategoryName == "Favorites")
            {
                lastPage = ((favorites.Count + buttonsPerPage - 1) / buttonsPerPage) - 1;
            }

            if ((ControllerInputPoller.instance.leftGrab || Keyboard.current.gKey.isPressed) && !disableFavBinds)
            {
                if (buttonText != "Exit Favorites" && buttonText != "Favorites")
                {
                    if (favorites.Contains(buttonText))
                    {
                        favorites.Remove(buttonText);
                        VRRig.LocalRig.PlayHandTapLocal(35, rightHanded, 0.6f);
                        NotificationManager.SendNotification("<color=grey>[</color><color=cyan>FAVORITES</color><color=grey>]</color> Removed from favorites.", NotificationDelay);
                    }
                    else
                    {
                        favorites.Add(buttonText);
                        VRRig.LocalRig.PlayHandTapLocal(35, rightHanded, 0.6f);
                        NotificationManager.SendNotification("<color=grey>[</color><color=cyan>FAVORITES</color><color=grey>]</color> Added to favorites.", NotificationDelay);
                    }
                    ReloadMenu();
                    return;
                }
            }

            if ((ControllerInputPoller.instance.leftControllerTriggerButton || Keyboard.current.vKey.isPressed) && !disableQuickactionsBinds)
            {
                string actionButton = GetIndex(buttonText).buttonText;
                if (!quickactions.Contains(actionButton))
                {
                    quickactions.Add(actionButton);
                    VRRig.LocalRig.PlayHandTapLocal(50, rightHanded, 0.4f);
                    NotificationManager.SendNotification("<color=grey>[</color><color=cyan>QUICK ACTIONS</color><color=grey>]</color> Added quick action button.", NotificationDelay);
                }
                else
                {
                    quickactions.Remove(actionButton);
                    VRRig.LocalRig.PlayHandTapLocal(48, rightHanded, 0.4f);
                    NotificationManager.SendNotification("<color=grey>[</color><color=cyan>QUICK ACTIONS</color><color=grey>]</color> Removed quick action button.", NotificationDelay);
                }
                ReloadMenu();
                return;
            }

            if (buttonText == "PreviousPage")
            {
                pageNumber--;
                if (pageNumber < 0)
                    pageNumber = lastPage;
            }
            else
            {
                if (buttonText == "NextPage")
                {
                    pageNumber++;
                    if (pageNumber > lastPage)
                        pageNumber = 0;
                }
                else
                {
                    ButtonInfo target = GetIndex(buttonText);
                    if (target != null)
                    {
                        if (target.isTogglable)
                        {
                            target.enabled = !target.enabled;
                            if (target.enabled)
                            {
                                NotificationManager.SendNotification("<color=grey>[</color><color=cyan>ENABLE</color><color=grey>]</color> " + target.toolTip, NotificationDelay);
                                if (target.enableMethod != null)
                                    try { target.enableMethod.Invoke(); } catch { }
                            }
                            else
                            {
                                NotificationManager.SendNotification("<color=grey>[</color><color=red>DISABLE</color><color=grey>]</color> " + target.toolTip, NotificationDelay);
                                if (target.disableMethod != null)
                                    try { target.disableMethod.Invoke(); } catch { }
                            }
                        }
                        else
                        {
                            NotificationManager.SendNotification("<color=grey>[</color><color=cyan>ENABLE</color><color=grey>]</color> " + target.toolTip, NotificationDelay);
                            if (target.method != null)
                                try { target.method.Invoke(); } catch { }
                        }
                    }
                    else
                        CustomConsole.Log(buttonText + " does not exist", CustomConsole.LogType.Warning);
                }
            }
            ReloadMenu();
        }

        public static void ToggleIncremental(string buttonText, bool increment)
        {
            string realButtonName = buttonText;
            if (buttonText.StartsWith("Increment_"))
                realButtonName = buttonText.Substring("Increment_".Length);
            if (buttonText.StartsWith("Decrement_"))
                realButtonName = buttonText.Substring("Decrement_".Length);
            ButtonInfo target = GetIndex(realButtonName);
            if (target == null)
                return;
            if (target.label)
                return;
            if (increment)
            {
                if (target.enableMethod != null)
                {
                    try
                    {
                        NotificationManager.SendNotification("<color=grey>[</color><color=cyan>INCREMENT</color><color=grey>]</color> " + target.toolTip, NotificationDelay);
                        target.enableMethod.Invoke();
                    }
                    catch { }
                }
            }
            else
            {
                if (target.disableMethod != null)
                {
                    try
                    {
                        NotificationManager.SendNotification("<color=grey>[</color><color=red>DECREMENT</color><color=grey>]</color> " + target.toolTip, NotificationDelay);
                        target.disableMethod.Invoke();
                    }
                    catch { }
                }
            }
            ReloadMenu();
        }

        private static readonly Dictionary<string, (int Category, int Index)> cacheGetIndex = new Dictionary<string, (int Category, int Index)>(); // Looping through 800 elements is not a light task :/
        public static ButtonInfo GetIndex(string buttonText)
        {
            if (buttonText == null)
                return null;
            if (cacheGetIndex.ContainsKey(buttonText))
            {
                var CacheData = cacheGetIndex[buttonText];
                try
                {
                    if (buttons[CacheData.Category][CacheData.Index].buttonText == buttonText)
                        return buttons[CacheData.Category][CacheData.Index];
                }
                catch { cacheGetIndex.Remove(buttonText); }
            }
            int categoryIndex = 0;
            foreach (ButtonInfo[] buttons in buttons)
            {
                int buttonIndex = 0;
                foreach (ButtonInfo button in buttons)
                {
                    if (button.buttonText == buttonText)
                    {
                        try
                        {
                            cacheGetIndex.Add(buttonText, (categoryIndex, buttonIndex));
                        }
                        catch
                        {
                            if (cacheGetIndex.ContainsKey(buttonText))
                                cacheGetIndex.Remove(buttonText);
                        }
                        return button;
                    }
                    buttonIndex++;
                }
                categoryIndex++;
            }

            return null;
        }

        public static Quaternion RandomQuaternion(float range = 360f) =>
            Quaternion.Euler(UnityEngine.Random.Range(0f, range), UnityEngine.Random.Range(0f, range), UnityEngine.Random.Range(0f, range));

        public static Color RandomColor(byte range = 255, byte alpha = 255) =>
            new Color32((byte)UnityEngine.Random.Range(0, range), (byte)UnityEngine.Random.Range(0, range), (byte)UnityEngine.Random.Range(0, range), alpha);

        public static Vector3 RandomVector3(float range = 1f) =>
            new Vector3(UnityEngine.Random.Range(-range, range), UnityEngine.Random.Range(-range, range), UnityEngine.Random.Range(-range, range));
        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueLeftHand()
        {
            Quaternion rot = GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handRotOffset;
            return (GorillaTagger.Instance.leftHandTransform.position + GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
        {
            Quaternion rot = GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handRotOffset;
            return (GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        public static void WorldScale(GameObject obj, Vector3 targetWorldScale)
        {
            Vector3 parentScale = obj.transform.parent.lossyScale;
            obj.transform.localScale = new Vector3(targetWorldScale.x / parentScale.x, targetWorldScale.y / parentScale.y, targetWorldScale.z / parentScale.z);
        }

        public static void FixStickyColliders(GameObject platform)
        {
            Vector3[] localPositions = new Vector3[]
            {
                new Vector3(0, 1f, 0),
                new Vector3(0, -1f, 0),
                new Vector3(1f, 0, 0),
                new Vector3(-1f, 0, 0),
                new Vector3(0, 0, 1f),
                new Vector3(0, 0, -1f)
            };
            Quaternion[] localRotations = new Quaternion[]
            {
                Quaternion.Euler(90, 0, 0),
                Quaternion.Euler(-90, 0, 0),
                Quaternion.Euler(0, -90, 0),
                Quaternion.Euler(0, 90, 0),
                Quaternion.identity,
                Quaternion.Euler(0, 180, 0)
            };
            for (int i = 0; i < localPositions.Length; i++)
            {
                GameObject side = GameObject.CreatePrimitive(PrimitiveType.Cube);
                try
                {
                    if (platform.GetComponent<GorillaSurfaceOverride>() != null)
                        side.AddComponent<GorillaSurfaceOverride>().overrideIndex = platform.GetComponent<GorillaSurfaceOverride>().overrideIndex;
                }
                catch { }
                float size = 0.025f;
                side.transform.SetParent(platform.transform);
                side.transform.position = localPositions[i] * (size / 2);
                side.transform.rotation = localRotations[i];
                WorldScale(side, new Vector3(size, size, 0.01f));
                side.GetComponent<Renderer>().enabled = false;
            }
        }

        private static int? noInvisLayerMask;
        public static int NoInvisLayerMask()
        {
            noInvisLayerMask ??= ~(1 << LayerMask.NameToLayer("TransparentFX") |
                1 << LayerMask.NameToLayer("Ignore Raycast") | 1 << LayerMask.NameToLayer("Zone") | 1 << LayerMask.NameToLayer("Gorilla Trigger") |
                1 << LayerMask.NameToLayer("Gorilla Boundary") | 1 << LayerMask.NameToLayer("GorillaCosmetics") | 1 << LayerMask.NameToLayer("GorillaParticle"));
            return noInvisLayerMask ?? GTPlayer.Instance.locomotionEnabledLayers;
        }
        public static bool gunLocked;
        public static VRRig lockTarget;
        public static int gunLineStyle = 0;
        public static float gunPointerSize = 0.15f; // ill add a setting for this later -sleepy
        public static float gunLineWidth = 0.025f; // ill add a setting for this later -sleepy
        public static bool swapgunhand = false;
        public static bool gunTargetESP = false;
        public static bool gunPointerTrail = false;

        public static bool GetGunInput(bool isShooting)
        {
            return isShooting ? ControllerInputPoller.instance.rightControllerTriggerButton || Mouse.current.leftButton.isPressed : ControllerInputPoller.instance.rightGrab || Mouse.current.rightButton.isPressed;
        }

        public static Vector3 MidPosition;
        public static Vector3 MidVelocity;

        public static (RaycastHit Ray, GameObject NewPointer) RenderGun(int? overrideLayerMask = null)
        {
            Transform gunTransform = swapgunhand ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
            Vector3 startPos = gunTransform.position;
            Vector3 direction = gunTransform.forward;
            Vector3 up = gunTransform.up;
            Vector3 right = gunTransform.right;
            Physics.Raycast(startPos, -gunTransform.up + -gunTransform.forward, out var ray, 512f, NoInvisLayerMask());
            if (shouldBePC)
            {
                Ray screenRay = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                Physics.Raycast(screenRay, out ray, 512f, NoInvisLayerMask());
                direction = screenRay.direction;
            }
            Vector3 endPos = gunLocked && lockTarget != null ? lockTarget.transform.position : ray.point;
            if (endPos == Vector3.zero)
                endPos = startPos + direction * 512f;
            if (GunPointer == null)
                GunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GunPointer.SetActive(true);
            GunPointer.transform.localScale = Vector3.one * gunPointerSize;
            GunPointer.transform.position = endPos;
            GunPointer.GetComponent<Renderer>().enabled = gunpointer;

            if (gunPointerTrail)
            {
                try
                {
                    TrailRenderer trail = GunPointer.GetOrAddComponent<TrailRenderer>();
                    trail.startColor = backgroundColor.GetColor(0);
                    trail.endColor = backgroundColor.GetColor(1);
                    trail.startWidth = 0.075f;
                    trail.endWidth = 0f;
                    trail.minVertexDistance = 0.05f;
                    trail.material.shader = Shader.Find("Sprites/Default");
                    trail.time = 0.8f;
                }
                catch { }
            }

            Renderer pointerRenderer = GunPointer.GetComponent<Renderer>();
            if (pointerRenderer.material.shader.name != "GUI/Text Shader")
                pointerRenderer.material.shader = Shader.Find("GUI/Text Shader");
            pointerRenderer.material.color = gunLocked || GetGunInput(true) ? buttonColors[1].GetCurrentColor() : buttonColors[0].GetCurrentColor();
            Collider col = GunPointer.GetComponent<Collider>();
            if (col != null)
                Destroy(col);
            if (gunline)
            {
                if (GunLine == null)
                {
                    GameObject lineObj = new GameObject("GunLine");
                    GunLine = lineObj.AddComponent<LineRenderer>();
                }
                GunLine.gameObject.SetActive(true);
                if (GunLine.material.shader.name != "GUI/Text Shader")
                    GunLine.material.shader = Shader.Find("GUI/Text Shader");
                GunLine.startWidth = gunLineWidth;
                GunLine.endWidth = gunLineWidth;
                GunLine.useWorldSpace = true;
                int steps = 50;
                Vector3[] points = new Vector3[steps];
                switch (gunLineStyle)
                {
                    case 0: // Bezier/Default
                        Vector3 baseMid = Vector3.Lerp(startPos, endPos, 0.5f);
                        float angle = Time.time * 3f;
                        Vector3 wobble = up * (Mathf.Sin(angle) * 0.15f) + right * (Mathf.Cos(angle * 1.3f) * 0.15f);
                        Vector3 targetMid = baseMid + wobble;
                        if (MidPosition == Vector3.zero)
                            MidPosition = targetMid;
                        Vector3 force = (targetMid - MidPosition) * 40f;
                        MidVelocity += force * Time.deltaTime;
                        MidVelocity *= Mathf.Exp(-6f * Time.deltaTime);
                        MidPosition += MidVelocity * Time.deltaTime;
                        for (int i = 0; i < steps; i++)
                        {
                            float t = (float)i / (steps - 1);
                            points[i] = Mathf.Pow(1 - t, 2) * startPos + 2 * (1 - t) * t * MidPosition + Mathf.Pow(t, 2) * endPos;
                        }
                        break;
                    case 1: // Straight
                        for (int i = 0; i < steps; i++)
                        {
                            float t = (float)i / (steps - 1);
                            points[i] = Vector3.Lerp(startPos, endPos, t);
                        }
                        break;
                    case 2: // Rainbow
                        for (int i = 0; i < steps; i++)
                        {
                            float t = (float)i / (steps - 1);
                            points[i] = Vector3.Lerp(startPos, endPos, t);
                        }
                        GunLine.startColor = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.4f, 1f), 1f, 1f);
                        GunLine.endColor = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.4f + 0.5f, 1f), 1f, 1f);
                        break;
                    case 3: // Zigline ?
                        points[0] = startPos;
                        points[steps - 1] = endPos;
                        for (int i = 1; i < steps - 1; i++)
                        {
                            float t = (float)i / (steps - 1);
                            Vector3 basePoint = Vector3.Lerp(startPos, endPos, t);
                            Vector3 offset = up * (Mathf.PerlinNoise(Time.time * 8f + i * 0.3f, 0f) - 0.5f) * 0.35f
                                        + right * (Mathf.PerlinNoise(0f, Time.time * 8f + i * 0.3f) - 0.5f) * 0.35f;
                            points[i] = basePoint + offset;
                        }
                        break;
                    case 4: // Pulse
                        float pulse = 0.015f + Mathf.Sin(Time.time * 6f) * 0.01f;
                        GunLine.startWidth = pulse;
                        GunLine.endWidth = pulse;
                        for (int i = 0; i < steps; i++)
                        {
                            float t = (float)i / (steps - 1);
                            points[i] = Vector3.Lerp(startPos, endPos, t);
                        }
                        break;
                }
                if (gunLineStyle != 2)
                {
                    GunLine.startColor = backgroundColor.GetCurrentColor();
                    GunLine.endColor = backgroundColor.GetCurrentColor(0.5f);
                }
                GunLine.positionCount = steps;
                GunLine.SetPositions(points);
            }
            return (ray, GunPointer);
        }

        // Variables & Functions
        public static GameObject menu;
        public static GameObject menuBackground;
        public static GameObject reference;
        public static GameObject canvasObject;
        public static Text title;

        public static List<ButtonInfo> enabledMods = new List<ButtonInfo>() { };
        public static readonly List<string> favorites = new List<string> { "Exit Favorites" };
        public static readonly List<string> quickactions = new List<string>();

        public static SphereCollider buttonCollider;
        public static Camera TPC;
        public static Text fpsObject;

        private static GameObject GunPointer;
        private static LineRenderer GunLine;

        public static float NotificationDelay = 1f;

        public static string currentRoomName = "";
        public static string reconnectingRoomName = "";

        public static void Search()
        {
            searching = !searching;
            pageNumber = 0;
            keyboardInput = "";

            if (searching)
            {
                lastPressedKeys = detectedKeys.Where(k => UnityEngine.InputSystem.Keyboard.current[k].isPressed).ToList();
            }
            else
            {
                lastPressedKeys.Clear();
                keyPressedTimes.Clear();
            }

            ReloadMenu();
        }

        public static bool searching = false;
        public static string keyboardInput = "";
        public static List<UnityEngine.InputSystem.Key> lastPressedKeys = new List<UnityEngine.InputSystem.Key>();
        public static readonly Dictionary<UnityEngine.InputSystem.Key, (float, float)> keyPressedTimes = new Dictionary<UnityEngine.InputSystem.Key, (float, float)>();
        public static readonly UnityEngine.InputSystem.Key[] detectedKeys =
        {
            UnityEngine.InputSystem.Key.A,
            UnityEngine.InputSystem.Key.B,
            UnityEngine.InputSystem.Key.C,
            UnityEngine.InputSystem.Key.D,
            UnityEngine.InputSystem.Key.E,
            UnityEngine.InputSystem.Key.F,
            UnityEngine.InputSystem.Key.G,
            UnityEngine.InputSystem.Key.H,
            UnityEngine.InputSystem.Key.I,
            UnityEngine.InputSystem.Key.J,
            UnityEngine.InputSystem.Key.K,
            UnityEngine.InputSystem.Key.L,
            UnityEngine.InputSystem.Key.M,
            UnityEngine.InputSystem.Key.N,
            UnityEngine.InputSystem.Key.O,
            UnityEngine.InputSystem.Key.P,
            UnityEngine.InputSystem.Key.Q,
            UnityEngine.InputSystem.Key.R,
            UnityEngine.InputSystem.Key.S,
            UnityEngine.InputSystem.Key.T,
            UnityEngine.InputSystem.Key.U,
            UnityEngine.InputSystem.Key.V,
            UnityEngine.InputSystem.Key.W,
            UnityEngine.InputSystem.Key.X,
            UnityEngine.InputSystem.Key.Y,
            UnityEngine.InputSystem.Key.Z,

            UnityEngine.InputSystem.Key.Digit0,
            UnityEngine.InputSystem.Key.Digit1,
            UnityEngine.InputSystem.Key.Digit2,
            UnityEngine.InputSystem.Key.Digit3,
            UnityEngine.InputSystem.Key.Digit4,
            UnityEngine.InputSystem.Key.Digit5,
            UnityEngine.InputSystem.Key.Digit6,
            UnityEngine.InputSystem.Key.Digit7,
            UnityEngine.InputSystem.Key.Digit8,
            UnityEngine.InputSystem.Key.Digit9,

            UnityEngine.InputSystem.Key.Space,
            UnityEngine.InputSystem.Key.Backspace,
            UnityEngine.InputSystem.Key.Enter,
            UnityEngine.InputSystem.Key.Escape
        };
        public static string[] allowedKeys = new string[]
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l",
            "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x",
            "y", "z"
        };
        private static void UpdateKeyboard()
        {
            if (!searching)
            {
                lastPressedKeys.Clear();
                keyPressedTimes.Clear();
                return;
            }
            List<UnityEngine.InputSystem.Key> keysPressed = new List<UnityEngine.InputSystem.Key>();
            foreach (var key in detectedKeys)
            {
                if (!UnityEngine.InputSystem.Keyboard.current[key].isPressed)
                {
                    keyPressedTimes.Remove(key);
                    continue;
                }
                if (keyPressedTimes.TryGetValue(key, out var delay))
                {
                    float newDelay = Mathf.Max(delay.Item2 * 0.75f, 0.05f);
                    if (Time.time > delay.Item1)
                    {
                        keyPressedTimes[key] = (Time.time + newDelay, newDelay);
                    }
                    else
                    {
                        keysPressed.Add(key);
                        continue;
                    }
                }
                else
                {
                    keyPressedTimes[key] = (Time.time + 0.5f, 0.5f);
                }
                keysPressed.Add(key);
                if (lastPressedKeys.Contains(key))
                    continue;

                bool ctrl = UnityEngine.InputSystem.Keyboard.current.leftCtrlKey.isPressed || UnityEngine.InputSystem.Keyboard.current.rightCtrlKey.isPressed;
                if (ctrl)
                {
                    switch (key)
                    {
                        case UnityEngine.InputSystem.Key.A:
                            keyboardInput = "";
                            pageNumber = 0;
                            ReloadMenu();
                            break;
                        case UnityEngine.InputSystem.Key.C:
                            GUIUtility.systemCopyBuffer = keyboardInput;
                            break;
                        case UnityEngine.InputSystem.Key.V:
                            string clipboard = GUIUtility.systemCopyBuffer ?? "";
                            foreach (char c in clipboard)
                            {
                                string cs = c.ToString();
                                if (allowedKeys.Contains(cs.ToLower()))
                                    keyboardInput += cs;
                                else if (c >= '0' && c <= '9')
                                    keyboardInput += cs;
                                else if (c == ' ')
                                    keyboardInput += cs;
                            }
                            pageNumber = 0;
                            ReloadMenu();
                            break;
                    }
                    continue;
                }

                switch (key)
                {
                    case UnityEngine.InputSystem.Key.Backspace:
                        if (!string.IsNullOrEmpty(keyboardInput))
                            keyboardInput = keyboardInput.Substring(0, keyboardInput.Length - 1);
                        break;
                    case UnityEngine.InputSystem.Key.Escape:
                        searching = false;
                        keyboardInput = "";
                        ReloadMenu();
                        break;
                    case UnityEngine.InputSystem.Key.Enter:
                        HandleSearch();
                        break;
                    case UnityEngine.InputSystem.Key.Space:
                        keyboardInput += " ";
                        break;
                    default:
                        string keyName = key.ToString();
                        if (keyName.Length == 1 && allowedKeys.Contains(keyName.ToLower()))
                        {
                            bool shift = UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed || UnityEngine.InputSystem.Keyboard.current.rightShiftKey.isPressed;
                            keyboardInput += shift ? keyName.ToUpper() : keyName.ToLower();
                        }
                        else if (keyName.StartsWith("Digit"))
                        {
                            keyboardInput += keyName.Replace("Digit", "");
                        }
                        break;
                }
                pageNumber = 0;
                ReloadMenu();
            }
            lastPressedKeys = keysPressed;
        }

        private static void HandleSearch()
        {
            if (!searching)
                return;
            if (string.IsNullOrWhiteSpace(keyboardInput))
                return;
            string search = keyboardInput.Replace(" ", "").ToLower();
            List<ButtonInfo> results = new List<ButtonInfo>();
            foreach (ButtonInfo[] category in Buttons.buttons)
            {
                foreach (ButtonInfo button in category)
                {
                    if (button == null)
                        continue;
                    if (button.label)
                        continue;
                    string buttonName = (button.buttonText ?? "").Replace(" ", "").ToLower();
                    if (buttonName.Contains(search))
                    {
                        results.Add(button);
                    }
                }
            }
            if (results.Count == 0)
                return;
            results = results.OrderBy(x => x.buttonText).ToList();
            Toggle(results[0].buttonText);
        }

        public static float lastsavesprefstime;
        public static float saveRoomDelay;

        public static GameObject ghostSphereR;
        public static GameObject ghostSphereL;

        public static int pageNumber = 0;
        public static int GetCategory(string categoryName) =>
           Buttons.categoryNames.ToList().IndexOf(categoryName);

        public static int pageButtonType = 1;

        public static int _currentCategoryIndex;
        public static int currentCategoryIndex
        {
            get => _currentCategoryIndex;
            set
            {
                _currentCategoryIndex = value;
                pageNumber = 0;
            }
        }

        public static bool loadonce;
        public static string CurrentCategoryName
        {
            get => Buttons.categoryNames[currentCategoryIndex];
            set =>
                currentCategoryIndex = GetCategory(value);
        }

        private static readonly Dictionary<PhotonView, GameObject> views = new Dictionary<PhotonView, GameObject>();
        private static PhotonView[] cachedViews;
        private static bool cached = true;
        public static PhotonView[] GetViews()
        {
            if (!cached && cachedViews != null)
                return cachedViews;
            views.Clear();
            PhotonView[] foundViews = GameObject.FindObjectsOfType<PhotonView>(true);
            foreach (PhotonView view in foundViews)
            {
                if (view == null)
                    continue;
                GameObject go = view.gameObject;
                if (go != null)
                    views[view] = go;
            }
            cachedViews = views.Keys.ToArray();
            cached = false;
            return cachedViews;
        }

        private static readonly List<GorillaGuardianZoneManager> allGZMans = new List<GorillaGuardianZoneManager>();
        public static GorillaGuardianZoneManager[] GuardianZMan()
        {
            allGZMans.RemoveAll(x => x == null);
            foreach (var zone in GameObject.FindObjectsByType<GorillaGuardianZoneManager>(FindObjectsSortMode.None))
            {
                if (!allGZMans.Contains(zone))
                    allGZMans.Add(zone);
            }
            return allGZMans.ToArray();
        }

        private static readonly List<TappableGuardianIdol> allTappable = new List<TappableGuardianIdol>();
        public static TappableGuardianIdol[] GetAllTappables()
        {
            allTappable.RemoveAll(x => x == null);
            foreach (var t in GameObject.FindObjectsByType<TappableGuardianIdol>(FindObjectsSortMode.None))
            {
                if (!allTappable.Contains(t))
                    allTappable.Add(t);
            }
            return allTappable.ToArray();
        }

        private static readonly List<GliderHoldable> allGliders = new List<GliderHoldable>();
        public static GliderHoldable[] GetGliders()
        {
            allGliders.RemoveAll(x => x == null);
            foreach (var g in GameObject.FindObjectsByType<GliderHoldable>(FindObjectsSortMode.None))
            {
                if (!allGliders.Contains(g))
                    allGliders.Add(g);
            }
            return allGliders.ToArray();
        }

        private static readonly List<TransferrableObject> allHoldables = new List<TransferrableObject>();
        public static TransferrableObject[] GetHoldables()
        {
            allHoldables.RemoveAll(x => x == null);
            foreach (var h in GameObject.FindObjectsByType<TransferrableObject>(FindObjectsSortMode.None))
            {
                if (!allHoldables.Contains(h))
                    allHoldables.Add(h);
            }
            return allHoldables.ToArray();
        }

        private static readonly System.Random random = new System.Random();

        public static bool RareChance = false;
        public static bool Sussy = false;
        public static bool OneIn(int odds) =>
            random.Next(odds) == 0;

        public static string GetCurrentMapName()
        {
            var mapChecks = new (GameObject obj, string name, string path)[]
            {
                (null, "Monke Blocks", "MonkeBlocksRoomScene"),
                (null, "Shared Block", "MonkeBlocksSharedRoom"),
                (null, "Ghost Reactor", "GhostReactorRoot"),
                (null, "Metropolis", "MetroMain"),
                (null, "Ranked", "RankedMain"),
                (null, "Hover Park", "HoverboardLevel"),
                (null, "Forest", "Environment Objects/LocalObjects_Prefab/Forest"),
                (null, "City", "City_Pretty"),
                (null, "Mountain", "Mountain"),
                (null, "Canyon", "Canyon"),
                (null, "Beach", "Beach"),
                (null, "Critters", "Critters"),
                (null, "Basement", "Basement"),
                (null, "Bayou", "BayouMain"),
                (null, "Cave", "Cave_Main_Prefab"),
                (null, "Clouds", "skyjungle")
            };
            for (int i = 0; i < mapChecks.Length; i++)
            {
                var mapCheck = mapChecks[i];
                GameObject foundObj = GameObject.Find(mapCheck.path);
                if (foundObj != null && foundObj.activeInHierarchy)
                {
                    return mapCheck.name;
                }
            }
            return "N/A";
        }

        public class PromptData
        {
            public string message;
            public string accepttext;
            public string declinetext;
            public Action accept;
            public Action decline;
        }
        public static List<PromptData> prompts = new List<PromptData>();
        public static PromptData CurrentPrompt
        {
            get
            {
                if (prompts.Count > 0)
                    return prompts[0];
                else
                    return null;
            }
        }
        public static void StopCurrentPrompt() =>
            prompts.RemoveAt(0);
        public static void Prompt(string message, Action accept = null, Action decline = null, string accepbutton = "Yes", string declinebutton = "No")
        {
            prompts.Add(new PromptData
            {
                message = message,
                accept = accept ?? (() => { }),
                decline = decline ?? (() => { }),
                accepttext = accepbutton,
                declinetext = declinebutton
            });
            if (menu != null && prompts.Count <= 1)
                ReloadMenu();
        }
        public static void PromptSingle(string Message, Action Accept = null, string AcceptButton = "Ok")
        {
            prompts.Add(new PromptData { message = Message, accept = Accept, decline = null, accepttext = AcceptButton, declinetext = null });
            if (menu != null && prompts.Count <= 1)
                ReloadMenu();
        }

        public static void ShowPrompt()
        {
            if (CurrentPrompt == null)
                return;
            Text prompttext = new GameObject { transform = { parent = canvasObject.transform } }.AddComponent<Text>();
            prompttext.font = currentFont;
            prompttext.text = CurrentPrompt.message;
            prompttext.fontSize = 1;
            prompttext.lineSpacing = 0.8f;
            prompttext.color = textColors[0];
            prompttext.supportRichText = true;
            prompttext.fontStyle = currentStyle;
            prompttext.alignment = TextAnchor.MiddleCenter;
            prompttext.resizeTextForBestFit = true;
            prompttext.resizeTextMinSize = 0;
            RectTransform rect = prompttext.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0.28f, 0.28f);
            rect.localPosition = new Vector3(0.06f, 0f, 0f);
            rect.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            {
                GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
                button.GetComponent<BoxCollider>().isTrigger = true;
                button.transform.parent = menu.transform;
                button.transform.rotation = Quaternion.identity;
                button.transform.localScale = new Vector3(0.09f, CurrentPrompt.declinetext == null ? 0.9f : 0.4375f, 0.08f);
                button.transform.localPosition = new Vector3(0.56f, CurrentPrompt.declinetext == null ? 0f : 0.2375f, -0.43f);
                button.AddComponent<ButtonCollider>().relatedText = "AcceptPrompt";
                ColorChanger colorChanger = button.AddComponent<ColorChanger>();
                colorChanger.colors = buttonColors[0];
                Text text = new GameObject { transform = { parent = canvasObject.transform } }.AddComponent<Text>();
                text.font = currentFont;
                text.fontStyle = currentStyle;
                text.text = CurrentPrompt.accepttext;
                text.fontSize = 1;
                text.alignment = TextAnchor.MiddleCenter;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;
                text.color = currentTheme == 9 ? textColors[0] : textColors[1];
                RectTransform textRect = text.GetComponent<RectTransform>();
                textRect.sizeDelta = new Vector2(0.2f, 0.03f);
                textRect.localPosition = new Vector3(0.064f, CurrentPrompt.declinetext != null ? 0.075f : 0f, -0.16f);
                textRect.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }
            if (CurrentPrompt.declinetext != null)
            {
                GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
                button.GetComponent<BoxCollider>().isTrigger = true;
                button.transform.parent = menu.transform;
                button.transform.rotation = Quaternion.identity;
                button.transform.localScale = new Vector3(0.09f, 0.4375f, 0.08f);
                button.transform.localPosition = new Vector3(0.56f, -0.2375f, -0.43f);
                button.AddComponent<ButtonCollider>().relatedText = "DeclinePrompt";
                ColorChanger colorChanger = button.AddComponent<ColorChanger>();
                colorChanger.colors = buttonColors[0];
                Text text = new GameObject { transform = { parent = canvasObject.transform } }.AddComponent<Text>();
                text.font = currentFont;
                text.fontStyle = currentStyle;
                text.text = CurrentPrompt.declinetext;
                text.fontSize = 1;
                text.alignment = TextAnchor.MiddleCenter;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;
                text.color = currentTheme == 9 ? textColors[0] : textColors[1];
                RectTransform rect2 = text.GetComponent<RectTransform>();
                rect2.sizeDelta = new Vector2(0.2f, 0.03f);
                rect2.localPosition = new Vector3(0.064f, -0.075f, -0.16f);
                rect2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }
        }

        public static Texture2D returnIcon;
        public static Material returnMat;

        private static void ReturnButton(bool showsearchbutton)
        {
            GameObject buttonObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buttonObject.GetComponent<BoxCollider>().isTrigger = true;
            buttonObject.transform.parent = menu.transform;
            buttonObject.transform.rotation = Quaternion.identity;
            buttonObject.transform.localScale = new Vector3(0.09f, 0.102f, 0.08f);
            buttonObject.transform.localPosition = new Vector3(0.56f, -0.450f, -0.58f);
            if (showsearchbutton)
                buttonObject.transform.localPosition += new Vector3(0f, 0.16f, 0f);
            buttonObject.AddComponent<ButtonCollider>().relatedText = "GlobalReturn";
            ColorChanger colorChanger = buttonObject.AddComponent<ColorChanger>();
            colorChanger.colors = colorChanger.colors = buttonColors[0];
            Image returnImage = new GameObject
            {
                transform = { parent = canvasObject.transform }
            }.AddComponent<Image>();
            if (returnIcon == null)
                returnIcon = LoadTexture("return");
            if (returnMat == null)
                returnMat = new Material(returnImage.material);
            returnImage.material = returnMat;
            returnImage.material.SetTexture("_MainTex", returnIcon);
            returnImage.color = currentTheme == 9 ? textColors[0] : textColors[1];
            RectTransform imageTransform = returnImage.GetComponent<RectTransform>();
            imageTransform.localPosition = Vector3.zero;
            imageTransform.sizeDelta = new Vector2(.03f, .03f);
            imageTransform.localPosition = new Vector3(.064f, -0.35f / 2.6f, -0.58f / 2.6f);
            if (showsearchbutton)
                imageTransform.localPosition += new Vector3(0f, 0.0475f, 0f);
            imageTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }


        public static Texture2D searchIcon;
        public static Material searchMat;

        private static void SearchButton()
        {
            GameObject buttonObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buttonObject.GetComponent<BoxCollider>().isTrigger = true;
            buttonObject.transform.parent = menu.transform;
            buttonObject.transform.rotation = Quaternion.identity;
            buttonObject.transform.localScale = new Vector3(0.09f, 0.102f, 0.08f);
            buttonObject.transform.localPosition = new Vector3(0.56f, -0.450f, -0.58f);
            buttonObject.AddComponent<ButtonCollider>().relatedText = "Search";
            ColorChanger colorChanger = buttonObject.AddComponent<ColorChanger>();
            colorChanger.colors = buttonColors[searching ? 1 : 0];
            Image searchImage = new GameObject
            {
                transform = { parent = canvasObject.transform }
            }.AddComponent<Image>();
            if (searchIcon == null)
                searchIcon = LoadTexture("search");
            if (searchMat == null)
                searchMat = new Material(searchImage.material);
            searchImage.material = searchMat;
            searchImage.material.SetTexture("_MainTex", searchIcon);
            searchImage.color = textColors[1];
            RectTransform imageTransform = searchImage.GetComponent<RectTransform>();
            imageTransform.localPosition = Vector3.zero;
            imageTransform.sizeDelta = new Vector2(.03f, .03f);
            imageTransform.localPosition = new Vector3(.064f, -0.35f / 2.6f, -0.58f / 2.6f);
            imageTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }


        public static Texture2D LoadTexture(string fileName)
        {
            using (Stream stream = typeof(Plugin).Assembly.GetManifestResourceStream($"Arrakis.Resources.Images.{fileName}.png"))
            {
                if (stream == null) return null;
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                Texture2D texture = new Texture2D(2, 2);
                ImageConversion.LoadImage(texture, bytes);
                return texture;
            }
        }

        public static void LoadRooms()
        {
            string currentRoom = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : null;
            CurrentCategoryName = "Rooms";
            pageNumber = 0;
            List<ButtonInfo> buttons = new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Exit Rooms", method = () => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." }
            };
            foreach (var roomFile in new DirectoryInfo($"{PluginInfo.BaseDirectory}\\Rooms").GetFiles())
            {
                string roomName = Path.GetFileNameWithoutExtension(roomFile.Name);
                string displayName = roomName;
                if (currentRoom == roomName)
                    displayName += " (CURRENT)";
                buttons.Add(new ButtonInfo { buttonText = displayName, method = () => OpenRoom(roomFile.FullName), isTogglable = false, toolTip = "Opens this room." });
            }
            Buttons.buttons[GetCategory("Rooms")] = buttons.ToArray();
        }

        public static void OpenRoom(string room)
        {
            string[] fileContents = File.ReadAllLines(room);
            string roomName = Path.GetFileNameWithoutExtension(room);
            List<ButtonInfo> buttons = new List<ButtonInfo>
            {
                new ButtonInfo { buttonText = "Back", method = () => LoadRooms(), isTogglable = false, toolTip = "Returns to the rooms list." },
                new ButtonInfo { buttonText = "Delete Room", method = () => { File.Delete(room); LoadRooms(); }, isTogglable = false, toolTip = "Deletes this room." },
                new ButtonInfo { buttonText = "Join Room", method = () => { PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomName, GorillaNetworking.JoinType.Solo); LoadRooms(); }, isTogglable = false, toolTip = "Joins this room." },
                new ButtonInfo { buttonText = "Room : " + roomName, label = true },
                new ButtonInfo { buttonText = "Playercount : " + fileContents[1], label = true },
                new ButtonInfo { buttonText = "Map : " + fileContents[2], label = true },
                new ButtonInfo { buttonText = "Gamemode : " + fileContents[3], label = true }
            };
            Buttons.buttons[GetCategory("Rooms")] = buttons.ToArray();
            CurrentCategoryName = "Rooms";
            pageNumber = 0;
        }
        public static int AddCategory(string categoryName)
        {
            List<ButtonInfo[]> buttonInfoList = buttons.ToList();
            buttonInfoList.Add(new ButtonInfo[] { });
            buttons = buttonInfoList.ToArray();
            List<string> categoryList = categoryNames.ToList();
            categoryList.Add(categoryName);
            categoryNames = categoryList.ToArray();
            return buttons.Length - 1;
        }
        public static void RemoveCategory(string categoryName)
        {
            List<ButtonInfo[]> buttonInfoList = buttons.ToList();
            buttonInfoList.RemoveAt(GetCategory(categoryName));
            buttons = buttonInfoList.ToArray();
            List<string> categoryList = categoryNames.ToList();
            categoryList.Remove(categoryName);
            categoryNames = categoryList.ToArray();
        }
        public static void AddButton(int category, ButtonInfo button, int index = -1)
        {
            List<ButtonInfo> buttonInfoList = buttons[category].ToList();
            if (index > 0)
                buttonInfoList.Insert(index, button);
            else
                buttonInfoList.Add(button);
            buttons[category] = buttonInfoList.ToArray();
        }
        public static void AddButtons(int category, ButtonInfo[] buttons, int index = -1)
        {
            List<ButtonInfo> buttonInfoList = Buttons.buttons[category].ToList();
            if (index > 0)
            {
                for (int i = 0; i < buttons.Length; i++)
                    buttonInfoList.Insert(index + i, buttons[i]);
            }
            else
                buttonInfoList.AddRange(buttons);
            Buttons.buttons[category] = buttonInfoList.ToArray();
        }
        public static void RemoveButton(int category, string name, int index = -1)
        {
            List<ButtonInfo> buttonInfoList = buttons[category].ToList();
            if (index > 0)
                buttonInfoList.RemoveAt(index);
            else
            {
                foreach (var button in buttonInfoList.Where(button => button.buttonText == name))
                {
                    buttonInfoList.Remove(button);
                    break;
                }
            }
            buttons[category] = buttonInfoList.ToArray();
        }
        public static void DestroyMenu()
        {
            Safety.Panic(true);
            BoardManager.DisableBoards();
            GameObject.Destroy(menu);
            menu = null;
            GameObject.Destroy(reference);
            reference = null;
            if (leftReference != null)
            {
                GameObject.Destroy(leftReference);
                leftReference = null;
            }
            if (rightReference != null)
            {
                GameObject.Destroy(rightReference);
                rightReference = null;
            }
            GameObject.Destroy(Plugin.holder);
            HarmonyLoader.RemovePatches();
        }

        private const string RandomChars = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789";
        public static string GenerateRandomString(int length = 8)
        {
            StringBuilder roomCode = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                roomCode.Append(RandomChars[UnityEngine.Random.Range(0, RandomChars.Length)]);
            }
            return roomCode.ToString();
        }
        public static readonly Dictionary<VRRig, int> playerPing = new Dictionary<VRRig, int>();
        private static void OnPlayerSerialize(VRRig rig) =>
            playerPing[rig] = rig.GetPing(); // mainly used for debuging, might make a ping nametags later -sleepy

        public static Vector3 ServerSyncPos;
        public static Vector3 ServerSyncLeftHandPos;
        public static Vector3 ServerSyncRightHandPos;
        public static Vector3 ServerPos;
        public static Vector3 ServerLeftHandPos;
        public static Vector3 ServerRightHandPos;
        private static void OnSerialize()
        {
            ServerSyncPos = VRRig.LocalRig?.transform.position ?? ServerSyncPos;
            ServerSyncLeftHandPos = VRRig.LocalRig?.leftHand?.rigTarget?.transform.position ?? ServerSyncLeftHandPos;
            ServerSyncRightHandPos = VRRig.LocalRig?.rightHand?.rigTarget?.transform.position ?? ServerSyncRightHandPos;
        }

        private static bool stumptextonce = false;
        private static TextMeshPro stumpText;
        public static void CreateStumpText()
        {
            stumpText = new GameObject().AddComponent<TextMeshPro>();
            stumpText.transform.position = new Vector3(-66.1859f, 12f, -82.471f);
            stumpText.transform.rotation = Quaternion.identity;
            stumpText.richText = true;
            stumpText.alignment = TextAlignmentOptions.Center;
            stumpText.fontSize = 1.2f;
            stumpText.text = $"<color=yellow>[ARRAKIS]</color>\nThank you for using arrakis\nVersion: <color=#00ffff>[{PluginInfo.Version}]</color> | Status: <color=#00ffff>[{ServerData.menustate.ToUpper()}]</color>";
        }
    }
}