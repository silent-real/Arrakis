/*
 * Arrakis | Patches/Logging/LogEvent.cs
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

using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

namespace Arrakis.Patches
{
    public class LogEvent : MonoBehaviour
    {
        public void Awake()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }

        public void OnEvent(EventData data)
        {
            if (PhotonNetwork.InRoom)
            {
                if (!Settings.logphotonevents)
                    return;

                string playerName = "[UNKNOWN]";
                Photon.Realtime.Player plr = PhotonNetwork.CurrentRoom.GetPlayer(data.Sender, false);
                if (plr != null)
                {
                    playerName = plr.NickName;
                }
                if (data.Code != 201 && data.Code != 205 && data.Code != 206 && data.Code != 208)
                {
                    try
                    {
                        if (data.Code != 200)
                        {
                            object[] array = (object[])((Hashtable)data.CustomData)[(byte)4];
                            string thing = "";
                            foreach (object element in array)
                            {
                                try
                                {
                                    thing += element.ToString() + ", ";
                                }
                                catch { thing += "[Could not find value]" + ", "; }
                            }
                            CustomConsole.Log($"Event by {playerName} sent {data.Code.ToString()} // {thing}", CustomConsole.LogType.Info);
                        }
                        else
                        {
                            object[] array = (object[])((Hashtable)data.CustomData)[(byte)4];
                            string thing = "";
                            foreach (object element in array)
                            {
                                try
                                {
                                    thing += element.ToString() + ", ";
                                }
                                catch { thing += "[Could not find value]" + ", "; }
                            }
                            CustomConsole.Log($"RPC by {playerName} sent {PhotonNetwork.PhotonServerSettings.RpcList[int.Parse(((Hashtable)data.CustomData)[(byte)5].ToString())]} // {thing}", CustomConsole.LogType.Info);
                        }
                    }
                    catch { }
                }
            }
        }
    }
}