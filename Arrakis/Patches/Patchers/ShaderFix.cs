/*
 * Arrakis | Patches/Patchers/ShaderFix.cs
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

using HarmonyLib;
using UnityEngine;

namespace Arrakis.Patches.Menu
{
    [HarmonyPatch(typeof(GameObject), nameof(GameObject.CreatePrimitive))]
    public class ShaderFix
    {
        private static void Postfix(GameObject __result)
        {
            __result.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
            __result.GetComponent<Renderer>().material.color = Color.black;
        }
    }
}