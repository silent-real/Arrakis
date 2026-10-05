/*
 * Arrakis | Notifications/NotificationManager.cs
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Arrakis.Notifications
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager instance;
        public GameObject HUDObj;
        public GameObject HUDObj2;
        private GameObject MainCamera;
        private Material AlertText = new Material(Shader.Find("GUI/Text Shader"));
        public static Text NotifiText;
        private bool HasInit;
        public static float notificationDecayTime = 1f;
        private static readonly List<Notification> activeNotifications = new List<Notification>();
        private GUIStyle pcNotificationStyle;

        private void Start()
        {
            instance = this;
        }

        private void Init()
        {
            MainCamera = Camera.main.gameObject;
            HUDObj = new GameObject("HUD");
            HUDObj2 = new GameObject("HUD_PARENT");
            HUDObj.transform.SetParent(HUDObj2.transform);
            HUDObj.AddComponent<Canvas>();
            HUDObj.AddComponent<CanvasScaler>();
            HUDObj.AddComponent<GraphicRaycaster>();
            Canvas canvas = HUDObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = MainCamera.GetComponent<Camera>();
            RectTransform rect = HUDObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(5f, 5f);
            HUDObj2.transform.position = MainCamera.transform.position;
            rect.localPosition = new Vector3(0f, 0f, 1.6f);
            HUDObj.transform.localScale = Vector3.one;
            rect.rotation = Quaternion.Euler(0f, -270f, 0f);
            NotifiText = CreateText("Notifications", new Vector2(450f, 210f), TextAnchor.LowerLeft, new Vector3(-1f, -1f, -0.5f), 25);
        }

        private Text CreateText(string name, Vector2 size, TextAnchor anchor, Vector3 pos, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(HUDObj.transform);
            Text txt = obj.AddComponent<Text>();
            txt.text = "";
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = fontSize;
            txt.fontStyle = Settings.currentStyle;
            txt.alignment = anchor;
            txt.rectTransform.sizeDelta = size;
            txt.rectTransform.localScale = new Vector3(0.0033f, 0.0033f, 0.0033f);
            txt.rectTransform.localPosition = pos;
            txt.material = AlertText;
            txt.supportRichText = true;
            return txt;
        }

        private void FixedUpdate()
        {
            try
            {
                if (!HasInit && Camera.main != null)
                {
                    Init();
                    HasInit = true;
                }
                if (!HasInit)
                    return;
                HUDObj.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
                HUDObj2.transform.position = MainCamera.transform.position;
                HUDObj2.transform.rotation = MainCamera.transform.rotation;

                float time = Time.time;
                activeNotifications.RemoveAll(n => time >= n.Delay);
                if (NotifiText != null)
                {
                    NotifiText.text = string.Concat(activeNotifications.Select(n => n.Text));
                    NotifiText.alignment = Settings.flipnotifications ? TextAnchor.LowerRight : TextAnchor.LowerLeft;
                    NotifiText.rectTransform.localPosition = Settings.flipnotifications ? new Vector3(-1f, -1f, 0.5f) : new Vector3(-1f, -1f, -0.5f);
                    try
                    {
                        NotifiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                        NotifiText.fontStyle = Settings.currentStyle;
                    }
                    catch { }
                }
            }
            catch
            {
            }
        }

        private void OnGUI()
        {
            if (Settings.disableNotifications)
                return;
            if (activeNotifications.Count == 0)
                return;
            if (pcNotificationStyle == null)
            {
                pcNotificationStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    alignment = TextAnchor.MiddleRight,
                    richText = true,
                    wordWrap = true
                };
            }
            pcNotificationStyle.fontStyle = Settings.currentStyle;
            string text = string.Concat(activeNotifications.Select(n => n.Text));
            float width = 500f;
            float height = 200f;
            float rightMargin = 25f;
            float bottomMargin = 25f;
            float x = Screen.width - width - rightMargin;
            float y = Screen.height - height - bottomMargin;
            GUI.Label(new Rect(x, y, width, height), text,pcNotificationStyle);
        }

        public static void SendNotification(string text, float duration = -1f)
        {
            if (Settings.disableNotifications)
                return;
            if (duration < 0)
                duration = notificationDecayTime;
            if (!text.EndsWith("\n"))
                text += "\n";
            activeNotifications.Add(new Notification
            {
                Text = text,
                Delay = Time.time + duration
            });
        }

        public static void ClearAllNotifications()
        {
            activeNotifications.Clear();

            if (NotifiText != null)
                NotifiText.text = "";
        }
    }

    class Notification
    {
        public string Text;
        public float Delay;
    }
}