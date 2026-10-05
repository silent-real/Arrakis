/*
 * Arrakis | Arrakis/Networking/MenuNetwork.cs
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
using System.Collections.Generic;
using System.Linq;
using Arrakis.Classes;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Arrakis.Networking
{
    public static class MenuNetwork
    {
        public const string Enabled = "ARR_MenuEnabled";
        public const string Theme = "ARR_MenuTheme";

        public const string BackgroundColor = "ARR_BGColor";
        public const string ButtonColor = "ARR_ButtonColor";
        public const string EnabledButtonColor = "ARR_EnabledButtonColor";
        public const string TextColor = "ARR_TextColor";
        public const string EnabledTextColor = "ARR_EnabledTextColor";

        public const string Position = "ARR_MenuPos";
        public const string Rotation = "ARR_MenuRot";
        public const string Scale = "ARR_MenuScale";

        public const string Category = "ARR_MenuCategory";
        public const string Page = "ARR_MenuPage";

        public const string RightHanded = "ARR_RightHanded";
        public const string FloatMenu = "ARR_FloatMenu";

        public const string MenuTrail = "ARR_MenuTrail";
        public const string PointerTrail = "ARR_PointerTrail";
        public const string Outline = "ARR_Outline";

        public const string EnabledMods = "ARR_EnabledMods";

        private const float ModUpdateInterval = 0.20f;
        private const float TransformUpdateInterval = 0.10f;

        private static float nextModUpdate;
        private static float nextTransformUpdate;

        private static bool initialized;
        private static bool lastRoomState;

        public static void Initialize()
        {
            initialized = false;
            lastRoomState = false;
            nextModUpdate = 0f;
            nextTransformUpdate = 0f;
        }

        public static void Update()
        {
            try
            {
                if (Settings.disableNetworkedMenu)
                    return;

                if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
                {
                    initialized = false;
                    lastRoomState = false;
                    return;
                }
                if (!lastRoomState)
                {
                    lastRoomState = true;
                    initialized = false;
                }
                if (!initialized)
                {
                    initialized = true;
                    PublishInitialState();
                    nextModUpdate = Time.time + ModUpdateInterval;
                    nextTransformUpdate = Time.time + TransformUpdateInterval;
                }
                if (Time.time >= nextModUpdate)
                {
                    nextModUpdate = Time.time + ModUpdateInterval;
                    SyncMods();
                    SyncSettings();
                }
                if (Time.time >= nextTransformUpdate)
                {
                    nextTransformUpdate = Time.time + TransformUpdateInterval;
                    if (Menu.Main.menu != null)
                    {
                        SendTransform(Menu.Main.menu.transform.position, Menu.Main.menu.transform.rotation, Menu.Main.menu.transform.localScale);
                    }
                }
            }
            catch { }
        }

        public static float[] ColorToArray(Color c)
        {
            return new float[] { c.r, c.g, c.b, c.a };
        }

        public static Color ArrayToColor(object value)
        {
            if (value is float[] a && a.Length >= 4)
                return new Color(a[0], a[1], a[2], a[3]);

            return Color.white;
        }

        public static float[] Vector3ToArray(Vector3 v)
        {
            return new float[] { v.x, v.y, v.z };
        }

        public static Vector3 ArrayToVector3(object value)
        {
            if (value is float[] a && a.Length >= 3)
                return new Vector3(a[0], a[1], a[2]);

            return Vector3.zero;
        }

        public static float[] QuaternionToArray(Quaternion q)
        {
            return new float[] { q.x, q.y, q.z, q.w };
        }

        public static Quaternion ArrayToQuaternion(object value)
        {
            if (value is float[] a && a.Length >= 4)
                return new Quaternion(a[0], a[1], a[2], a[3]);
            return Quaternion.identity;
        }

        public static void Set(string key, object value)
        {
            if (Settings.disableNetworkedMenu)
                return;

            if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
                return;
            try
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { key, value } });
            }
            catch { }
        }

        public static void SetMultiple(Dictionary<string, object> values)
        {
            if (Settings.disableNetworkedMenu)
                return;

            if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
                return;
            if (values == null || values.Count == 0)
                return;
            try
            {
                Hashtable props = new Hashtable();
                foreach (var pair in values)
                {
                    if (pair.Value != null)
                        props[pair.Key] = pair.Value;
                }
                if (props.Count > 0)
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
            catch { }
        }
        public static void SetMenuEnabled(bool enabled) => Set(Enabled, enabled);
        public static void SetMenuOpen(bool open) => SetMenuEnabled(open);
        public static void SetBackgroundColor(Color color) => Set(BackgroundColor, ColorToArray(color));
        public static void SetButtonColor(Color color) => Set(ButtonColor, ColorToArray(color));
        public static void SetEnabledButtonColor(Color color) => Set(EnabledButtonColor, ColorToArray(color));
        public static void SetTextColor(Color color) => Set(TextColor, ColorToArray(color));
        public static void SetEnabledTextColor(Color color) => Set(EnabledTextColor, ColorToArray(color));
        public static void SetPosition(Vector3 position) => Set(Position, Vector3ToArray(position));
        public static void SetRotation(Quaternion rotation) => Set(Rotation, QuaternionToArray(rotation));
        public static void SetScale(Vector3 scale) => Set(Scale, Vector3ToArray(scale));
        public static void SetRightHanded(bool value) => Set(RightHanded, value);
        public static void SetFloatMenu(bool value) => Set(FloatMenu, value);
        public static void SetOutline(bool value) => Set(Outline, value);
        public static void SetMenuTrail(bool value) => Set(MenuTrail, value);
        public static void SetPointerTrail(bool value) => Set(PointerTrail, value);

        public static void SetCategory(string category)
        {
            if (!string.IsNullOrEmpty(category))
                Set(Category, category);
        }
        public static void SetPage(int page) => Set(Page, page);
        public static void SetTheme(string themeName)
        {
            if (!string.IsNullOrEmpty(themeName))
                Set(Theme, themeName);
        }

        public static void SetTransform(Transform transform)
        {
            if (transform == null)
                return;
            SetMultiple(new Dictionary<string, object>
            {
                { Position, Vector3ToArray(transform.position) },
                { Rotation, QuaternionToArray(transform.rotation) },
                { Scale, Vector3ToArray(transform.localScale) }
            });
        }

        public static void SendTransform(Vector3 position, Quaternion rotation)
        {
            SendTransform(position, rotation, Menu.Main.menu != null ? Menu.Main.menu.transform.localScale : Vector3.one);
        }

        public static void SendTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (!PhotonNetwork.InRoom)
                return;

            SetMultiple(new Dictionary<string, object>
            {
                { Position, Vector3ToArray(position) },
                { Rotation, QuaternionToArray(rotation) },
                { Scale, Vector3ToArray(scale) }
            });
        }

        public static void SyncMods()
        {
            try
            {
                string[] mods = Menu.Buttons.buttons.SelectMany(x => x)
                    .Where(x => x != null && x.enabled && !x.label && !string.IsNullOrEmpty(x.buttonText)).Select(x => x.buttonText)
                    .Distinct().ToArray();
                Set(EnabledMods, mods);
            }
            catch { }
        }

        public static void SetMod(string modName, bool enabled)
        {
            if (string.IsNullOrEmpty(modName))
                return;
            List<string> mods = GetLocalMods();
            if (enabled)
            {
                if (!mods.Contains(modName))
                    mods.Add(modName);
            }
            else
            {
                mods.Remove(modName);
            }
            Set(EnabledMods, mods.ToArray());
        }

        private static List<string> GetLocalMods()
        {
            try
            {
                return Menu.Buttons.buttons
                    .SelectMany(x => x).Where(x => x != null && x.enabled && !x.label && !string.IsNullOrEmpty(x.buttonText))
                    .Select(x => x.buttonText).Distinct().ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static void SyncSettings()
        {
            try
            {
                SetMultiple(new Dictionary<string, object>
                {
                    { RightHanded, Settings.rightHanded },
                    { FloatMenu, Settings.FloatMenu },
                    { Outline, Settings.outlineMenu },
                    { MenuTrail, Settings.menutrail },
                    { PointerTrail, Settings.pointertrail },
                    { Category, Menu.Main.CurrentCategoryName },
                    { Page, Menu.Main.pageNumber }
                });
            }
            catch { }
        }

        public static void PublishInitialState()
        {
            if (!PhotonNetwork.InRoom)
                return;
            try
            {
                Color bg = Arrakis.Settings.backgroundColor.GetCurrentColor();
                SetMultiple(new Dictionary<string, object>
                {
                    { Enabled, Menu.Main.menu != null },
                    { BackgroundColor, ColorToArray(bg) },
                    { RightHanded, Settings.rightHanded },
                    { FloatMenu, Settings.FloatMenu },
                    { Outline, Settings.outlineMenu },
                    { MenuTrail, Settings.menutrail },
                    { PointerTrail, Settings.pointertrail },
                    { Category, Menu.Main.CurrentCategoryName },
                    { Page, Menu.Main.pageNumber }
                });
                if (Arrakis.Menu.Main.menu != null)
                    SetTransform(Arrakis.Menu.Main.menu.transform);
                SyncMods();
            }
            catch { }
        }

        public static bool GetBool(Player player, string key, bool defaultValue = false)
        {
            if (player == null)
                return defaultValue;
            if (!player.CustomProperties.TryGetValue(key, out object value))
                return defaultValue;
            if (value is bool b)
                return b;
            return defaultValue;
        }
        public static int GetInt(Player player, string key, int defaultValue = 0)
        {
            if (player == null)
                return defaultValue;
            if (!player.CustomProperties.TryGetValue(key, out object value))
                return defaultValue;
            if (value is int i) return i;
            if (value is byte b) return b;
            return defaultValue;
        }

        public static string GetString(Player player, string key, string defaultValue = "")
        {
            if (player == null)
                return defaultValue;
            if (!player.CustomProperties.TryGetValue(key, out object value))
                return defaultValue;
            return value as string ?? defaultValue;
        }

        public static Color GetColor(Player player, string key, Color defaultValue)
        {
            if (player == null)
                return defaultValue;
            if (!player.CustomProperties.TryGetValue(key, out object value))
                return defaultValue;
            if (value is float[] a && a.Length >= 4)
                return new Color(a[0], a[1], a[2], a[3]);
            return defaultValue;
        }

        public static Vector3 GetVector3(Player player, string key, Vector3 defaultValue)
        {
            if (player == null)
                return defaultValue;
            if (!player.CustomProperties.TryGetValue(key, out object value))
                return defaultValue;
            if (value is float[] a && a.Length >= 3)
                return new Vector3(a[0], a[1], a[2]);
            return defaultValue;
        }

        public static Quaternion GetQuaternion(Player player, string key, Quaternion defaultValue)
        {
            if (player == null)
                return defaultValue;
            if (!player.CustomProperties.TryGetValue(key, out object value))
                return defaultValue;
            if (value is float[] a && a.Length >= 4)
                return new Quaternion(a[0], a[1], a[2], a[3]);
            return defaultValue;
        }

        public static string[] GetMods(Player player)
        {
            if (player == null)
                return Array.Empty<string>();
            if (!player.CustomProperties.TryGetValue(EnabledMods, out object value))
                return Array.Empty<string>();
            if (value is string[] mods)
                return mods;
            return Array.Empty<string>();
        }

        public static void ApplyPlayerNameColor(Player player)
        {
            try
            {
                var rig = RigManager.GetVRRigFromPlayer(player);
                if (rig == null)
                    return;
                rig.playerText1.color = GetColor(player, BackgroundColor, Color.white);
            }
            catch { }
        }

        public static void OnPropertiesUpdated(Player player, Hashtable changedProperties)
        {
            if (player == null || changedProperties == null)
                return;
            if (PhotonNetwork.LocalPlayer != null && player == PhotonNetwork.LocalPlayer)
                return;
            if (changedProperties.ContainsKey(BackgroundColor))
                ApplyPlayerNameColor(player);
            if (changedProperties.ContainsKey(Enabled))
            {
                bool enabled = GetBool(player, Enabled);
            }
            if (changedProperties.ContainsKey(Position) || changedProperties.ContainsKey(Rotation) || changedProperties.ContainsKey(Scale))
            {
                Vector3 position = GetVector3(player, Position, Vector3.zero);
                Quaternion rotation = GetQuaternion(player, Rotation, Quaternion.identity);
                Vector3 scale = GetVector3(player, Scale, Vector3.one);
            }
            if (changedProperties.ContainsKey(EnabledMods))
            {
                string[] mods = GetMods(player);
            }
        }
    }
}