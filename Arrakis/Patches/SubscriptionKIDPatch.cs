/*
 * Arrakis | Patches/SubscriptionKIDPatch.cs
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

using GorillaTagScripts;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Arrakis
{
    public static class SubscriptionKIDPatch
    {
        private static Harmony harmony;

        public static void Apply()
        {
            try
            {
                harmony = new Harmony("org.nova.cosmeticfixer");
                MethodInfo original = AccessTools.Method(typeof(SubscriptionManager), "InitializePersonalSubscriptionData");
                if (original == null)
                {
                    return;
                }
                AsyncStateMachineAttribute asyncAttribute = original.GetCustomAttribute<AsyncStateMachineAttribute>();
                if (asyncAttribute == null)
                {
                    return;
                }
                Type stateMachineType = asyncAttribute.StateMachineType;
                MethodInfo moveNext = AccessTools.Method(stateMachineType, "MoveNext");
                if (moveNext == null)
                {
                    return;
                }
                harmony.Patch(moveNext, transpiler: new HarmonyMethod(typeof(SubscriptionKIDPatch), nameof(Transpiler)));
            }
            catch { }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;
            foreach (CodeInstruction instruction in instructions)
            {
                if (!patched && instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo &&
                    ((MethodInfo)instruction.operand).Name =="get_InitialisationComplete" &&((MethodInfo)instruction.operand).DeclaringType == typeof(KIDManager))
                {
                    instruction.opcode = OpCodes.Ldc_I4_1;
                    patched = true;
                    yield return instruction;
                    continue;
                }
                yield return instruction;
            }
        }
    }
}