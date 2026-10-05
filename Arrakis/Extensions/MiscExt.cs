/*
 * Arrakis | Extensions/MiscExt.cs
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

using System.Collections.Generic;
using UnityEngine;

namespace Arrakis.Extensions
{
    public static class MiscExt
    {
        public static IEnumerable<GameObject> Children(this Transform t)
        {
            var list = new List<GameObject>();
            for (int i = 0; i < t.childCount; i++)
                list.Add(t.GetChild(i).gameObject);
            return list;
        }

        public static string CleanString(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return "s is somehow empty";
            return s.Replace("<", "").Replace(">", "").Replace("\n", "").Replace("\r", "").Replace("\b", "")
                .Replace("/", "").Replace(" ", "").Replace("discord.gg", "")
                .Replace("<color", "").Replace("</color>", "").Replace("<size", "");
        }
    }
}