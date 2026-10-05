/*
 * Arrakis | Mods/Soundboard.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Arrakis.Classes;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Arrakis.Mods
{
    public static class Soundboard
    {
        private static readonly string[] SupportedExtensions = { ".mp3", ".wav", ".ogg" };
        private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, ButtonInfo> SoundButtons = new Dictionary<string, ButtonInfo>(StringComparer.OrdinalIgnoreCase);

        private static GameObject audioObject;
        private static AudioSource audioSource;
        private static int playbackId;
        private static bool mymic;

        public static bool LoopAudio = false;
        public static bool HearSelf = true;
        public static float LocalVolume = 0.2f;
        public static bool IsPlaying { get; private set; }
        public static string CurrentFile { get; private set; }

        private static string SoundsPath =>
            Path.GetFullPath(Path.Combine(PluginInfo.BaseDirectory, "Sounds"));

        public static void Play(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;
            fileName = Path.GetFileName(fileName);
            if (!IsSupportedFile(fileName))
                return;
            string path = Path.Combine(SoundsPath, fileName);
            if (!File.Exists(path))
            {
                CustomConsole.Log("Cant find sound " + path, CustomConsole.LogType.Error);
                SyncButtons(CurrentFile);
                return;
            }
            int id = ++playbackId;
            StopPlayback(restoreMicrophone: false);
            CurrentFile = fileName;
            SyncButtons(fileName);
            EnsureObject();
            CRunner.instance.StartCoroutine(Load(path, id));
        }

        private static IEnumerator Load(string path, int id)
        {
            string fileName = Path.GetFileName(path);
            if (!Cache.TryGetValue(fileName, out AudioClip clip) || clip == null)
            {
                string uri = new Uri(Path.GetFullPath(path)).AbsoluteUri;
                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, GetAudioType(path)))
                {
                    DownloadHandlerAudioClip handler = (DownloadHandlerAudioClip)request.downloadHandler;
                    handler.streamAudio = false;
                    handler.compressed = false;
                    yield return request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        CustomConsole.Log("Sound loading failed " + request.error, CustomConsole.LogType.Error);
                        if (id == playbackId)
                            Stop();
                        yield break;
                    }
                    clip = DownloadHandlerAudioClip.GetContent(request);
                    if (clip == null)
                    {
                        CustomConsole.Log("clip is null cant load.", CustomConsole.LogType.Error);
                        if (id == playbackId)
                            Stop();
                        yield break;
                    }
                    clip.name = Path.GetFileNameWithoutExtension(fileName);
                    Cache[fileName] = clip;
                }
            }

            if (id != playbackId)
                yield break;

            PlayClip(clip);

            if (!LoopAudio)
                CRunner.instance.StartCoroutine(WatchForEnd(clip.length, id));
        }

        private static IEnumerator WatchForEnd(float length, int id)
        {
            yield return new WaitForSecondsRealtime(length + 0.25f);
            if (id == playbackId && !LoopAudio)
                Stop();
        }

        private static void PlayClip(AudioClip clip)
        {
            if (clip == null)
                return;

            bool inRoom = InRoom();
            if (inRoom)
                PlayPhoton(clip);
            if (!inRoom || HearSelf)
                PlayLocal(clip);

            IsPlaying = true;
        }

        private static void PlayPhoton(AudioClip clip)
        {
            try
            {
                Recorder recorder = GetRecorder();
                if (recorder == null)
                {
                    CustomConsole.Log("Somehow the recorder is null 😭.", CustomConsole.LogType.Error);
                    return;
                }
                recorder.AudioClip = clip;
                recorder.LoopAudioClip = LoopAudio;
                recorder.SourceType = Recorder.InputSourceType.AudioClip;
                recorder.RestartRecording(true);
                mymic = true;
            }
            catch (Exception e)
            {
                CustomConsole.Log("Soundboard: Photon playback error: " + e, CustomConsole.LogType.Error);
            }
        }

        private static void PlayLocal(AudioClip clip)
        {
            EnsureObject();
            audioSource.clip = clip;
            audioSource.loop = LoopAudio;
            audioSource.volume = Mathf.Clamp01(LocalVolume);
            audioSource.Play();
        }

        public static void Stop()
        {
            playbackId++;
            StopPlayback(restoreMicrophone: true);
            CurrentFile = null;
            SyncButtons(null);
        }

        private static void StopPlayback(bool restoreMicrophone)
        {
            IsPlaying = false;
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
            if (restoreMicrophone)
                RestoreMicrophone();
        }

        public static void RestoreMicrophone()
        {
            if (!mymic)
                return;
            try
            {
                Recorder recorder = GetRecorder();
                if (recorder == null)
                    return;
                recorder.SourceType = Recorder.InputSourceType.Microphone;
                recorder.AudioClip = null;
                recorder.LoopAudioClip = false;
                recorder.RestartRecording(true);
            }
            catch (Exception e)
            {
                CustomConsole.Log("Failed to restore mic " + e, CustomConsole.LogType.Error);
            }
            finally
            {
                mymic = false;
            }
        }

        private static bool InRoom() =>
            NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom;

        private static Recorder GetRecorder() =>
            NetworkSystem.Instance != null ? NetworkSystem.Instance.LocalRecorder : null;

        private static bool IsSupportedFile(string fileName) =>
            Array.IndexOf(SupportedExtensions, Path.GetExtension(fileName).ToLowerInvariant()) >= 0;

        public static string GamePath() =>
            AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        private static void SyncButtons(string activeFile)
        {
            foreach (KeyValuePair<string, ButtonInfo> entry in SoundButtons)
                entry.Value.enabled = activeFile != null && string.Equals(entry.Key, activeFile, StringComparison.OrdinalIgnoreCase);
        }

        private static void OpenSoundsFolder()
        {
            string path = SoundsPath;
            Directory.CreateDirectory(path);
            try
            {
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch
            {
                Application.OpenURL(new Uri(path).AbsoluteUri);
            }
        }

        public static void StartSoundboard()
        {
            int categoryIndex = Array.IndexOf(Menu.Buttons.categoryNames, "Soundboard");
            if (categoryIndex < 0)
            {
                CustomConsole.Log("Could not find soundboard category.", CustomConsole.LogType.Error);
                return;
            }
            string soundsPath = SoundsPath;
            Directory.CreateDirectory(soundsPath);
            List<ButtonInfo> buttons = new List<ButtonInfo>
            {
                new ButtonInfo
                {
                    buttonText = "Exit Soundboard",
                    method = () => Menu.Main.CurrentCategoryName = "Main",
                    isTogglable = false,
                    toolTip = "Returns to the main page for the menu."
                },
                new ButtonInfo
                {
                    buttonText = "Reload Soundboard",
                    method = () => StartSoundboard(),
                    isTogglable = false,
                    toolTip = "Reloads the soundboard page."
                },
                new ButtonInfo
                {
                    buttonText = "Open Sound Folder",
                    method = OpenSoundsFolder,
                    isTogglable = false,
                    toolTip = "Opens the sound folder."
                },
                new ButtonInfo
                {
                    buttonText = "Hear Self",
                    enableMethod = () => HearSelf = true,
                    disableMethod = () =>
                    {
                        HearSelf = false;
                        if (InRoom() && audioSource != null)
                            audioSource.Stop();
                    },
                    isTogglable = true,
                    enabled = HearSelf,
                    toolTip = "Play sounds locally while in a room."
                },
                new ButtonInfo
                {
                    buttonText = "Loop Audio",
                    enableMethod = () => LoopAudio = true,
                    disableMethod = () => LoopAudio = false,
                    isTogglable = true,
                    enabled = LoopAudio,
                    toolTip = "Loops the next sound you play."
                },
                new ButtonInfo
                {
                    buttonText = "Stop All Sounds",
                    method = Stop,
                    isTogglable = false,
                    toolTip = "Stops all sounds currently playing."
                }
            };

            SoundButtons.Clear();

            IEnumerable<string> files = Directory.GetFiles(soundsPath).Where(IsSupportedFile).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string displayName = Path.GetFileNameWithoutExtension(file);
                ButtonInfo soundButton = new ButtonInfo
                {
                    buttonText = displayName,
                    enableMethod = () => Play(fileName),
                    disableMethod = () =>
                    {
                        if (string.Equals(CurrentFile, fileName, StringComparison.OrdinalIgnoreCase))
                            Stop();
                    },
                    isTogglable = true,
                    enabled = string.Equals(CurrentFile, fileName, StringComparison.OrdinalIgnoreCase),
                    toolTip = "Play " + displayName
                };
                SoundButtons[fileName] = soundButton;
                buttons.Add(soundButton);
            }
            Menu.Buttons.buttons[categoryIndex] = buttons.ToArray();
            Menu.Main.CurrentCategoryName = "Soundboard";
        }

        private static AudioType GetAudioType(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".wav": return AudioType.WAV;
                case ".ogg": return AudioType.OGGVORBIS;
                case ".mp3": return AudioType.MPEG;
                default: return AudioType.UNKNOWN;
            }
        }

        private static void EnsureObject()
        {
            if (audioObject != null)
                return;

            audioObject = new GameObject("Arrakis Soundboard");
            UnityEngine.Object.DontDestroyOnLoad(audioObject);
            audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        public static void ClearCache()
        {
            Stop();
            foreach (AudioClip clip in Cache.Values)
            {
                if (clip != null)
                    UnityEngine.Object.Destroy(clip);
            }
            Cache.Clear();
        }
    }
}