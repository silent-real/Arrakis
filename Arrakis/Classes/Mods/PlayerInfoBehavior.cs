/*
 * Arrakis | Classes/Mods/PlayerInfoBehavior.cs
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

using Arrakis.Menu;
using Photon.Pun;
using UnityEngine;

namespace Arrakis.Classes.Mods
{
    public class PlayerInfoBehavior : MonoBehaviour  // pasted RIGHT from my plugin so code might be ass -sleepy
    {
        private TextMesh textMesh;
        private GUIStyle pcStyle;
        private int fps;
        private float timer;
        private string infoText = "";
        private string plainInfoText = "";
        private void Start()
        {
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textMesh.GetComponent<MeshRenderer>().material = textMesh.font.material;
            textMesh.fontSize = 500;
            textMesh.characterSize = 0.5f;
            textMesh.richText = true;
            textMesh.anchor = TextAnchor.UpperLeft;
            if (Camera.main != null)
            {
                transform.SetParent(Camera.main.transform, false);
            }
            transform.localPosition = new Vector3(-0.75f, 0.25f, 1.0f);
            transform.localRotation = Quaternion.Euler(0f, -15f, 0f);
            transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
        }
        public static bool Infected(VRRig p)
        {
            return p != null && p.mainSkin != null && p.mainSkin.material != null && (p.mainSkin.material.name.Contains("It") || p.mainSkin.material.name.Contains("fected"));
        }
        private void Update()
        {
            transform.localPosition = new Vector3(-0.75f, 0.25f, 1.0f);
            transform.localRotation = Quaternion.Euler(0f, -15f, 0f);
            bool isVR = UnityEngine.XR.XRSettings.isDeviceActive;
            textMesh.GetComponent<MeshRenderer>().enabled = isVR;
            float frameFPS = 1.0f / Time.unscaledDeltaTime;
            fps = (int)Mathf.Lerp(fps, frameFPS, Time.unscaledDeltaTime * 10f);
            timer += Time.deltaTime;
            if (timer > 0.1f)
            {
                timer = 0f;
                var p = PhotonNetwork.LocalPlayer;
                string name = p != null ? p.NickName : "Unknown";
                string roomPlayers = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount.ToString() : "0";
                bool inRoom = PhotonNetwork.InRoom;
                bool isMaster = PhotonNetwork.IsMasterClient;
                VRRig localRig = GorillaTagger.Instance.offlineVRRig;
                bool isInfected = localRig != null && Infected(localRig);
                string mapName = Main.GetCurrentMapName();
                Vector3 playerPos = transform.position;
                string position = $"({playerPos.x:F2}, {playerPos.y:F2}, {playerPos.z:F2})";
                string velocity = "N/A";
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 vel = rb.velocity;
                    velocity = $"({vel.x:F2}, {vel.y:F2}, {vel.z:F2}) | Speed: {vel.magnitude:F2}";
                }
                else
                {
                    Vector3 currentPos = transform.position;
                    if (lastPosition != Vector3.zero)
                    {
                        Vector3 vel = (currentPos - lastPosition) / Time.deltaTime;
                        velocity = $"({vel.x:F2}, {vel.y:F2}, {vel.z:F2}) | Speed: {vel.magnitude:F2}";
                    }
                    lastPosition = currentPos;
                }
                string[] lines = new string[]
                {
                    $"Name: {name}",
                    $"FPS: {fps}",
                    $"Ping: {PhotonNetwork.GetPing()} ms",
                    $"In Lobby: {inRoom}",
                    $"Room Players: {roomPlayers}",
                    $"Is Master Client: {isMaster}",
                    $"Is Tagged: {isInfected}",
                    $"Map: {mapName}",
                    $"Position: {position}",
                    $"Velocity: {velocity}"
                };
                infoText = "";
                plainInfoText = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    float normalizedPos = lines.Length > 1 ? (float)i / (lines.Length - 1) : 0f;
                    Color c = Color.white;
                    infoText += $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{lines[i]}</color>\n";
                    plainInfoText += lines[i] + "\n";
                }
                textMesh.text = infoText;
                textMesh.color = Color.white;
            }
        }
        private Vector3 lastPosition = Vector3.zero;
        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            if (UnityEngine.XR.XRSettings.isDeviceActive) return;
            if (pcStyle == null)
            {
                pcStyle = new GUIStyle(GUI.skin.label);
                pcStyle.fontSize = 20;
                pcStyle.fontStyle = FontStyle.Normal;
                pcStyle.alignment = TextAnchor.UpperLeft;
            }
            float yOffset = 8f;
            float xOffset = 8f;
            int lineCount = 10;
            float lineHeight = pcStyle.fontSize * 1.2f;
            float labelHeight = lineCount * lineHeight + 16f;
            float labelWidth = 500f;
            pcStyle.richText = false;
            pcStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(xOffset + 1f, yOffset + 1f, labelWidth, labelHeight), plainInfoText, pcStyle);
            pcStyle.richText = true;
            GUI.Label(new Rect(xOffset, yOffset, labelWidth, labelHeight), infoText, pcStyle);
        }
    }
}