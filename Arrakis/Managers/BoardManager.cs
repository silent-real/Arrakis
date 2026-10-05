/*
 * Arrakis | Managers/Boards.cs
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

using Arrakis.Classes.Menu;
using Arrakis.Extensions;
using Photon.Pun;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Arrakis.Managers
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance;
        private GameObject motdObj;
        private static TextMeshPro motdTMP;
        private GameObject cocText;
        private GameObject cocHeading;
        private TextMeshPro cocTextTMP;
        private TextMeshPro cocHeadingTMP;
        private TextMeshPro WelcomeToGorilllaTagHeadingTextTMP;
        private TextMeshPro HeadOutsideTMP;
        private GameObject monitorObj;
        private MeshRenderer boardRenders;
        private Material boardmat;
        private string ogHeadoutsideText;
        private string ogWelcomeToGorilllaTagHeadingText;
        private Material ogboardmat;
        public GameObject motdTextObj;
        public TextMeshPro motdTextTMP;
        private Material oldMonitorMat;
        private GameObject HeadOutside;
        private GameObject WelcomeToGorilllaTagHeadingText;

        public void Awake()
        {
            Instance = this;
            boardmat = new Material(Shader.Find("GorillaTag/UberShader"));
            boardmat.color = Settings.backgroundColor.GetCurrentColor();
        }

        public void Start()
        {
            HeadOutside = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/HeadOutside");
            WelcomeToGorilllaTagHeadingText = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/WelcomeToGorilllaTagHeadingText");
            motdObj = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText");
            cocText = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData");
            cocHeading = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText");

            motdTMP = motdObj?.GetComponent<TextMeshPro>();
            cocTextTMP = cocText?.GetComponent<TextMeshPro>();
            cocHeadingTMP = cocHeading?.GetComponent<TextMeshPro>();

            boardRenders = boards?.GetComponent<MeshRenderer>();

            if (boardRenders != null)
                ogboardmat = boardRenders.material;

            motdTextObj = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText");
            motdTextTMP = motdTextObj?.GetComponent<TextMeshPro>();
            HeadOutsideTMP = HeadOutside?.GetComponent<TextMeshPro>();
            WelcomeToGorilllaTagHeadingTextTMP = WelcomeToGorilllaTagHeadingText?.GetComponent<TextMeshPro>();

            if (HeadOutsideTMP != null)
                ogHeadoutsideText = HeadOutsideTMP.text;

            if (WelcomeToGorilllaTagHeadingTextTMP != null)
                ogWelcomeToGorilllaTagHeadingText = WelcomeToGorilllaTagHeadingTextTMP.text;

            monitorObj = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen");

            if (monitorObj != null)
                oldMonitorMat = monitorObj.GetComponent<MeshRenderer>()?.material;
        }

        public void Update()
        {
            if (HeadOutsideTMP != null && WelcomeToGorilllaTagHeadingTextTMP != null && motdTMP != null)
            {
                if (Settings.disablecustomboards)
                {
                    DisableBoards();
                }
                else
                {
                    if (cocText != null)
                        cocText.SetActive(false);

                    if (cocHeading != null)
                        cocHeading.SetActive(false);

                    motdTMP.text = "ARRAKIS";
                    WelcomeToGorilllaTagHeadingTextTMP.text = "ARRAKIS";
                    HeadOutsideTMP.text = $"FPS: {(1f / Time.deltaTime):F0}\nPing: {PhotonNetwork.GetPing()}\nRegion: {(PhotonNetwork.CloudRegion ?? "N/A").Replace("/*", "")}\nConnected: {PhotonNetwork.IsConnected}\nIn Room: {PhotonNetwork.InRoom}\nRoom Name: {(PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "N/A")}\nMaster Client: {PhotonNetwork.IsMasterClient}\nPlayer Count: {PhotonNetwork.CountOfPlayers}\nPlayers in Room Count: {PhotonNetwork.CountOfPlayersInRooms}\nRoom Count: {PhotonNetwork.CountOfRooms}\n\nName: {PhotonNetwork.LocalPlayer?.NickName ?? "N/A"}\nUserid: {PhotonNetwork.LocalPlayer?.UserId ?? "N/A"}".ToUpper();
                }
            }

            if (boardmat != null)
                boardmat.color = Settings.backgroundColor.GetCurrentColor();

            if (monitorObj != null && boardRenders != null)
            {
                if (Settings.disablecustomboards)
                {
                    MeshRenderer monitorRenderer = monitorObj.GetComponent<MeshRenderer>();

                    if (monitorRenderer != null && oldMonitorMat != null)
                        monitorRenderer.material = oldMonitorMat;

                    if (ogboardmat != null)
                        boardRenders.material = ogboardmat;
                }
                else
                {
                    MeshRenderer monitorRenderer = monitorObj.GetComponent<MeshRenderer>();

                    if (monitorRenderer != null && boardmat != null)
                        monitorRenderer.material = boardmat;

                    if (boardmat != null)
                        boardRenders.material = boardmat;
                }
            }
        }

        public static void DisableBoards()
        {
            if (Instance == null) return;

            if (Instance.HeadOutsideTMP != null)
                Instance.HeadOutsideTMP.text = Instance.ogHeadoutsideText;

            if (Instance.WelcomeToGorilllaTagHeadingTextTMP != null)
                Instance.WelcomeToGorilllaTagHeadingTextTMP.text = Instance.ogWelcomeToGorilllaTagHeadingText;

            if (motdTMP != null)
                motdTMP.text = "MESSAGE OF THE DAY";

            if (Instance.motdTextTMP != null)
            {
                PlayFabTitleDataTextDisplay display = Instance.motdTextTMP.gameObject.GetComponent<PlayFabTitleDataTextDisplay>();

                if (display != null)
                    Instance.motdTextTMP.text = display._cachedText;
            }

            if (Instance.cocText != null)
                Instance.cocText.SetActive(true);

            if (Instance.cocHeading != null)
                Instance.cocHeading.SetActive(true);

            if (Instance.monitorObj != null)
            {
                MeshRenderer monitorRenderer = Instance.monitorObj.GetComponent<MeshRenderer>();

                if (monitorRenderer != null && Instance.oldMonitorMat != null)
                    monitorRenderer.material = Instance.oldMonitorMat;
            }

            if (Instance.boardRenders != null && Instance.ogboardmat != null)
                Instance.boardRenders.material = Instance.ogboardmat;
        }

        public static GameObject _boards;
        public static GameObject boards
        {
            get
            {
                if (_boards == null)
                {
                    var stumpChildren = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom").transform.Children().Where(x =>
                    x.name.Contains("UnityTempFile")).ToList();

                    if (3 >= 0 && 3 < stumpChildren.Count)
                    {
                        var stumpBoard = stumpChildren[3];
                        if (stumpBoard != null)
                            _boards = stumpBoard;
                    }
                }
                return _boards;
            }
        }
    }
}