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

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.IO.Pipes;

namespace Arrakis.Classes.Menu
{
    public class DiscordRpc // i made og for bunny.lol pcvr then FOR SOME REASON it didnt work, asked AI to add linux support (nw rn) and fix windows -sleepy
    {
        private const string ClientId = "1557416786758992052";

        private static Socket linuxSocket;
        private static NamedPipeClientStream windowsPipe;

        private static bool connected;
        private static long nonce;

        public static void Initialize()
        {
            CustomConsole.Log("Initializing Discord RPC...", CustomConsole.LogType.Debug);

            if (TryLinux())
            {
                CustomConsole.Log("Connected to Discord through Linux IPC.", CustomConsole.LogType.Info);
                connected = true;
                SendHandshake();
                Update();
                return;
            }

            CustomConsole.Log(
                "Linux IPC was not available, trying Windows IPC...",
                CustomConsole.LogType.Debug
            );

            if (TryWindows())
            {
                CustomConsole.Log("Connected to Discord through Windows IPC.", CustomConsole.LogType.Info);
                connected = true;
                SendHandshake();
                Update();
                return;
            }

            CustomConsole.Log(
                "Could not connect to Discord IPC.",
                CustomConsole.LogType.Warning
            );
        }

        private static bool TryLinux()
        {
            string[] paths = GetLinuxPaths();

            foreach (string path in paths)
            {
                CustomConsole.Log(
                    $"Trying Linux Discord IPC socket: {path}",
                    CustomConsole.LogType.Debug
                );

                try
                {
                    linuxSocket = new Socket(
                        AddressFamily.Unix,
                        SocketType.Stream,
                        ProtocolType.Unspecified
                    );

                    linuxSocket.ReceiveTimeout = 2000;
                    linuxSocket.SendTimeout = 2000;

                    var endpoint = new UnixDomainSocketEndPoint(path);

                    linuxSocket.Connect(endpoint);

                    if (linuxSocket.Connected)
                    {
                        CustomConsole.Log(
                            $"Linux Discord IPC connected: {path}",
                            CustomConsole.LogType.Debug
                        );

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    CustomConsole.Log(
                        $"Linux IPC failed for {path}: {ex.Message}",
                        CustomConsole.LogType.Debug
                    );

                    try
                    {
                        linuxSocket?.Dispose();
                    }
                    catch
                    {
                    }

                    linuxSocket = null;
                }
            }

            CustomConsole.Log(
                "No usable Linux Discord IPC socket found.",
                CustomConsole.LogType.Debug
            );

            return false;
        }

        private static bool TryWindows()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    CustomConsole.Log(
                        $"Trying Windows Discord IPC pipe {i}...",
                        CustomConsole.LogType.Debug
                    );

                    windowsPipe = new NamedPipeClientStream(
                        ".",
                        "discord-ipc-" + i,
                        PipeDirection.InOut
                    );

                    windowsPipe.Connect(1000);

                    if (windowsPipe.IsConnected)
                    {
                        CustomConsole.Log(
                            $"Windows Discord IPC connected to pipe {i}.",
                            CustomConsole.LogType.Debug
                        );

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    CustomConsole.Log(
                        $"Windows IPC pipe {i} failed: {ex.Message}",
                        CustomConsole.LogType.Debug
                    );

                    try
                    {
                        windowsPipe?.Dispose();
                    }
                    catch
                    {
                    }

                    windowsPipe = null;
                }
            }

            return false;
        }

        private static string[] GetLinuxPaths()
        {
            var paths = new System.Collections.Generic.List<string>();

            string runtime = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");

            if (!string.IsNullOrEmpty(runtime))
            {
                CustomConsole.Log(
                    $"XDG_RUNTIME_DIR = {runtime}",
                    CustomConsole.LogType.Debug
                );

                AddDiscordSockets(paths, runtime);
            }
            else
            {
                CustomConsole.Log(
                    "XDG_RUNTIME_DIR is not set. Falling back to /run/user/<uid>.",
                    CustomConsole.LogType.Debug
                );
            }

            string uid = GetUserId();

            if (!string.IsNullOrEmpty(uid))
            {
                string userRuntime = "/run/user/" + uid;

                CustomConsole.Log(
                    $"Using user runtime directory: {userRuntime}",
                    CustomConsole.LogType.Debug
                );

                AddDiscordSockets(paths, userRuntime);
            }

            return paths.ToArray();
        }

        private static void AddDiscordSockets(
            System.Collections.Generic.List<string> paths,
            string runtime)
        {
            // Normal/native Discord.
            for (int i = 0; i < 10; i++)
            {
                paths.Add(
                    Path.Combine(
                        runtime,
                        "discord-ipc-" + i
                    )
                );
            }

            // Discord Flatpak.
            for (int i = 0; i < 10; i++)
            {
                paths.Add(
                    Path.Combine(
                        runtime,
                        "app",
                        "com.discordapp.Discord",
                        "discord-ipc-" + i
                    )
                );
            }

            // Flatpak xdg-run path.
            for (int i = 0; i < 10; i++)
            {
                paths.Add(
                    Path.Combine(
                        runtime,
                        ".flatpak",
                        "com.discordapp.Discord",
                        "xdg-run",
                        "discord-ipc-" + i
                    )
                );
            }

            // Vesktop Flatpak.
            for (int i = 0; i < 10; i++)
            {
                paths.Add(
                    Path.Combine(
                        runtime,
                        ".flatpak",
                        "dev.vencord.Vesktop",
                        "xdg-run",
                        "discord-ipc-" + i
                    )
                );
            }
        }

        private static string GetUserId()
        {
            try
            {
                string uid = Environment.GetEnvironmentVariable("UID");

                if (!string.IsNullOrEmpty(uid))
                    return uid;

                string home = Environment.GetEnvironmentVariable("HOME");

                if (string.IsNullOrEmpty(home))
                    return null;

                // Try /run/user/<uid> directories.
                if (Directory.Exists("/run/user"))
                {
                    string[] directories = Directory.GetDirectories("/run/user");

                    foreach (string directory in directories)
                    {
                        string name = Path.GetFileName(directory);

                        if (int.TryParse(name, out _))
                            return name;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomConsole.Log(
                    $"Could not determine Linux user ID: {ex.Message}",
                    CustomConsole.LogType.Debug
                );
            }

            return null;
        }

        private static void SendHandshake()
        {
            string payload =
                "{"
                + "\"v\":1,"
                + "\"client_id\":\"" + ClientId + "\""
                + "}";

            Send(0, payload);
        }

        public static void Update()
        {
            if (!connected)
                return;

            string activity =
                "{"
                + "\"cmd\":\"SET_ACTIVITY\","
                + "\"args\":{"
                + "\"pid\":" + Process.GetCurrentProcess().Id + ","
                + "\"activity\":{"
                + "\"details\":\"Playing Arrakis\","
                + "\"state\":\"Arrakis Mod Menu\","
                + "\"assets\":{"
                + "\"large_image\":\"arrakislogo\","
                + "\"large_text\":\"Arrakis\""
                + "},"
                + "\"buttons\":["
                + "{"
                + "\"label\":\"Arrakis Discord\","
                + "\"url\":\"" + EscapeJson(PluginInfo.DiscordLink) + "\""
                + "}"
                + "]"
                + "}"
                + "},"
                + "\"nonce\":\"" + (++nonce) + "\""
                + "}";

            Send(1, activity);
        }

        private static void Send(int opcode, string payload)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(payload);
                byte[] header = new byte[8];

                Buffer.BlockCopy(
                    BitConverter.GetBytes(opcode),
                    0,
                    header,
                    0,
                    4
                );

                Buffer.BlockCopy(
                    BitConverter.GetBytes(data.Length),
                    0,
                    header,
                    4,
                    4
                );

                if (linuxSocket != null && linuxSocket.Connected)
                {
                    SendSocket(linuxSocket, header);
                    SendSocket(linuxSocket, data);
                }
                else if (windowsPipe != null && windowsPipe.IsConnected)
                {
                    windowsPipe.Write(header, 0, header.Length);
                    windowsPipe.Write(data, 0, data.Length);
                    windowsPipe.Flush();
                }
            }
            catch (Exception ex)
            {
                CustomConsole.Log(
                    $"Discord RPC send failed: {ex.Message}",
                    CustomConsole.LogType.Warning
                );

                connected = false;
            }
        }

        private static void SendSocket(Socket socket, byte[] data)
        {
            int offset = 0;

            while (offset < data.Length)
            {
                int sent = socket.Send(
                    data,
                    offset,
                    data.Length - offset,
                    SocketFlags.None
                );

                if (sent <= 0)
                    throw new IOException("Discord IPC socket disconnected.");

                offset += sent;
            }
        }

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        public static void Shutdown()
        {
            connected = false;

            try
            {
                linuxSocket?.Dispose();
            }
            catch
            {
            }

            try
            {
                windowsPipe?.Dispose();
            }
            catch
            {
            }

            linuxSocket = null;
            windowsPipe = null;
        }
    }
}