/*
 * Arrakis | Classes/Mods/RigManager.cs
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

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Arrakis.Classes
{
    public class RigManager
    {
        public static VRRig GetVRRigFromPlayer(NetPlayer p) =>
            GorillaGameManager.instance.FindPlayerVRRig(p);

        public static VRRig GetRandomVRRig(bool includeSelf)
        {
            VRRig random = VRRigCache.ActiveRigs[Random.Range(0, VRRigCache.ActiveRigs.Count - 1)];
            if (includeSelf)
                return random;
            else
            {
                if (random != VRRig.LocalRig)
                    return random;
                else
                    return GetRandomVRRig(includeSelf);
            }
        }
        public static VRRig GetClosestVRRig()
        {
            float num = float.MaxValue;
            VRRig outRig = null;
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig.transform.position) < num)
                {
                    num = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig.transform.position);
                    outRig = vrrig;
                }
            }
            return outRig;
        }
        public static NetworkView GetNetworkViewFromVRRig(VRRig p) =>
            p.netView;
        public static PhotonView GetPhotonViewFromVRRig(VRRig p) =>
            p.netView.GetView;
        public static Player GetRandomPlayer(bool includeSelf)
        {
            if (includeSelf)
                return PhotonNetwork.PlayerList[Random.Range(0, PhotonNetwork.PlayerList.Length - 1)];
            else
                return PhotonNetwork.PlayerListOthers[Random.Range(0, PhotonNetwork.PlayerListOthers.Length - 1)];
        }

        public static NetPlayer GetNetPlayerFromVRRig(VRRig p) =>
            GetNetworkViewFromVRRig(p).Owner;

        public static NetPlayer GetNetPlayerFromID(string id)
        {
            NetPlayer found = null;
            foreach (NetPlayer target in NetworkSystem.Instance.PlayerListOthers)
            {
                if (target.UserId == id)
                {
                    found = target;
                    break;
                }
            }
            return found;
        }

        public static Player GetPlayerFromID(string id)
        {
            Player found = null;
            foreach (Player target in PhotonNetwork.PlayerList)
            {
                if (target.UserId == id)
                {
                    found = target;
                    break;
                }
            }
            return found;
        }

        public static NetPlayer GetNetPlayerFromNickName(string nickname)
        {
            NetPlayer found = null;
            foreach (NetPlayer plr in NetworkSystem.Instance.PlayerListOthers)
            {
                if (plr.NickName == nickname)
                {
                    found = plr;
                    break;
                }
            }
            return found;
        }

        public static Color GetPlayerColor(VRRig Player)
        {
            if (Player.bodyRenderer.cosmeticBodyType == GorillaBodyType.Skeleton)
                return Color.green;

            switch (Player.setMatIndex)
            {
                case 1:
                    return Color.red;
                case 2:
                case 11:
                    return new Color32(255, 128, 0, 255);
                case 3:
                case 7:
                    return Color.blue;
                case 12:
                    return Color.green;
                default:
                    return Player.playerColor;
            }
        }
    }
}