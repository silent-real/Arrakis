/*
 * Arrakis | Patches/Logging/LogEvent.cs
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

using System;
using System.Collections;
using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Arrakis.Patches
{
    public class LogEvent : MonoBehaviour
    {
        private const int MaxDepth = 6;
        private const int MaxBytesShown = 32;
        private const int MaxLogLength = 2000;

        public void Awake()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }

        public void OnDestroy()
        {
            if (PhotonNetwork.NetworkingClient != null)
                PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        }

        public void OnEvent(EventData data)
        {
            if (!PhotonNetwork.InRoom || !Settings.logphotonevents)
                return;
            byte code = data.Code;
            if (code == 201 || code == 205 || code == 206 || code == 208)
                return;
            if (data.Sender == PhotonNetwork.LocalPlayer.ActorNumber)
                return;
            string playerName = PhotonNetwork.CurrentRoom.GetPlayer(data.Sender, false)?.NickName ?? "[UNKNOWN]";
            try
            {
                if (code == 200 && data.CustomData is Hashtable rpc)
                {
                    LogRpc(playerName, rpc);
                    return;
                }
                Write($"Event by {playerName} sent {code} // {Format(data.CustomData, 0)}");
            }
            catch (Exception e)
            {
                Write($"Event by {playerName} sent {code} // [failed to format: {e.GetType().Name}: {e.Message}]");
            }
        }

        private static void LogRpc(string playerName, Hashtable rpc)
        {
            string method = "[unknown RPC]";
            if (rpc.ContainsKey((byte)5))
            {
                int index = Convert.ToInt32(rpc[(byte)5]);
                var rpcList = PhotonNetwork.PhotonServerSettings.RpcList;
                method = index >= 0 && index < rpcList.Count ? rpcList[index] : $"[rpc index {index}]";
            }
            else if (rpc[(byte)3] is string name)
            {
                method = name;
            }
            string viewId = rpc.ContainsKey((byte)0) ? rpc[(byte)0].ToString() : "?";
            object args = rpc.ContainsKey((byte)4) ? rpc[(byte)4] : null;
            Write($"RPC by {playerName} sent {method} (view {viewId}) // {Format(args, 0)}");
        }

        private static string Format(object value, int depth)
        {
            if (value == null)
                return "null";
            if (depth > MaxDepth)
                return "...";
            switch (value)
            {
                case string s:
                    return $"\"{s}\"";
                case byte[] bytes:
                    {
                        int shown = Math.Min(bytes.Length, MaxBytesShown);
                        string hex = shown > 0 ? BitConverter.ToString(bytes, 0, shown) : "";
                        return $"byte[{bytes.Length}] {hex}{(bytes.Length > shown ? "..." : "")}";
                    }
                case IDictionary dict:
                    {
                        var sb = new StringBuilder("{");
                        bool first = true;
                        foreach (DictionaryEntry entry in dict)
                        {
                            if (!first) sb.Append(", ");
                            first = false;
                            sb.Append(FormatKey(entry.Key)).Append(": ").Append(Format(entry.Value, depth + 1));
                        }
                        return sb.Append('}').ToString();
                    }
                case IEnumerable seq:
                    {
                        var sb = new StringBuilder("[");
                        bool first = true;
                        foreach (object item in seq)
                        {
                            if (!first) sb.Append(", ");
                            first = false;
                            sb.Append(Format(item, depth + 1));
                        }
                        return sb.Append(']').ToString();
                    }
                default:
                    try { return value.ToString(); }
                    catch { return "[Could not find value]"; }
            }
        }

        private static string FormatKey(object key)
        {
            return key is byte b ? $"(byte){b}" : Format(key, MaxDepth);
        }

        private static void Write(string message)
        {
            if (message.Length > MaxLogLength)
                message = message.Substring(0, MaxLogLength) + "...";
            CustomConsole.Log(message, CustomConsole.LogType.Info);
        }
    }
}