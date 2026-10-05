/*
 * Arrakis | Mods/Sound.cs
 *
 * Copyright (C) 2026 Arrakis
 * https://github.com/real-Silent/ArrakisV2
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

using Arrakis.Managers;
using UnityEngine.XR;
using UnityEngine;

namespace Arrakis.Mods
{
    public class Sound
    {
        public static float SoundDelay = 0f;
        private static void PlaySound(int soundIndex, bool leftHand, float volume, bool button)
        {
            if (button)
            {
                if (NetworkSystem.Instance.InRoom && Time.time > SoundDelay)
                {
                    SoundDelay = Time.time + 0.1f;
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", Photon.Pun.RpcTarget.All, new object[] { soundIndex, leftHand, volume });
                    Safety.RPCProc();
                }
                else
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(soundIndex, leftHand, volume);
            }
        }
        public static void SoundSpam(int index) =>
            PlaySound(index, false, 99f, InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Right, !XRSettings.isDeviceActive));
    }
}