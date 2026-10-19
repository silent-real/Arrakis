/*
 * Arrakis | Managers/KeyboardManager.cs
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
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Arrakis.Managers // ported from JupiterX (https://user44504.na.imgto.link/public/20261013/6b5b9cddf7e36cbda7f2ad9b-image.avif) -sleepy
{
    public class KeyboardManagerButtonInfo
    {
        public string Text;
        public Action Action;
        public GameObject Object;
        public Renderer Renderer;
        public TextMeshPro Label;
        public bool IsCharacter;
        public Func<bool> IsActive;
    }
    public class KeyboardManager
    {
        private enum Mode { None, Search, Input }
        private static Mode mode = Mode.None;

        public static GameObject Root;

        public static bool KeyboardEnabled => mode == Mode.Search;

        public static bool InputActive => mode == Mode.Input;

        public static bool MenuPinned => mode == Mode.Search || mode == Mode.Input;

        public static bool KeepMenuOpen;

        public static Vector3 KeyboardOffset = new Vector3(0f, -0.38f, 0.48f);
        public static Vector3 MenuOffset = new Vector3(0f, -0.01f, 0.64f);
        public static float KeyboardTilt = 50f;
        public static float MenuTilt = 1f;
        public static float TurnThreshold = 40f;
        public static float ReturnDistance = 1.0f;
        public static float FollowSpeed = 8f;

        public static Vector3 RightSphereOffset = new Vector3(0f, -0.1f, 0f);
        public static Vector3 LeftSphereOffset = new Vector3(0f, -0.1f, 0f);

        private static readonly HashSet<string> ExcludedFromSearch = new HashSet<string>() { "Search", "Disconnect", "Global Return", "Admin" };

        private static readonly List<KeyboardManagerButtonInfo> keys = new List<KeyboardManagerButtonInfo>();
        private static readonly List<Action> pendingActions = new List<Action>();

        private static GameObject leftSphere;
        private static GameObject rightSphere;

        private static float lastSearchToggle = -1f;

        private static string query = "";
        private static int maxLength = 32;

        private static string inputPrompt = "";
        private static Action<string> inputSubmit;
        private static Action inputCancel;

        private static bool shift;
        private static bool capsLock;
        private static bool Upper => shift ^ capsLock;

        private static readonly Dictionary<string, string> ShiftMap = new  Dictionary<string, string>()
        {
            { "1", "!" }, { "2", "@" }, { "3", "#" }, { "4", "$" }, { "5", "%" },
            { "6", "^" }, { "7", "&" }, { "8", "*" }, { "9", "(" }, { "0", ")" }
        };

        private static bool anchorInit;
        private static float anchorYaw;
        private static float targetYaw;
        private static Vector3 anchorPos;
        private static Vector3 targetPos;

        private static bool caretOn = true;
        private static float caretTimer;

        private static readonly Vector3 Visible = Vector3.one * 0.55f;
        private const float KeyDepth = -0.02f;

        private static readonly List<(ButtonInfo info, string key)> searchIndex = new List<(ButtonInfo info, string key)>();
        private static readonly List<ButtonInfo> searchResults = new List<ButtonInfo>();
        private static string lastQuery;

        private static Transform Head => GorillaTagger.Instance.headCollider.transform;

        private static Quaternion YawRotation => Quaternion.Euler(0f, anchorYaw, 0f);

        public static void Search()
        {
            if (Time.time - lastSearchToggle < 0.3f) return;
            lastSearchToggle = Time.time;

            if (mode == Mode.Search)
                Close();
            else
                OpenSearch();
        }

        public static void Input(string prompt, Action<string> onSubmit, string startText = "", int maxChars = 32, Action onCancel = null)
        {
            if (mode != Mode.None)
                Close();

            mode = Mode.Input;
            inputPrompt = prompt ?? "";
            inputSubmit = onSubmit;
            inputCancel = onCancel;
            maxLength = Mathf.Max(1, maxChars);
            query = Sanitize(startText ?? "");
            if (query.Length > maxLength) query = query.Substring(0, maxLength);

            shift = false;
            capsLock = false;
            anchorInit = false;

            Load();

            if (Menu.Main.menu != null)
                Menu.Main.ReloadMenu();
            EnsureMenu();
            KeepMenuOpen = true;
        }

        public static void Close()
        {
            Mode was = mode;
            Action cancel = inputCancel;

            mode = Mode.None;
            KeepMenuOpen = false;
            query = "";
            inputSubmit = null;
            inputCancel = null;
            pendingActions.Clear();
            DestroyKeyboard();

            if (was == Mode.Search)
            {
                Menu.Main.pageNumber = 0;
                Menu.Main.keyboardInput = "";
                Menu.Main.ReloadMenu();
            }
            else if (was == Mode.Input)
            {
                Menu.Main.ReloadMenu();
                cancel?.Invoke();
            }
        }
        public static void QueueAction(Action action)
        {
            if (action != null)
                pendingActions.Add(action);
        }

        private static void OpenSearch()
        {
            if (mode != Mode.None)
                Close();

            mode = Mode.Search;
            query = "";
            maxLength = 32;
            shift = false;
            capsLock = false;
            anchorInit = false;
            Menu.Main.pageNumber = 0;
            Menu.Main.keyboardInput = "";
            BuildSearchIndex();
            Load();

            EnsureMenu();
            KeepMenuOpen = true;
        }

        public static void DestroyKeyboard()
        {
            if (Root != null) GameObject.Destroy(Root);
            if (leftSphere != null) GameObject.Destroy(leftSphere);
            if (rightSphere != null) GameObject.Destroy(rightSphere);
            Root = null;
            leftSphere = null;
            rightSphere = null;
            keys.Clear();
        }

        private static void Load()
        {
            DestroyKeyboard();
            UpdateAnchor(true);

            Root = new GameObject("JupiterXKeyboard");
            Root.transform.SetPositionAndRotation(KeyboardPosition(), KeyboardRotation());
            Root.transform.localScale = Visible;

            CreateBackground();
            CreateHandSpheres();
            BuildLayout();
            SpawnKeys();
            RefreshKeyLabels();
            RefreshDisplay();
        }
        private static void EnsureMenu()
        {
            if (!MenuPinned) return;

            if (Menu.Main.menu == null)
            {
                Menu.Main.CreateMenu();
                KeepMenuOpen = true;
            }

            if (Menu.Main.reference == null)
                Menu.Main.CreateReference();

            PositionMenu(Menu.Main.menu);
        }

        private static void UpdateAnchor(bool snap = false)
        {
            Transform head = Head;
            float headYaw = head.eulerAngles.y;

            if (snap || !anchorInit)
            {
                anchorYaw = targetYaw = headYaw;
                anchorPos = targetPos = head.position;
                anchorInit = true;
                return;
            }

            bool tooFar = Vector3.Distance(head.position, targetPos) > ReturnDistance;
            bool turnedAway = Mathf.Abs(Mathf.DeltaAngle(targetYaw, headYaw)) > TurnThreshold;
            if (tooFar || turnedAway)
            {
                targetPos = head.position;
                targetYaw = headYaw;
            }

            float t = 1f - Mathf.Exp(-FollowSpeed * Time.deltaTime);
            anchorYaw = Mathf.LerpAngle(anchorYaw, targetYaw, t);
            anchorPos = Vector3.Lerp(anchorPos, targetPos, t);
        }

        private static Vector3 KeyboardPosition() => anchorPos + YawRotation * KeyboardOffset;
        private static Quaternion KeyboardRotation() => YawRotation * Quaternion.Euler(KeyboardTilt, 0f, 0f);

        public static void PositionMenu(GameObject menu)
        {
            if (menu == null) return;
            if (!anchorInit) UpdateAnchor(true);

            Quaternion frame = YawRotation * Quaternion.Euler(MenuTilt, 0f, 0f);
            Vector3 up = frame * Vector3.up;
            Vector3 right = frame * Vector3.right;

            menu.transform.position = anchorPos + YawRotation * MenuOffset;
            menu.transform.rotation = Quaternion.LookRotation(up, -right);
        }

        public static string Normalize(string s) =>
            string.IsNullOrEmpty(s) ? "" : Menu.Main.NoRichtextTags(s).Replace(" ", "").ToLowerInvariant();

        public static void BuildSearchIndex()
        {
            searchIndex.Clear();
            searchResults.Clear();
            lastQuery = null;

            int categoryIndex = 0;
            foreach (ButtonInfo[] list in Menu.Buttons.buttons)
            {
                bool admin = Menu.Buttons.categoryNames[categoryIndex++].Contains("Admin");
                if (admin) continue;

                foreach (ButtonInfo b in list)
                {
                    if (b == null || b.label || ExcludedFromSearch.Contains(b.buttonText)) continue;
                    searchIndex.Add((b, Normalize(b.overlapText ?? b.buttonText)));
                }
            }
        }

        public static List<ButtonInfo> GetResults(string input)
        {
            if (searchIndex.Count == 0)
                BuildSearchIndex();

            string q = Normalize(input);
            if (q == lastQuery)
                return searchResults;

            lastQuery = q;
            searchResults.Clear();
            foreach (var (info, key) in searchIndex)
                if (q.Length == 0 || key.Contains(q))
                    searchResults.Add(info);

            return searchResults;
        }

        public static string InputDisplay()
        {
            string caret = caretOn ? "<color=#00FFFF>|</color>" : "<color=#00000000>|</color>";

            if (mode == Mode.Input)
                return query.Length == 0
                    ? caret + $"<color=#888888>{inputPrompt}</color>"
                    : $"<color=#888888>{inputPrompt}:</color> {query}{caret}";

            return query.Length == 0
                ? caret + "<color=#888888>Search</color>"
                : query + caret;
        }

        private static string Sanitize(string s) =>
            s.Replace("<", "").Replace(">", "").Replace("\n", " ").Replace("\r", "");

        private static void TypeKey(string key)
        {
            string c = Upper
                ? (ShiftMap.TryGetValue(key, out string symbol) ? symbol : key.ToUpperInvariant())
                : key.ToLowerInvariant();

            Append(c);

            if (shift)
            {
                shift = false;
                RefreshKeyLabels();
            }
        }

        private static void Append(string s)
        {
            if (string.IsNullOrEmpty(s)) return;

            s = Sanitize(s);
            int room = maxLength - query.Length;
            if (room <= 0) return;
            if (s.Length > room) s = s.Substring(0, room);

            query += s;
            OnQueryChanged();
        }

        private static void Backspace()
        {
            if (query.Length == 0) return;
            query = query.Substring(0, query.Length - 1);
            OnQueryChanged();
        }

        private static void Clear()
        {
            if (query.Length == 0) return;
            query = "";
            OnQueryChanged();
        }

        private static void ToggleShift()
        {
            shift = !shift;
            RefreshKeyLabels();
        }

        private static void ToggleCaps()
        {
            capsLock = !capsLock;
            RefreshKeyLabels();
        }

        private static void Copy()
        {
            GUIUtility.systemCopyBuffer = query;
            NotificationManager.SendNotification("<color=grey>[</color><color=cyan>ARRAKIS  </color><color=grey>]</color> Copied text.", 2f);
        }

        private static void Paste() => Append(GUIUtility.systemCopyBuffer);

        private static void Enter()
        {
            if (mode == Mode.Input)
            {
                Action<string> submit = inputSubmit;
                string text = query;

                inputCancel = null;
                Close();

                try { submit?.Invoke(text); }
                catch (Exception exc)
                {
                    CustomConsole.Log($"KeyboardManager.Input callback error: {exc.Message}", CustomConsole.LogType.Error);
                }
                return;
            }

            if (Normalize(query).Length == 0) return;

            List<ButtonInfo> results = GetResults(query);
            if (results.Count == 0)
            {
                NotificationManager.SendNotification($"<color=grey>[</color><color=red>SEARCH</color><color=grey>]</color> No results for \"{query}\".", 3f);
                return;
            }

            ButtonInfo top = results[0];
            if (top.isIncremental)
                Menu.Main.ToggleIncremental(top.buttonText, true);
            else
                Menu.Main.Toggle(top.buttonText);

            EnsureMenu();
        }

        private static void OnQueryChanged()
        {
            caretOn = true;
            caretTimer = 0f;

            if (mode == Mode.Search)
            {
                Menu.Main.keyboardInput = query;
                Menu.Main.pageNumber = 0;
                Menu.Main.ReloadMenu();
                EnsureMenu();
            }

            RefreshDisplay();
        }

        private static void RefreshDisplay()
        {
            if (MenuPinned && Menu.Main.searchInputText != null)
                Menu.Main.searchInputText.text = InputDisplay();
        }

        private static void RefreshKeyLabels()
        {
            foreach (var k in keys)
            {
                if (k.Label != null && k.IsCharacter)
                {
                    k.Label.text = Upper
                        ? (ShiftMap.TryGetValue(k.Text, out string symbol) ? symbol : k.Text.ToUpperInvariant())
                        : k.Text.ToLowerInvariant();
                }

                if (k.Renderer != null && k.IsActive != null)
                    k.Renderer.material.color = KeyColor(k);
            }
        }

        public static Color KeyColor(KeyboardManagerButtonInfo k) =>
            Settings.buttonColors[(k.IsActive != null && k.IsActive()) ? 1 : 0].GetCurrentColor();

        private static void BuildLayout()
        {
            keys.Clear();
            AddRow("1", "2", "3", "4", "5", "6", "7", "8", "9", "0");
            AddRow("Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P");
            AddRow("A", "S", "D", "F", "G", "H", "J", "K", "L");
            AddRow("Z", "X", "C", "V", "B", "N", "M");

            AddSpecial("SHIFT", ToggleShift, () => shift);
            AddSpecial("BACK", Backspace);
            AddSpecial("CAPS", ToggleCaps, () => capsLock);
            AddSpecial("COPY", Copy);
            AddSpecial("PASTE", Paste);
            AddSpecial("SPACE", () => Append(" "));
            AddSpecial("CLEAR", Clear);
            AddSpecial("ENTER", Enter);
        }

        private static void AddRow(params string[] row)
        {
            foreach (string key in row)
            {
                string captured = key;
                keys.Add(new KeyboardManagerButtonInfo
                {
                    Text = captured,
                    IsCharacter = true,
                    Action = () => TypeKey(captured)
                });
            }
        }

        private static void AddSpecial(string name, Action action, Func<bool> isActive = null) =>
            keys.Add(new KeyboardManagerButtonInfo { Text = name, Action = action, IsActive = isActive });

        private static void SpawnKeys()
        {
            const float spacingX = 0.09f;
            const float spacingY = 0.11f;
            const float startY = 0.18f;
            int[] rowCounts = { 10, 10, 9, 7 };

            int index = 0;
            for (int row = 0; row < rowCounts.Length; row++)
            {
                int count = rowCounts[row];
                float startX = -(count - 1) * spacingX * 0.5f;
                for (int col = 0; col < count; col++)
                    CreateKey(keys[index++], new Vector3(startX + col * spacingX, startY - row * spacingY, KeyDepth));
            }

            float zRowY = startY - 3 * spacingY;
            CreateKey(GetKey("SHIFT"), new Vector3(-0.42f, zRowY, KeyDepth), 0.13f);
            CreateKey(GetKey("BACK"), new Vector3(0.42f, zRowY, KeyDepth), 0.13f);

            SpawnStrip(startY - 4 * spacingY,
                ("CAPS", 0.12f),
                ("COPY", 0.11f),
                ("PASTE", 0.11f),
                ("SPACE", 0.30f),
                ("CLEAR", 0.12f),
                ("ENTER", 0.14f));
        }
        private static void SpawnStrip(float y, params (string name, float width)[] items)
        {
            const float gap = 0.012f;

            float total = -gap;
            foreach (var item in items)
                total += item.width + gap;

            float x = -total / 2f;
            foreach (var (name, width) in items)
            {
                CreateKey(GetKey(name), new Vector3(x + width / 2f, y, KeyDepth), width);
                x += width + gap;
            }
        }

        private static KeyboardManagerButtonInfo GetKey(string name)
        {
            foreach (var k in keys)
                if (k.Text == name)
                    return k;
            return null;
        }

        private static TextMeshPro CreateLabel(string name, Vector3 localPos, Vector2 size, float maxSize, TextAlignmentOptions align)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(Root.transform, false);
            obj.transform.localPosition = localPos;
            obj.transform.localRotation = Quaternion.identity;

            TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
            tmp.rectTransform.sizeDelta = size;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.05f;
            tmp.fontSizeMax = maxSize;
            tmp.enableWordWrapping = false;
            tmp.richText = true;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
        }

        private static void CreateKey(KeyboardManagerButtonInfo info, Vector3 pos, float width = 0.08f)
        {
            if (info == null) return;

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "Key_" + info.Text;
            obj.transform.SetParent(Root.transform, false);
            obj.transform.localPosition = pos;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(width, 0.08f, 0.04f);

            Renderer r = obj.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("GorillaTag/UberShader"));
            obj.GetComponent<BoxCollider>().isTrigger = true;

            info.Object = obj;
            info.Renderer = r;
            r.material.color = KeyColor(info);

            info.Label = CreateLabel("KeyText", pos + new Vector3(0f, 0f, -0.025f), new Vector2(width * 0.9f, 0.07f), 2f, TextAlignmentOptions.Center);
            info.Label.text = info.Text;

            obj.AddComponent<SphereKeyTrigger>().Info = info;
        }

        private static void CreateBackground()
        {
            GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "KeyboardBackground";
            bg.transform.SetParent(Root.transform, false);
            bg.transform.localRotation = Quaternion.identity;

            bg.transform.localPosition = new Vector3(0f, -0.04f, 0f);
            bg.transform.localScale = new Vector3(1.0f, 0.6f, 0.03f);

            Renderer r = bg.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("GorillaTag/UberShader"));
            r.material.color = Settings.backgroundColor.GetCurrentColor();
            UnityEngine.Object.Destroy(bg.GetComponent<Collider>());
        }
        private static void CreateHandSpheres()
        {
            rightSphere = CreateSphere(GorillaTagger.Instance.rightHandTransform, RightSphereOffset);
            leftSphere = CreateSphere(GorillaTagger.Instance.leftHandTransform, LeftSphereOffset);
        }

        private static GameObject CreateSphere(Transform parent, Vector3 localOffset)
        {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = "KeyboardSphere";
            s.transform.SetParent(parent, false);
            s.transform.localPosition = localOffset;
            s.transform.localScale = Vector3.one * 0.02f;

            Renderer r = s.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("GorillaTag/UberShader"));
            r.material.color = Color.gray;

            s.GetComponent<SphereCollider>().isTrigger = true;

            Rigidbody rb = s.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            return s;
        }

        public static void UpdateKeyboard()
        {
            if (Root == null) return;

            if (pendingActions.Count > 0)
            {
                Action[] toRun = pendingActions.ToArray();
                pendingActions.Clear();
                foreach (Action a in toRun)
                {
                    try { a(); }
                    catch (Exception exc)
                    {
                        CustomConsole.Log($"Keyboard key error: {exc.Message}", CustomConsole.LogType.Error);
                    }
                }
                if (Root == null) return;
            }
            Root.transform.SetPositionAndRotation(KeyboardPosition(), KeyboardRotation());
            EnsureMenu();

            caretTimer += Time.deltaTime;
            if (caretTimer >= 0.5f)
            {
                caretTimer = 0f;
                caretOn = !caretOn;
                RefreshDisplay();
            }
        }
    }

    public class SphereKeyTrigger : MonoBehaviour
    {
        public KeyboardManagerButtonInfo Info;
        private static float lastPress;
        private const float Delay = 0.2f;

        public void OnTriggerStay(Collider other)
        {
            if (!other.name.Contains("KeyboardSphere")) return;
            if (Time.time - lastPress < Delay) return;
            lastPress = Time.time;
            Press();
        }

        private void Press()
        {
            if (Info == null || Info.Renderer == null) return;

            Info.Renderer.material.color = Settings.buttonColors[1].GetCurrentColor();
            KeyboardManager.QueueAction(Info.Action);
            CRunner.instance.StartCoroutine(Reset(Info));
        }

        private System.Collections.IEnumerator Reset(KeyboardManagerButtonInfo info)
        {
            yield return new WaitForSeconds(0.15f);
            if (info.Renderer != null)
                info.Renderer.material.color = KeyboardManager.KeyColor(info);
        }
    }
}