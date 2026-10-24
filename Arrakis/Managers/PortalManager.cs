/*
 * Arrakis | Managers/PortalManager.cs
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
using UnityEngine.InputSystem;
using static Arrakis.Menu.Main;

namespace Arrakis.Managers
{
    public class PortalManager
    {
        private static GameObject _orangePortal = null;
        private static GameObject _bluePortal = null;

        private static Camera _orangeCam = null;
        private static Camera _blueCam = null;

        private static RenderTexture _orangeRT = null;
        private static RenderTexture _blueRT = null;

        private static bool _camsInitialized = false;
        private static PortalType _activeColor = PortalType.Orange;
        private static bool _lastTrigger = false;

        private const string PortalLayerName = "PortalSurface";
        private static int _portalLayer = -1;

        private static int GetPortalLayer()
        {
            if (_portalLayer != -1) return _portalLayer;
            _portalLayer = LayerMask.NameToLayer(PortalLayerName);
            if (_portalLayer == -1) _portalLayer = 31;
            return _portalLayer;
        }

        private static void EnsureCameras()
        {
            if (_camsInitialized) return;
            _orangeRT = new RenderTexture(512, 1024, 16, RenderTextureFormat.Default);
            _orangeRT.Create();
            _blueRT = new RenderTexture(512, 1024, 16, RenderTextureFormat.Default);
            _blueRT.Create();
            int excludePortals = ~(1 << GetPortalLayer());
            _orangeCam = new GameObject("Cam_orange").AddComponent<Camera>();
            _orangeCam.targetTexture = _orangeRT;
            _orangeCam.nearClipPlane = 0.15f;
            _orangeCam.farClipPlane = 500f;
            _orangeCam.fieldOfView = 90f;
            _orangeCam.cullingMask = excludePortals;
            _orangeCam.clearFlags = CameraClearFlags.Skybox;
            _orangeCam.backgroundColor = Color.black;
            _orangeCam.enabled = false;
            _blueCam = new GameObject("Cam_blue").AddComponent<Camera>();
            _blueCam.targetTexture = _blueRT;
            _blueCam.nearClipPlane = 0.15f;
            _blueCam.farClipPlane = 500f;
            _blueCam.fieldOfView = 90f;
            _blueCam.cullingMask = excludePortals;
            _blueCam.clearFlags = CameraClearFlags.Skybox;
            _blueCam.backgroundColor = Color.black;
            _blueCam.enabled = false;
            _camsInitialized = true;
        }

        public static void SpawnPortal()
        {
            EnsureCameras();
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                _activeColor = _activeColor == PortalType.Orange ? PortalType.Blue : PortalType.Orange;
            }
            if (!GetGunInput(false)) return;
            var GunData = RenderGun();
            GameObject NewPointer = GunData.NewPointer;
            if (NewPointer == null) return;
            bool triggerNow = GetGunInput(true);
            bool triggerDown = triggerNow && !_lastTrigger;
            _lastTrigger = triggerNow;
            if (!triggerDown) return;
            Vector3 spawnPos = NewPointer.transform.position + new Vector3(0f, 0.9f, 0f);
            if (_activeColor == PortalType.Orange)
                PlacePortal(ref _orangePortal, _orangeCam, "orange", PortalType.Orange, spawnPos);
            else
                PlacePortal(ref _bluePortal, _blueCam, "blue", PortalType.Blue, spawnPos);
            LinkPortalCameras();
        }

        private static void PlacePortal(ref GameObject existing, Camera cam, string portalName, PortalType type, Vector3 spawnPos)
        {
            if (existing != null)
            {
                Object.Destroy(existing);
                existing = null;
            }
            existing = CreatePortalMesh(portalName);
            existing.transform.position = spawnPos;
            cam.transform.SetParent(existing.transform, false);
            cam.transform.localPosition = new Vector3(0f, 0f, 0.2f);
            cam.transform.localRotation = Quaternion.identity;
            AddOutline(existing, GetPortalColor(type));
        }

        private static void LinkPortalCameras()
        {
            if (_orangePortal == null || _bluePortal == null) return;
            _orangeCam.transform.SetParent(_bluePortal.transform, false);
            _orangeCam.transform.localPosition = new Vector3(0f, 0f, 0.2f);
            _orangeCam.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            _orangeCam.targetTexture = _orangeRT;
            _orangeCam.enabled = true;
            _blueCam.transform.SetParent(_orangePortal.transform, false);
            _blueCam.transform.localPosition = new Vector3(0f, 0f, 0.2f);
            _blueCam.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            _blueCam.targetTexture = _blueRT;
            _blueCam.enabled = true;
            ApplyPortalMaterial(_orangePortal, _orangeRT);
            ApplyPortalMaterial(_bluePortal, _blueRT);
        }

        private static void ApplyPortalMaterial(GameObject portal, RenderTexture rt)
        {
            if (portal == null) return;
            var renderer = portal.GetComponent<Renderer>();
            var mat = new Material(renderer.material);
            mat.mainTexture = rt;
            mat.SetTexture("_MainTex", rt);
            mat.SetTexture("_BaseMap", rt);
            mat.SetTexture("_BaseColorMap", rt);
            mat.EnableKeyword("_EMISSION");
            mat.SetTexture("_EmissionMap", rt);
            mat.SetColor("_EmissionColor", Color.white);
            mat.color = Color.white;
            mat.SetColor("_BaseColor", Color.white);
            renderer.material = mat;
        }

        private static GameObject CreatePortalMesh(string portalName)
        {
            GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            portal.name = portalName;
            portal.layer = GetPortalLayer();
            portal.transform.localScale = new Vector3(1f, 1f, 0.1f);
            portal.transform.rotation = Quaternion.identity;
            Object.Destroy(portal.GetComponent<Collider>());
            return portal;
        }

        private static void AddOutline(GameObject portal, Color outlineColor)
        {
            GameObject outline = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            outline.name = "Outline";
            outline.layer = GetPortalLayer();
            outline.transform.SetParent(portal.transform, false);
            outline.transform.localPosition = Vector3.zero;
            outline.transform.localRotation = Quaternion.identity;
            outline.transform.localScale = new Vector3(1.08f, 1.04f, 1.5f);
            Object.Destroy(outline.GetComponent<Collider>());
            var mat = new Material(portal.GetComponent<Renderer>().material);
            mat.mainTexture = null;
            mat.SetTexture("_MainTex", null);
            mat.SetTexture("_BaseMap", null);
            mat.color = outlineColor;
            mat.SetColor("_BaseColor", outlineColor);
            mat.SetColor("_EmissionColor", outlineColor);
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Front);
            outline.GetComponent<Renderer>().material = mat;
        }

        public static Color GetPortalColor(PortalType portalType)
        {
            switch (portalType)
            {
                case PortalType.Blue: return Color.blue;
                case PortalType.Orange: return new Color(1f, 0.5f, 0f);
                default: return Color.red;
            }
        }

        public static void ResetPortals()
        {
            if (_orangeCam != null) { _orangeCam.targetTexture = null; Object.Destroy(_orangeCam.gameObject); }
            if (_blueCam != null) { _blueCam.targetTexture = null; Object.Destroy(_blueCam.gameObject); }
            if (_orangeRT != null) { _orangeRT.Release(); Object.Destroy(_orangeRT); }
            if (_blueRT != null) { _blueRT.Release(); Object.Destroy(_blueRT); }
            if (_orangePortal != null) Object.Destroy(_orangePortal);
            if (_bluePortal != null) Object.Destroy(_bluePortal);
            _orangePortal = _bluePortal = null;
            _orangeCam = _blueCam = null;
            _orangeRT = _blueRT = null;
            _activeColor = PortalType.Orange;
            _lastTrigger = false;
            _camsInitialized = false;
        }

        public enum PortalType
        {
            None,
            Blue,
            Orange
        }
    }
}