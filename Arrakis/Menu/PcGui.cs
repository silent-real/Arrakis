/*
 * Arrakis | Menu/PcGui.cs
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
using Photon.Pun;
using UnityEngine;
using static Arrakis.Menu.Buttons;
using static Arrakis.Settings;

namespace Arrakis.Menu
{
    public class PcGui : MonoBehaviour
    {
        private static Rect windowRect = new Rect(120, 80, 850, 600);
        private static Vector2 categoryScroll;
        private static Vector2 buttonScroll;
        private static int selectedCategory;
        private static GUIStyle windowStyle;
        private static GUIStyle titleStyle;
        private static GUIStyle buttonStyle;
        private static GUIStyle labelStyle;
        private static GUIStyle fpsStyle;
        private static float fps;
        private static float fpsTimer;
        private static GUIStyle arrayStyle;
        private static float arrayWidth= 250f;
        public static bool OnGUIMenu = false;
        public static bool ShowOnScreenStuff = true;
        private static bool stylesCreated = false;

        private void OnGUI()
        {
            if (!ShowOnScreenStuff)
                return;
            DrawInfoBar();
            DrawArrayList();
            if (!OnGUIMenu)
                return;
            if (!stylesCreated)
                CreateStyles();
            windowRect = GUI.Window(999, windowRect, DrawWindow, "Arrakis", windowStyle);
        }
        private static void CreateStyles()
        {
            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.fontSize = 18;
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.fixedHeight = 35;
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;
            UpdateColors();
            stylesCreated = true;
        }
        private static void DrawWindow(int id)
        {
            if (categoryNames == null || categoryNames.Length == 0)
            {
                GUILayout.Label("No categories found.", labelStyle);
                GUI.DragWindow();
                return;
            }
            if (selectedCategory >= categoryNames.Length)
                selectedCategory = 0;
            GUILayout.BeginHorizontal();
            DrawCategories();
            DrawButtons();
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
        private static void DrawCategories()
        {
            GUILayout.BeginVertical(GUILayout.Width(200));
            categoryScroll = GUILayout.BeginScrollView(categoryScroll);
            for (int i = 0; i < categoryNames.Length; i++)
            {
                SetButtonColor(false);
                if (GUILayout.Button(categoryNames[i], buttonStyle))
                    selectedCategory = i;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        private static void DrawButtons()
        {
            GUILayout.BeginVertical();
            GUILayout.Label(categoryNames[selectedCategory], titleStyle);
            buttonScroll = GUILayout.BeginScrollView(buttonScroll);
            foreach (ButtonInfo button in buttons[selectedCategory])
            {
                if (button == null && button.buttonText.Contains("Admin") || button.buttonText.Contains("Temporary") || button.buttonText.Contains("Internal"))
                    continue;
                if (IsLabel(button))
                {
                    GUILayout.Space(5);
                    GUILayout.Label(button.buttonText, labelStyle);
                    continue;
                }
                SetButtonColor(button.enabled);
                if (GUILayout.Button(button.buttonText, buttonStyle))
                    Main.Toggle(button.buttonText);
                GUILayout.Space(3);
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        private static bool IsLabel(ButtonInfo button) =>
            !button.isTogglable && button.method == null && button.enableMethod == null && button.disableMethod == null;
        private static void SetButtonColor(bool enabled)
        {
            if (buttonColors == null || buttonColors.Length < 2)
                return;
            ExtGradient gradient = enabled ? buttonColors[1] : buttonColors[0];
            if (gradient != null && gradient.colors != null && gradient.colors.Length > 0)
                GUI.backgroundColor = gradient.colors[0].color;
        }
        private static void UpdateColors()
        {
            if (textColors == null || textColors.Length == 0)
                return;
            Color color = textColors[0];
            titleStyle.normal.textColor = color;
            buttonStyle.normal.textColor = color;
            buttonStyle.hover.textColor = color;
            buttonStyle.active.textColor = color;
            labelStyle.normal.textColor = color;
        }
        private static int GetPing() =>
            PhotonNetwork.GetPing();
        private static void DrawInfoBar()
        {
            if (fpsStyle == null)
            {
                fpsStyle = new GUIStyle(GUI.skin.label);
                fpsStyle.fontSize = 18;
                fpsStyle.alignment = TextAnchor.MiddleCenter;
            }
            fpsTimer += Time.deltaTime;
            if (fpsTimer >= 0.5f)
            {
                fps = 1f / Time.deltaTime;
                fpsTimer = 0f;
            }
            if (textColors != null && textColors.Length > 0)
                fpsStyle.normal.textColor = textColors[0];
            Rect infoRect = new Rect(Screen.width / 2 - 220, 10, 440, 45);
            if (Settings.backgroundColor != null && Settings.backgroundColor.colors != null && Settings.backgroundColor.colors.Length > 0)
                GUI.backgroundColor = Settings.backgroundColor.colors[0].color;
            GUI.Box(infoRect, GUIContent.none);
            GUI.backgroundColor = Color.white;
            int players = 0;
            int maxPlayers = 0;
            try
            {
                players = PhotonNetwork.CurrentRoom.PlayerCount;
                maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
            }
            catch
            {
                players = 0;
                maxPlayers = 0;
            }
            string text = $"Arrakis | FPS: {Mathf.RoundToInt(fps)} | Ping: {GetPing()} | {players}/{maxPlayers}";
            GUI.Label(infoRect, text, fpsStyle);
            Color lineColor = backgroundColor.GetCurrentColor();
            GUI.color = lineColor;
            GUI.DrawTexture(new Rect(infoRect.x, infoRect.yMax - 3, infoRect.width, 3), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        public static void DrawArrayList()
        {
            if (arrayStyle == null)
            {
                arrayStyle = new GUIStyle(GUI.skin.label);
                arrayStyle.fontSize = 18;
                arrayStyle.alignment = TextAnchor.MiddleRight;
            }
            if (textColors != null && textColors.Length > 0)
                arrayStyle.normal.textColor = textColors[0];
            float y = 50f;
            foreach (ButtonInfo[] category in buttons)
            {
                foreach (ButtonInfo button in category)
                {
                    if (button == null || !button.enabled || !button.ShowInArraylist)
                        continue;
                    string text = button.buttonText;
                    Vector2 size = arrayStyle.CalcSize(new GUIContent(text));
                    Rect rect = new Rect(Screen.width - arrayWidth - 15, y, arrayWidth, 25);
                    if (Settings.backgroundColor != null && Settings.backgroundColor.colors != null && Settings.backgroundColor.colors.Length > 0)
                        GUI.backgroundColor = Settings.backgroundColor.colors[0].color;
                    GUI.Box(rect, GUIContent.none);
                    GUI.Label(rect, text, arrayStyle);
                    y +=27;
                }
            }
        }
    }
}