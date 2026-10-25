/*
 * Arrakis | Managers/KeybindManager.cs
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
using Arrakis.Notifications;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;
using static Arrakis.Managers.InputManager;
using static Arrakis.Menu.Main;
using static Arrakis.Settings;

namespace Arrakis.Managers
{
    public static class KeybindManager
    {
        public static ButtonInfo Listening;

        private static readonly InputType[] bindable =
        {
            InputType.Primary,
            InputType.Secondary,
            InputType.Trigger,
            InputType.Grip,
            InputType.Joystick
        };

        private static readonly bool[,] prev = new bool[2, 5];
        private static float armTime;

        public static void StartListening(ButtonInfo button)
        {
            Listening = button;
            armTime = Time.time + 0.15f;
            Snapshot();
            ReloadMenu();
        }

        public static void StopListening()
        {
            Listening = null;
            ReloadMenu();
        }

        public static void Update()
        {
            if (Listening == null)
                return;
            if (menu == null)
            {
                Listening = null;
                return;
            }
            if (Time.time < armTime)
            {
                Snapshot();
                return;
            }
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.escapeKey.wasPressedThisFrame)
                {
                    StopListening();
                    return;
                }
                if (kb.backspaceKey.wasPressedThisFrame)
                {
                    Listening.bindType = null;
                    Listening.bindHand = null;
                    Listening.bindKey = Key.None;
                    Bound();
                    return;
                }
                if (!XRSettings.isDeviceActive)
                {
                    foreach (KeyControl key in kb.allKeys)
                    {
                        if (key != null && key.wasPressedThisFrame)
                        {
                            Listening.bindKey = key.keyCode;
                            Bound();
                            return;
                        }
                    }
                    return;
                }
            }
            for (int h = 0; h < 2; h++)
            {
                InputManager.Hand hand = (InputManager.Hand)h;
                for (int i = 0; i < bindable.Length; i++)
                {
                    bool down = GetInput(bindable[i], hand);
                    bool rising = down && !prev[h, i];
                    prev[h, i] = down;
                    bool menuButton = bindable[i] == InputType.Secondary && (hand == InputManager.Hand.Right) == rightHanded;
                    if (rising && !menuButton)
                    {
                        Listening.bindType = bindable[i];
                        Listening.bindHand = hand;
                        Bound();
                        return;
                    }
                }
            }
        }

        private static void Snapshot()
        {
            if (!XRSettings.isDeviceActive)
                return;
            for (int h = 0; h < 2; h++)
                for (int i = 0; i < bindable.Length; i++)
                    prev[h, i] = GetInput(bindable[i], (InputManager.Hand)h);
        }

        private static void Bound()
        {
            ButtonInfo b = Listening;
            Listening = null;
            NotificationManager.SendNotification($"<color=grey>[</color><color=cyan>KEYBIND</color><color=grey>]</color> {b.buttonText} → {BindText(b)}", NotificationDelay);
            ReloadMenu();
        }

        private static readonly Regex tagRegex = new Regex(@"(\[<color=cyan>)[^<]*(</color>\])");

        public static string Label(InputType type, InputManager.Hand hand)
        {
            bool right = hand == InputManager.Hand.Right;
            switch (type)
            {
                case InputType.Primary: return right ? "A" : "X";
                case InputType.Secondary: return right ? "B" : "Y";
                case InputType.Trigger: return right ? "RT" : "LT";
                case InputType.Grip: return right ? "RG" : "LG";
                case InputType.Joystick: return right ? "RJ" : "LJ";
                default: return type.ToString();
            }
        }

        public static string OverrideLabel(ButtonInfo b)
        {
            if (!XRSettings.isDeviceActive && b.bindKey != Key.None)
                return b.bindKey.ToString();
            if (b.bindType != null && b.bindHand != null)
                return Label(b.bindType.Value, b.bindHand.Value);
            return null;
        }

        public static string ApplyBindLabel(ButtonInfo b, string text)
        {
            if (b == null || !b.isKeybind || string.IsNullOrEmpty(text))
                return text;
            string label = OverrideLabel(b);
            if (label == null)
                return text;
            return tagRegex.IsMatch(text) ? tagRegex.Replace(text, "${1}" + label + "${2}", 1) : $"{text} <color=grey>[<color=cyan>{label}</color>]</color>";
        }
        public static string BindText(ButtonInfo b)
        {
            if (Listening == b)
                return "...";
            string label = OverrideLabel(b);
            if (label != null)
                return label;
            if (b.defaultBindKnown)
                return Label(b.defaultBindType, b.defaultBindHand);
            return "-";
        }
    }
}