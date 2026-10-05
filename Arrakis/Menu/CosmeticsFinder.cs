/*
 * Arrakis | Menu/CosmeticsFinder.cs
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

using Arrakis.Notifications;
using GorillaNetworking;
using UnityEngine;

namespace Arrakis.Menu
{
    public class CosmeticsFinder : MonoBehaviour
    {
        private string input = "";
        public void OnGUI()
        {
            if (!Settings.cosmeticfinder) return;

            GUILayout.Label($"CosmeticId: {input}");
            input = GUILayout.TextField(input);
            if (GUILayout.Button("Get Cosmetic Data"))
            {
                NotificationManager.SendNotification($"<color=yellow>[ARRAKIS]</color> Check console for info on {input}");
                CustomConsole.Log($"Got {input} name {CosmeticsController.instance.GetItemNameFromDisplayName(input)}", CustomConsole.LogType.Info);
                CustomConsole.Log($"Got {input} SO {CosmeticsController.instance.GetCosmeticSOFromDisplayName(input)}", CustomConsole.LogType.Info);
            }
        }
    }
}