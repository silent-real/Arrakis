/*
 * Arrakis | Menu/CosmeticsFinder.cs
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

using GorillaNetworking;
using Photon.Pun;
using System.Text;
using UnityEngine;

namespace Arrakis.Menu
{
    public class UtilsGUI : MonoBehaviour
    {
        private const int WindowId = 0x41524B;
        private const int MaxNameLength = 12;

        private Rect windowRect = new Rect(20f, 20f, 280f, 0f);

        private string newName = "";
        private float red, green, blue;
        private string cosmeticId = "";
        private string lookupResult = "";

        private void Start()
        {
            red = PlayerPrefs.GetFloat("redValue", 0f);
            green = PlayerPrefs.GetFloat("greenValue", 0f);
            blue = PlayerPrefs.GetFloat("blueValue", 0f);
            newName = PlayerPrefs.GetString("playerName", "");
        }

        private void OnGUI()
        {
            if (!Settings.utilsgui) return;
            windowRect = GUILayout.Window(WindowId, windowRect, DrawWindow, "Arrakis Utils");
        }

        private void DrawWindow(int id)
        {
            DrawNameSection();
            GUILayout.Space(8f);
            DrawColorSection();
            GUILayout.Space(8f);
            DrawCosmeticSection();
            GUI.DragWindow();
        }
        private void DrawNameSection()
        {
            GUILayout.Label("Name");
            newName = GUILayout.TextField(newName, MaxNameLength);
            if (GUILayout.Button("Change Name"))
                ApplyName(newName);
        }
        private void ApplyName(string raw)
        {
            string name = SanitizeName(raw);
            if (name.Length == 0)
            {
                return;
            }
            newName = name;
            GorillaTagger.Instance.offlineVRRig.SetNameTagText(name);
            GorillaComputer.instance.savedName = name;
            PhotonNetwork.LocalPlayer.NickName = name;
            PlayerPrefs.SetString("playerName", name);
            PlayerPrefs.Save();
        }

        private static string SanitizeName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) 
                return "";
            var sb = new StringBuilder(MaxNameLength);
            foreach (char c in raw.ToUpperInvariant())
            {
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                    sb.Append(c);
                if (sb.Length == MaxNameLength) break;
            }
            return sb.ToString();
        }

        private void DrawColorSection()
        {
            GUILayout.Label("Color");
            red = ColorSlider("R", red);
            green = ColorSlider("G", green);
            blue = ColorSlider("B", blue);
            Rect swatch = GUILayoutUtility.GetRect(0f, 18f, GUILayout.ExpandWidth(true));
            Color prev = GUI.color;
            GUI.color = new Color(red, green, blue, 1f);
            GUI.DrawTexture(swatch, Texture2D.whiteTexture);
            GUI.color = prev;
            if (GUILayout.Button("Change Color"))
                ApplyColor();
        }

        private static float ColorSlider(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(16f));
            value = GUILayout.HorizontalSlider(value, 0f, 1f);
            GUILayout.Label(Mathf.RoundToInt(value * 9f).ToString(), GUILayout.Width(16f));
            GUILayout.EndHorizontal();
            return value;
        }

        private void ApplyColor()
        {
            GorillaTagger.Instance.UpdateColor(red, green, blue);
            PlayerPrefs.SetFloat("redValue", red);
            PlayerPrefs.SetFloat("greenValue", green);
            PlayerPrefs.SetFloat("blueValue", blue);
            PlayerPrefs.Save();
        }

        private void DrawCosmeticSection()
        {
            GUILayout.Label("Cosmetic ID");
            cosmeticId = GUILayout.TextField(cosmeticId);

            if (GUILayout.Button("Get Cosmetic Data"))
                LookupCosmetic(cosmeticId.Trim());

            if (lookupResult.Length > 0)
                GUILayout.Label(lookupResult);
        }

        private void LookupCosmetic(string id)
        {
            if (id.Length == 0)
            {
                lookupResult = "Enter an ID first.";
                return;
            }
            var controller = CosmeticsController.instance;
            if (controller == null)
            {
                lookupResult = "CosmeticsController not ready.";
                return;
            }
            string itemName = controller.GetItemNameFromDisplayName(id);
            var so = controller.GetCosmeticSOFromDisplayName(id);
            lookupResult = $"Name: {(string.IsNullOrEmpty(itemName) ? "<none>" : itemName)}\n" + $"SO: {(so != null ? so.name : "<none>")}";
            CustomConsole.Log($"[{id}] name={itemName}", CustomConsole.LogType.Info);
            CustomConsole.Log($"[{id}] SO={so}", CustomConsole.LogType.Info);
        }
    }
}