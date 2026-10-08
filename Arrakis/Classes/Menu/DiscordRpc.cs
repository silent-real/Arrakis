/*
 * Arrakis | Classes/Menu/DiscordRpc.cs
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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;

namespace Arrakis.Classes.Menu
{
    public class DiscordRpc // i made og for bunny.lol pcvr then FOR SOME REASON it didnt work, asked AI to add linux support (nw rn) and fix windows -sleepy
    {
        private const string ClientId = "1557416786758992052";

        private const int OpHandshake = 0;
        private const int OpFrame = 1;
        private const int OpClose = 2;

        private static Socket linuxSocket;
        private static NamedPipeClientStream windowsPipe;

        private static bool connected;
        public static bool Connected => connected;
        private static long nonce;

        public static void Initialize()
        {
            CustomConsole.Log("Initializing Discord RPC...", CustomConsole.LogType.Debug);
            if (TryLinux() || TryWindows())
            {
                connected = true;
                if (!SendHandshake())
                {
                    CustomConsole.Log("Discord handshake failed.", CustomConsole.LogType.Warning);
                    Shutdown();
                    return;
                }
                if (!ReadFrame(out _, out string ready))
                {
                    CustomConsole.Log("No handshake reply from Discord.", CustomConsole.LogType.Warning);
                    Shutdown();
                    return;
                }
                CustomConsole.Log("Discord RPC connected and ready.", CustomConsole.LogType.Info);
                Update();
                return;
            }
            CustomConsole.Log("Could not connect to Discord IPC.", CustomConsole.LogType.Warning);
        }

        private static bool TryLinux()
        {
            foreach (string path in GetLinuxPaths())
            {
                CustomConsole.Log($"Trying Linux Discord IPC socket: {path}", CustomConsole.LogType.Debug);
                try
                {
                    linuxSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified)
                    {
                        ReceiveTimeout = 2000,
                        SendTimeout = 2000
                    };
                    var result = linuxSocket.BeginConnect(new UnixDomainSocketEndPoint(path), null, null);
                    if (!result.AsyncWaitHandle.WaitOne(1000) || !linuxSocket.Connected)
                        throw new TimeoutException("connect timed out");
                    linuxSocket.EndConnect(result);
                    CustomConsole.Log($"Linux Discord IPC connected: {path}", CustomConsole.LogType.Debug);
                    return true;
                }
                catch (Exception ex)
                {
                    CustomConsole.Log($"Linux IPC failed for {path}: {ex.Message}", CustomConsole.LogType.Debug);
                    try { linuxSocket?.Dispose(); } catch { }
                    linuxSocket = null;
                }
            }
            CustomConsole.Log("No usable Linux Discord IPC socket found.", CustomConsole.LogType.Debug);
            return false;
        }

        private static bool TryWindows()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    CustomConsole.Log($"Trying Windows Discord IPC pipe {i}...", CustomConsole.LogType.Debug);

                    windowsPipe = new NamedPipeClientStream(".", "discord-ipc-" + i, PipeDirection.InOut);
                    windowsPipe.Connect(1000);
                    if (windowsPipe.IsConnected)
                    {
                        CustomConsole.Log($"Windows Discord IPC connected to pipe {i}.", CustomConsole.LogType.Debug);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    CustomConsole.Log($"Windows IPC pipe {i} failed: {ex.Message}", CustomConsole.LogType.Debug);
                    try { windowsPipe?.Dispose(); } catch { }
                    windowsPipe = null;
                }
            }

            return false;
        }

        private static string[] GetLinuxPaths()
        {
            var paths = new List<string>();
            string runtime = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            if (!string.IsNullOrEmpty(runtime))
            {
                CustomConsole.Log($"XDG_RUNTIME_DIR = {runtime}", CustomConsole.LogType.Debug);
                AddDiscordSockets(paths, runtime);
            }
            else
            {
                CustomConsole.Log("XDG_RUNTIME_DIR is not set. Falling back to /run/user/<uid>.", CustomConsole.LogType.Debug);
            }
            string uid = GetUserId();
            if (!string.IsNullOrEmpty(uid))
            {
                string userRuntime = "/run/user/" + uid;
                CustomConsole.Log($"Using user runtime directory: {userRuntime}", CustomConsole.LogType.Debug);
                AddDiscordSockets(paths, userRuntime);
            }
            return paths.ToArray();
        }

        private static void AddDiscordSockets(List<string> paths, string runtime)
        {
            string[] prefixes =
            {
                "", "app/com.discordapp.Discord/", ".flatpak/com.discordapp.Discord/xdg-run/",".flatpak/dev.vencord.Vesktop/xdg-run/"
            };
            foreach (string prefix in prefixes)
                for (int i = 0; i < 10; i++)
                    paths.Add(Path.Combine(runtime, prefix.Replace('/', Path.DirectorySeparatorChar) + "discord-ipc-" + i));
        }

        private static string GetUserId()
        {
            try
            {
                string uid = Environment.GetEnvironmentVariable("UID");
                if (!string.IsNullOrEmpty(uid))
                    return uid;
                if (Directory.Exists("/run/user"))
                {
                    foreach (string directory in Directory.GetDirectories("/run/user"))
                    {
                        string name = Path.GetFileName(directory);
                        if (int.TryParse(name, out _))
                            return name;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomConsole.Log($"Could not determine Linux user ID: {ex.Message}", CustomConsole.LogType.Debug);
            }
            return null;
        }

        private static bool SendHandshake()
        {
            string payload = "{\"v\":1,\"client_id\":\"" + ClientId + "\"}";
            return Send(OpHandshake, payload);
        }

        public static void Update()
        {
            if (!connected)
                return;

            string details = "Not in a room";
            string state = "Arrakis Mod Menu";
            bool inRoom = false;

            try
            {
                var net = NetworkSystem.Instance;
                if (net != null && net.InRoom)
                {
                    int count = net.PlayerListOthers.Length + 1;
                    int max = PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.MaxPlayers > 0
                        ? PhotonNetwork.CurrentRoom.MaxPlayers
                        : 10;

                    bool visible = PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.IsVisible;
                    string code = visible ? net.RoomName : "Private";

                    details = $"Room: {code}";
                    state = $"{count}/{max} players";
                    inRoom = true;
                }
                else
                {
                    CustomConsole.Log($"RPC: not in room (net={(net == null ? "null" : "ok")}, InRoom={net?.InRoom})", CustomConsole.LogType.Debug);
                }
            }
            catch (Exception ex)
            {
                CustomConsole.Log($"RPC room read threw: {ex.Message}", CustomConsole.LogType.Warning);
            }

            string party = inRoom
                ? "\"party\":{\"size\":[" + RoomSize() + "]},"
                : "";

            string activity =
                "{"
                + "\"cmd\":\"SET_ACTIVITY\","
                + "\"args\":{"
                + "\"pid\":" + Process.GetCurrentProcess().Id + ","
                + "\"activity\":{"
                + "\"details\":\"" + EscapeJson(details) + "\","
                + "\"state\":\"" + EscapeJson(state) + "\","
                + party
                + "\"assets\":{"
                + "\"large_image\":\"arrakislogo\","
                + "\"large_text\":\"Arrakis\""
                + "},"
                + "\"buttons\":[{"
                + "\"label\":\"Arrakis Discord\","
                + "\"url\":\"" + EscapeJson(PluginInfo.DiscordLink) + "\""
                + "}]"
                + "}"
                + "},"
                + "\"nonce\":\"" + (++nonce) + "\""
                + "}";

            Send(OpFrame, activity);
        }

        private static string RoomSize()
        {
            try
            {
                var net = NetworkSystem.Instance;
                if (net != null && net.InRoom)
                {
                    int max = PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.MaxPlayers > 0
                        ? PhotonNetwork.CurrentRoom.MaxPlayers
                        : 10;
                    return (net.PlayerListOthers.Length + 1) + "," + max;
                }
            }
            catch { }
            return "0,0";
        }

        private static bool Send(int opcode, string payload)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(payload);
                byte[] header = new byte[8];
                WriteLE(header, 0, opcode);
                WriteLE(header, 4, data.Length);
                if (linuxSocket != null && linuxSocket.Connected)
                {
                    SendSocket(linuxSocket, header);
                    SendSocket(linuxSocket, data);
                    return true;
                }
                if (windowsPipe != null && windowsPipe.IsConnected)
                {
                    windowsPipe.Write(header, 0, header.Length);
                    windowsPipe.Write(data, 0, data.Length);
                    windowsPipe.Flush();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                CustomConsole.Log($"Discord RPC send failed: {ex.Message}", CustomConsole.LogType.Warning);
                connected = false;
                return false;
            }
        }

        private static bool ReadFrame(out int opcode, out string payload)
        {
            opcode = -1;
            payload = null;
            try
            {
                byte[] header = new byte[8];
                if (!ReadExact(header, 8))
                    return false;
                opcode = ReadLE(header, 0);
                int length = ReadLE(header, 4);
                if (length < 0 || length > 64 * 1024)
                    return false;
                byte[] body = new byte[length];
                if (length > 0 && !ReadExact(body, length))
                    return false;
                payload = Encoding.UTF8.GetString(body);
                if (opcode == OpClose)
                {
                    CustomConsole.Log($"Discord closed the connection: {payload}", CustomConsole.LogType.Warning);
                    connected = false;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                CustomConsole.Log($"Discord RPC read failed: {ex.Message}", CustomConsole.LogType.Warning);
                connected = false;
                return false;
            }
        }

        private static bool ReadExact(byte[] buffer, int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int read;
                if (linuxSocket != null)
                    read = linuxSocket.Receive(buffer, offset, count - offset, SocketFlags.None);
                else if (windowsPipe != null)
                    read = windowsPipe.Read(buffer, offset, count - offset);
                else
                    return false;
                if (read <= 0)
                    return false;
                offset += read;
            }
            return true;
        }
        private static void SendSocket(Socket socket, byte[] data)
        {
            int offset = 0;
            while (offset < data.Length)
            {
                int sent = socket.Send(data, offset, data.Length - offset, SocketFlags.None);
                if (sent <= 0)
                    throw new IOException("Discord IPC socket disconnected.");
                offset += sent;
            }
        }
        private static void WriteLE(byte[] buffer, int offset, int value)
        {
            buffer[offset + 0] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }
        private static int ReadLE(byte[] buffer, int offset)
        {
            return buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
        }

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public static void Shutdown()
        {
            connected = false;
            try { linuxSocket?.Dispose(); } catch { }
            try { windowsPipe?.Dispose(); } catch { }
            linuxSocket = null;
            windowsPipe = null;
        }
    }

    public class RpcRoomWatcher : MonoBehaviourPunCallbacks
    {
        public override void OnJoinedRoom() => DiscordRpc.Update();
        public override void OnLeftRoom() => DiscordRpc.Update();
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player _) => DiscordRpc.Update();
        public override void OnPlayerLeftRoom(Photon.Realtime.Player _) => DiscordRpc.Update();
    }
}