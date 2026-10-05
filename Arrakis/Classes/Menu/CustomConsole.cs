/*
 * Arrakis | Classes/Menu/CustomConsole.cs
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

using UnityEngine;

namespace Arrakis
{
    public class CustomConsole
    {
        public enum LogType
        {
            Info, Debug, Warning, Error
        }
        private static string type = "";
        public static void Log(string message, LogType logType = LogType.Info, string prefix = "ARRAKIS")
        {
            if (string.IsNullOrEmpty(message))
                return;
            switch (logType)
            {
                case LogType.Info: type = "Info"; break;
                case LogType.Debug: type = "Debug"; break;
                case LogType.Warning: type = "Warning"; break;
                case LogType.Error: type = "Error"; break;
            }
            Debug.Log($"[{prefix}] {type}: {message}");
        }
        public static void LogNoType(string message) => 
            Debug.Log(message);

        private static string ascii = @$"
              ___                 _    _     
             / _ \               | |  (_)    
            / /_\ \_ __ _ __ __ _| | ___ ___ 
            |  _  | '__| '__/ _` | |/ / / __|
            | | | | |  | | | (_| |   <| \__ \
            \_| |_/_|  |_|  \__,_|_|\_\_|___/
                             
                     Version: {PluginInfo.Version} 
                    By Nova, Sleepy
                                 
";
        public static void LoadStart()
        {
            Log("Thank you for using Arrakis.", LogType.Info);
            LogNoType(ascii);
        }
    }
}