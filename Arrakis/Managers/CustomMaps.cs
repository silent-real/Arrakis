/*
using static Arrakis.Managers.CustomMaps.SceneMapRegistry;
 * Arrakis | Managers/CustomMaps.cs
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Arrakis.Classes;
using Arrakis.Menu;
using GorillaTagScripts.VirtualStumpCustomMaps;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Arrakis.Managers.CustomMaps
{
    public abstract class CustomMap
    {
        public abstract long MapID { get; }
        public abstract ButtonInfo[] Buttons { get; }
    }
    public static class SceneMapLoader
    {
        public static void Init()
        {
            try
            {
                SceneMapRegistry.FillRegistry();
                SceneManager.activeSceneChanged += OnSceneChanged;
                CheckSceneForMap(SceneManager.GetActiveScene().name);
            }
            catch{}
        }

        public static void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            CheckSceneForMap(newScene.name);
        }

        public static void CheckSceneForMap(string sceneName)
        {
            var map = SceneMapRegistry.GetMapForScene(sceneName);
            if (map != null)
                Manager.UpdateCustomMapsTab(map.MapID);
            else
                Manager.UpdateCustomMapsTab();
        }
    }
    public class SceneMap
    {
        public long MapID { get; }
        public string SceneName { get; }

        public SceneMap(long mapID, string sceneName)
        {
            MapID = mapID;
            SceneName = sceneName;
        }
    }
    public static class SceneMapRegistry
    {
        public static readonly Dictionary<string, SceneMap> sceneMapLookup = new Dictionary<string, SceneMap>();

        public static void RegisterMap(long mapID, string sceneName)
        {
            if (!sceneMapLookup.ContainsKey(sceneName))
                sceneMapLookup.Add(sceneName, new SceneMap(mapID, sceneName));
        }

        public static SceneMap GetMapForScene(string sceneName)
        {
            sceneMapLookup.TryGetValue(sceneName, out var map);
            return map;
        }

        public static void FillRegistry()
        {
            RegisterMap(5135423, "Guns"); // Chimp Combat
            RegisterMap(5107228, "monke-magic-halloween-alt"); // Monke Magic
        }
    }
    public static class Manager
    {
        public static Dictionary<long, string> mapScriptArchives = new Dictionary<long, string>();
        public static Dictionary<long, CustomMap> mapCache = new Dictionary<long, CustomMap>();

        public static long? currentMapId;

        private static string GetScriptPath()
        {
            return Path.Combine(PluginInfo.BaseDirectory, "Scripts", CustomMapLoader.LoadedMapModId + ".luau");
        }

        private static void ReloadScript()
        {
            if (NetworkSystem.Instance.InRoom)
                LuauHud.Instance.RestartLuauScript();

            CustomMapManager.ReturnToVirtualStump();
        }

        public static void UpdateCustomMapsTab(long? mapId = null)
        {
            currentMapId = mapId;

            int page = Main.GetCategory("Custom Maps");
            List<ButtonInfo> newButtons = new List<ButtonInfo>();

            newButtons.Add(new ButtonInfo
            {
                buttonText = "Exit Custom Maps",
                method = () => Main.CurrentCategoryName = "Main",
                isTogglable = false,
                toolTip = "Returns you back to the main page."
            });

            if (mapId == null)
            {
                newButtons.Add(new ButtonInfo
                {
                    buttonText = "You have not loaded a map.",
                    label = true
                });

                Buttons.buttons[page] = newButtons.ToArray();
                return;
            }

            long id = mapId.Value;

            if (!mapScriptArchives.ContainsKey(id))
                mapScriptArchives.Add(id, CustomGameMode.LuaScript);

            CustomMap map = GetMapByID(id);

            if (map != null)
                newButtons.AddRange(map.Buttons);
            else
                newButtons.Add(new ButtonInfo
                {
                    buttonText = "This map is not supported.",
                    label = true
                });

            newButtons.Add(new ButtonInfo { buttonText = " ", label = true });

            newButtons.Add(new ButtonInfo
            {
                buttonText = "Edit Custom Script",
                method = EditUserScript,
                toolTip = "Opens your custom script for this map."
            });

            newButtons.Add(new ButtonInfo
            {
                buttonText = "Delete Custom Script",
                method = DeleteUserScript,
                toolTip = "Deletes your custom script for this map."
            });

            newButtons.Add(new ButtonInfo
            {
                buttonText = "Run Custom Script",
                enableMethod = StartUserScript,
                disableMethod = StopUserScript,
                toolTip = "Runs your custom script for this map."
            });

            Buttons.buttons[page] = newButtons.ToArray();
        }

        public static void ChangeCustomScript(Dictionary<int, string> edits)
        {
            string[] script = CustomGameMode.LuaScript.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (KeyValuePair<int, string> edit in edits)
            {
                if (edit.Key < 0 || edit.Key >= script.Length)
                    continue;

                Debug.Log("Changed line " + edit.Key);
                script[edit.Key] = edit.Value;
            }

            CustomGameMode.LuaScript = string.Join(Environment.NewLine, script);

            ReloadScript();
        }

        public static void EditUserScript()
        {
            string path = GetScriptPath();

            if (!File.Exists(path))
            {
                string text = CustomGameMode.LuaScript;

                if (string.IsNullOrWhiteSpace(text))
                {
                    text = "-- This map does not have a Lua script.\n" +
                           "-- You can write your own script here.";
                }
                else if (mapScriptArchives.ContainsKey(CustomMapManager.currentRoomMapModId))
                {
                    text = mapScriptArchives[CustomMapManager.currentRoomMapModId];
                }

                File.WriteAllText(path, text);
            }

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }

        public static void DeleteUserScript()
        {
            string path = GetScriptPath();

            if (File.Exists(path))
                File.Delete(path);
        }

        public static void StartUserScript()
        {
            string path = GetScriptPath();

            if (File.Exists(path))
                CustomGameMode.LuaScript = File.ReadAllText(path);

            ReloadScript();
        }

        public static void StopUserScript()
        {
            if (mapScriptArchives.ContainsKey(CustomMapManager.currentRoomMapModId))
                CustomGameMode.LuaScript = mapScriptArchives[CustomMapManager.currentRoomMapModId];

            ReloadScript();
        }

        public static void ResetCustomScript(int line)
        {
            ResetCustomScript(new int[] { line });
        }

        public static void ResetCustomScript(int[] lines)
        {
            if (!mapScriptArchives.ContainsKey(CustomMapManager.currentRoomMapModId))
                return;

            string[] original = mapScriptArchives[CustomMapManager.currentRoomMapModId]
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            Dictionary<int, string> restore = new Dictionary<int, string>();

            foreach (int line in lines)
            {
                if (line >= 0 && line < original.Length)
                    restore[line] = original[line];
            }

            ChangeCustomScript(restore);
        }

        public static CustomMap GetMapByID(long id)
        {
            if (mapCache.ContainsKey(id))
                return mapCache[id];

            Type[] allTypes;

            try
            {
                allTypes = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                allTypes = ex.Types.Where(x => x != null).ToArray();
            }

            CustomMap result = null;

            foreach (Type type in allTypes)
            {
                if (!type.IsSubclassOf(typeof(CustomMap)) || type.IsAbstract)
                    continue;

                CustomMap instance = (CustomMap)Activator.CreateInstance(type);

                if (instance.MapID == id)
                {
                    result = instance;
                    break;
                }
            }

            mapCache[id] = result;
            return result;
        }
    }
}