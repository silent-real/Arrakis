/*
 * Arrakis | Mods/Visual.cs
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
using System.Linq;
using System.Text;
using Arrakis.Classes;
using Arrakis.Classes.Mods;
using Arrakis.Extensions;
using Arrakis.Managers;
using Arrakis.Menu;
using GorillaExtensions;
using GorillaLocomotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using static Arrakis.Menu.Main;
using static Arrakis.Settings;
using Hand = Arrakis.Managers.InputManager.Hand;

namespace Arrakis.Mods
{
    public class Visual
    {
        public static Shader EspText =>
            EspShader;

        private enum TagKind { Name = 0, Id = 1, Platform = 2, Fps = 3, Tagged = 4 }
        private const float TagBaseHeight = 1f;
        private const float TagStep = 0.1f;

        private static readonly Dictionary<VRRig, SortedDictionary<TagKind, TextMeshPro>> tagStack = new Dictionary<VRRig, SortedDictionary<TagKind, TextMeshPro>>();
        private static void SetTag(VRRig rig, TagKind kind, TextMeshPro tag, Transform parent, Color color)
        {
            if (rig == null || tag == null) return;
            if (!tagStack.TryGetValue(rig, out var map))
            {
                map = new SortedDictionary<TagKind, TextMeshPro>();
                tagStack[rig] = map;
            }
            List<TagKind> dead = null;
            foreach (var kv in map)
                if (kv.Value == null) (dead ??= new List<TagKind>()).Add(kv.Key);
            if (dead != null)
                foreach (var k in dead) map.Remove(k);
            map[kind] = tag;
            tag.transform.SetParent(parent, false);
            tag.color = color;
            int i = 0;
            foreach (var kv in map)
            {
                var t = kv.Value;
                if (t == null) continue;
                t.transform.localPosition = new Vector3(0f, TagBaseHeight - TagStep * i++, 0f);
            }
        }
        private static void RemoveTag(VRRig rig, TagKind kind)
        {
            if (rig == null) return;
            if (!tagStack.TryGetValue(rig, out var map)) return;
            if (map.TryGetValue(kind, out var t) && t != null)
                Object.Destroy(t.gameObject);
            map.Remove(kind);
            if (map.Count == 0) { tagStack.Remove(rig); return; }
            int i = 0;
            foreach (var kv in map)
            {
                var rt = kv.Value;
                if (rt == null) continue;
                rt.transform.localPosition = new Vector3(0f, TagBaseHeight - TagStep * i++, 0f);
            }
        }

        private static int _occlusionMask;
        private static int OcclusionMask =>
            _occlusionMask != 0 ? _occlusionMask : (_occlusionMask = LayerMask.GetMask("Gorilla Object"));
        private static Shader _uberShader;
        private static Shader TextShader => EspText;
        private static Shader UberShader =>
            _uberShader != null ? _uberShader : (_uberShader = Shader.Find("GorillaTag/UberShader"));
        private static readonly Vector3[] occlusionProbes = new Vector3[5];

        private sealed class LimbChamState
        {
            public GameObject holder;
            public LineRenderer[] lines;
            public bool[] hidden;
            public float nextCheck;
        }
        private static readonly Dictionary<VRRig, LimbChamState> limbChamPool = new Dictionary<VRRig, LimbChamState>();
        private static readonly List<VRRig> limbChamStale = new List<VRRig>();
        private static Material _limbChamMat;
        private static Material LimbChamMat =>
            _limbChamMat != null ? _limbChamMat : (_limbChamMat = new Material(TextShader));
        private const int LimbSegments = 20;
        private const float LimbCheckInterval = 0.15f;
        private const float LimbWidth = 0.02f;
        private static bool TryGetEye(out Vector3 eye)
        {
            Camera cam = Camera.main;
            if (cam != null) { eye = cam.transform.position; return true; }
            if (VRRig.LocalRig != null) { eye = VRRig.LocalRig.transform.position + Vector3.up * 0.5f; return true; }
            eye = default;
            return false;
        }

        private static bool Blocked(Vector3 eye, Vector3 target) =>
            (target - eye).sqrMagnitude >= 0.25f && Physics.Linecast(eye, target, OcclusionMask, QueryTriggerInteraction.Ignore);
        private static bool BehindStuff(VRRig rig, Vector3 eye)
        {
            Transform t = rig.transform;
            Vector3 pos = t.position;
            Vector3 chest = pos + Vector3.up * 0.5f;
            Vector3 side = t.right * 0.3f;
            occlusionProbes[0] = chest;
            occlusionProbes[1] = pos + Vector3.up * 1.2f;
            occlusionProbes[2] = pos;
            occlusionProbes[3] = chest + side;
            occlusionProbes[4] = chest - side;
            int blocked = 0;
            for (int i = 0; i < occlusionProbes.Length; i++)
            {
                if (Blocked(eye, occlusionProbes[i]) && ++blocked >= 3) return true;
                if (blocked + (occlusionProbes.Length - 1 - i) < 3) return false;
            }
            return false;
        }
        private static LimbChamState CreateLimbChams(VRRig rig)
        {
            var state = new LimbChamState
            {
                holder = new GameObject("Arrakis_LimbChams"),
                lines = new LineRenderer[LimbSegments],
                hidden = new bool[LimbSegments],
                nextCheck = Time.time + UnityEngine.Random.Range(0f, LimbCheckInterval)
            };
            state.holder.transform.SetParent(rig.transform, false);
            for (int s = 0; s < LimbSegments; s++)
            {
                GameObject go = new GameObject("seg");
                go.transform.SetParent(state.holder.transform, false);
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.positionCount = 2;
                lr.startWidth = LimbWidth;
                lr.endWidth = LimbWidth;
                lr.numCapVertices = 4;
                lr.sharedMaterial = LimbChamMat;
                lr.enabled = false;
                state.lines[s] = lr;
            }
            return state;
        }

        private static void DisableLimbChams()
        {
            foreach (LimbChamState state in limbChamPool.Values)
            {
                if (state.holder != null)
                    Object.Destroy(state.holder);
            }
            limbChamPool.Clear();
        }

        public static void Chams(int type)
        {
            if (type != 2 && limbChamPool.Count > 0)
                DisableLimbChams();
            if (!NetworkSystem.Instance.InRoom)
            {
                DisableLimbChams();
                return;
            }

            bool hasEye = TryGetEye(out Vector3 eye);
            float now = Time.time;
            Camera cam = Camera.main;

            if (type == 2)
            {
                limbChamStale.Clear();
                foreach (var pair in limbChamPool)
                {
                    if (pair.Key == null || !VRRigCache.ActiveRigs.Contains(pair.Key))
                        limbChamStale.Add(pair.Key);
                }
                foreach (VRRig stale in limbChamStale)
                {
                    if (limbChamPool[stale].holder != null)
                        Object.Destroy(limbChamPool[stale].holder);
                    limbChamPool.Remove(stale);
                }
            }

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal() || rig.mainSkin == null) continue;
                if ((rig.transform.position - cam.transform.position).sqrMagnitude > 225f) continue;
                Material body = rig.mainSkin.material;
                Color chamColor = followmenutheme ? backgroundColor.GetCurrentColor() : rig.IsTagged() ? new Color(0.6f, 0f, 0f, 0.6f) : new Color(0.46f, 0.6f, 0.6f, 0.6f);
                switch (type)
                {
                    case 0:
                        if (!followmenutheme && body.name.ToLower().Contains("it"))
                            chamColor = new Color(0.6f, 0f, 0f, 0.6f);
                        if (body.shader != TextShader)
                            body.shader = TextShader;
                        body.color = chamColor;
                        break;

                    case 1:
                        if (hasEye && BehindStuff(rig, eye))
                        {
                            if (body.shader != TextShader)
                                body.shader = TextShader;
                            body.color = chamColor;
                        }
                        else
                        {
                            if (body.shader != UberShader)
                                body.shader = UberShader;
                            body.color = rig.playerColor;
                        }
                        break;

                    case 2:
                        if (body.shader != UberShader)
                        {
                            body.shader = UberShader;
                            body.color = rig.playerColor;
                        }
                        if (!hasEye)
                            break;
                        if (!limbChamPool.TryGetValue(rig, out LimbChamState state) || state.holder == null)
                        {
                            state = CreateLimbChams(rig);
                            limbChamPool[rig] = state;
                        }
                        Transform[] skinBones = rig.mainSkin.bones;
                        bool recheck = now >= state.nextCheck;
                        if (recheck)
                            state.nextCheck = now + LimbCheckInterval;
                        for (int s = 0; s < LimbSegments; s++)
                        {
                            Vector3 a, b;
                            if (s == 0)
                            {
                                Vector3 head = rig.head.rigTarget.position;
                                a = head + new Vector3(0f, 0.16f, 0f);
                                b = head - new Vector3(0f, 0.4f, 0f);
                            }
                            else
                            {
                                a = skinBones[bones[(s - 1) * 2]].position;
                                b = skinBones[bones[(s - 1) * 2 + 1]].position;
                            }
                            if (recheck)
                                state.hidden[s] = Blocked(eye, (a + b) * 0.5f);
                            LineRenderer lr = state.lines[s];
                            if (lr.enabled != state.hidden[s])
                                lr.enabled = state.hidden[s];
                            if (!state.hidden[s])
                                continue;
                            lr.SetPosition(0, a);
                            lr.SetPosition(1, b);
                            lr.startColor = chamColor;
                            lr.endColor = chamColor;
                        }
                        break;
                }
            }
        }

        public static void DisableChams()
        {
            DisableLimbChams();
            if (!NetworkSystem.Instance.InRoom) return;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal() || rig.mainSkin == null) continue;
                Material m = rig.mainSkin.sharedMaterial;
                if (m != null && m.shader != UberShader) rig.mainSkin.material.shader = UberShader;
                if (m != null && m.color != rig.playerColor) rig.mainSkin.material.color = rig.playerColor;
            }
        }

        private sealed class TracerState
        {
            public GameObject holder;
            public LineRenderer line;
        }
        private static readonly Dictionary<VRRig, TracerState> tracersPool = new Dictionary<VRRig, TracerState>();
        public static void Tracers()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> remove = null;
            foreach (var pair in tracersPool)
            {
                if (pair.Key == null || !VRRigCache.ActiveRigs.Contains(pair.Key))
                {
                    remove ??= new List<VRRig>();
                    remove.Add(pair.Key);
                    if (pair.Value.holder != null) Object.Destroy(pair.Value.holder);
                }
            }
            if (remove != null) foreach (var r in remove) tracersPool.Remove(r);
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!tracersPool.TryGetValue(rig, out var state) || state.holder == null)
                {
                    var holder = new GameObject("Arrakis_Tracer");
                    holder.transform.SetParent(rig.transform, false);
                    var line = holder.AddComponent<LineRenderer>();
                    line.useWorldSpace = true;
                    line.sharedMaterial = LimbChamMat;
                    line.positionCount = 2;
                    line.startWidth = 0.02f;
                    line.endWidth = 0.02f;
                    state = new TracerState { holder = holder, line = line };
                    tracersPool[rig] = state;
                }
                Color color = followmenutheme ? backgroundColor.GetCurrentColor() : rig.IsTagged() ? new Color(0.6f, 0f, 0f) : rig.playerColor;
                color.a = 0.7f;
                state.line.startColor = color;
                state.line.endColor = color;
                state.line.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                state.line.SetPosition(1, rig.transform.position);
            }
        }

        public static void DisableTracers()
        {
            foreach (var st in tracersPool.Values)
                if (st.holder != null) Object.Destroy(st.holder);
            tracersPool.Clear();
        }


        private static Dictionary<VRRig, List<LineRenderer>> boneEspPool = new Dictionary<VRRig, List<LineRenderer>>();
        private static Dictionary<VRRig, List<LineRenderer>> selfBoneEspPool = new Dictionary<VRRig, List<LineRenderer>>();
        private static readonly int[] bones = {
            4, 3, 5, 4, 19, 18, 20, 19, 3, 18, 21, 20, 22, 21, 25, 21, 29, 21, 31, 29, 27, 25, 24, 22, 6, 5, 7, 6, 10, 6, 14, 6, 16, 14, 12, 10, 9, 7
        };
        public static void BoneESP()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> remove = new List<VRRig>();
            foreach (var bone in boneEspPool)
            {
                if (bone.Key == null || !VRRigCache.ActiveRigs.Contains(bone.Key))
                {
                    remove.Add(bone.Key);
                    foreach (var line in bone.Value)
                        if (line != null) Object.Destroy(line);
                }
            }
            foreach (var rig in remove) boneEspPool.Remove(rig);
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal() || rig.mainSkin == null) continue;
                if (!boneEspPool.TryGetValue(rig, out List<LineRenderer> lines) || lines == null || lines.Count == 0 || lines[0] == null)
                {
                    lines = new List<LineRenderer>();
                    var headGO = rig.head.rigTarget.gameObject;
                    var head = headGO.GetComponent<LineRenderer>() ?? headGO.AddComponent<LineRenderer>();
                    head.sharedMaterial = LimbChamMat;
                    head.positionCount = 2;
                    head.startWidth = 0.02f;
                    head.endWidth = 0.02f;
                    lines.Add(head);
                    for (int i = 0; i < 19; i++)
                    {
                        var boneGO = rig.mainSkin.bones[bones[i * 2]].gameObject;
                        var line = boneGO.GetComponent<LineRenderer>() ?? boneGO.AddComponent<LineRenderer>();
                        line.sharedMaterial = LimbChamMat;
                        line.positionCount = 2;
                        line.startWidth = 0.02f;
                        line.endWidth = 0.02f;
                        lines.Add(line);
                    }
                    boneEspPool[rig] = lines;
                }
                Color boneColor = followmenutheme ? backgroundColor.GetCurrentColor() : rig.IsTagged() ? new Color(0.6f, 0f, 0f) : rig.playerColor;
                boneColor.a = 0.9f;
                LineRenderer l0 = lines[0];
                l0.startColor = boneColor;
                l0.endColor = boneColor;
                l0.SetPosition(0, rig.head.rigTarget.transform.position + new Vector3(0f, 0.16f, 0f));
                l0.SetPosition(1, rig.head.rigTarget.transform.position - new Vector3(0f, 0.4f, 0f));
                for (int i = 0; i < 19; i++)
                {
                    var linee = lines[i + 1];
                    if (linee == null) continue;
                    linee.startColor = boneColor;
                    linee.endColor = boneColor;
                    linee.SetPosition(0, rig.mainSkin.bones[bones[i * 2]].position);
                    linee.SetPosition(1, rig.mainSkin.bones[bones[i * 2 + 1]].position);
                }
            }
        }
        public static void DisableBoneESP()
        {
            foreach (var line in boneEspPool.SelectMany(bones => bones.Value))
                Object.Destroy(line);
            boneEspPool.Clear();
        }

        public static void SelfBoneESP()
        {
            var local = GorillaTagger.Instance.offlineVRRig;
            if (local == null || local.mainSkin == null) return;
            if (!selfBoneEspPool.TryGetValue(local, out List<LineRenderer> lines) || lines == null || lines.Count == 0 || lines[0] == null)
            {
                lines = new List<LineRenderer>();
                var headGO = local.head.rigTarget.gameObject;
                var head = headGO.GetComponent<LineRenderer>() ?? headGO.AddComponent<LineRenderer>();
                head.sharedMaterial = LimbChamMat;
                head.positionCount = 2;
                head.startWidth = 0.02f;
                head.endWidth = 0.02f;
                lines.Add(head);
                for (int i = 0; i < 19; i++)
                {
                    var boneGO = local.mainSkin.bones[bones[i * 2]].gameObject;
                    var line = boneGO.GetComponent<LineRenderer>() ?? boneGO.AddComponent<LineRenderer>();
                    line.sharedMaterial = LimbChamMat;
                    line.positionCount = 2;
                    line.startWidth = 0.02f;
                    line.endWidth = 0.02f;
                    lines.Add(line);
                }
                selfBoneEspPool[local] = lines;
            }
            Color boneColor = followmenutheme ? backgroundColor.GetCurrentColor() : local.playerColor;
            boneColor.a = 0.7f;
            LineRenderer l0 = lines[0];
            l0.startColor = boneColor;
            l0.endColor = boneColor;
            l0.SetPosition(0, local.headMesh.transform.position);
            l0.SetPosition(1, local.headMesh.transform.position);
            for (int i = 0; i < 19; i++)
            {
                var linee = lines[i + 1];
                if (linee == null) continue;
                linee.startColor = boneColor;
                linee.endColor = boneColor;
                linee.SetPosition(0, local.mainSkin.bones[bones[i * 2]].position);
                linee.SetPosition(1, local.mainSkin.bones[bones[i * 2 + 1]].position);
            }
        }
        public static void DisableSelfBoneESP()
        {
            foreach (var line in selfBoneEspPool.SelectMany(bones => bones.Value))
                Object.Destroy(line);
            selfBoneEspPool.Clear();
        }


        private static GameObject CreateObject(Transform parent, PrimitiveType type, Vector3 scale, Color color, Shader shader)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = scale;
            var col = obj.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            var rend = obj.GetComponent<Renderer>();
            rend.material = new Material(shader) { color = color };
            return obj;
        }

        private sealed class BoxState { public GameObject obj; public Renderer renderer; }
        private static readonly Dictionary<VRRig, BoxState> boxEspPool = new Dictionary<VRRig, BoxState>();

        public static void BoxESP()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Camera cam = Camera.main;
            if (cam == null) return;
            List<VRRig> remove = null;
            foreach (var pair in boxEspPool)
            {
                if (pair.Key == null || !VRRigCache.ActiveRigs.Contains(pair.Key))
                {
                    remove ??= new List<VRRig>();
                    remove.Add(pair.Key);
                    if (pair.Value.obj != null) Object.Destroy(pair.Value.obj);
                }
            }
            if (remove != null) foreach (var r in remove) boxEspPool.Remove(r);
            Vector3 camPos = cam.transform.position;
            Quaternion camRot = cam.transform.rotation;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!boxEspPool.TryGetValue(rig, out var st) || st.obj == null)
                {
                    var obj = CreateObject(rig.transform, PrimitiveType.Cube, new Vector3(0.5f, 0.5f, 0.1f), followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor, EspText);
                    st = new BoxState { obj = obj, renderer = obj.GetComponent<Renderer>() };
                    boxEspPool[rig] = st;
                }
                Color color = followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor;
                color.a = 0.4f;
                st.renderer.material.color = color;
                float sqrDist = (camPos - rig.transform.position).sqrMagnitude;
                float scale = Mathf.Clamp(Mathf.Sqrt(sqrDist) * 0.05f, 0.25f, 3f);
                st.obj.transform.localScale = new Vector3(scale, scale, 0.02f);
                st.obj.transform.rotation = camRot;
            }
        }

        public static void DisableBoxESP()
        {
            foreach (var st in boxEspPool.Values)
                if (st.obj != null) Object.Destroy(st.obj);
            boxEspPool.Clear();
        }

        private sealed class HollowState { public GameObject obj; public TextMesh text; }
        private static readonly Dictionary<VRRig, HollowState> hollowBoxPool = new Dictionary<VRRig, HollowState>();

        public static void HollowBoxESP()
        {
            if (!NetworkSystem.Instance.InRoom) { DisableHollowBoxESP(); return; }
            Camera cam = Camera.main;
            if (cam == null) return;
            List<VRRig> remove = null;
            foreach (var pair in hollowBoxPool)
            {
                if (pair.Key == null || !VRRigCache.ActiveRigs.Contains(pair.Key))
                {
                    remove ??= new List<VRRig>();
                    remove.Add(pair.Key);
                    if (pair.Value.obj != null) Object.Destroy(pair.Value.obj);
                }
            }
            if (remove != null) foreach (var r in remove) hollowBoxPool.Remove(r);
            Quaternion camRot = cam.transform.rotation;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!hollowBoxPool.TryGetValue(rig, out var st) || st.obj == null)
                {
                    var obj = new GameObject("HollowBoxESP");
                    var tm = obj.AddComponent<TextMesh>();
                    tm.alignment = TextAlignment.Center;
                    tm.anchor = TextAnchor.MiddleCenter;
                    tm.text = "□";
                    tm.fontSize = 300;
                    tm.characterSize = 0.05f;
                    st = new HollowState { obj = obj, text = tm };
                    hollowBoxPool[rig] = st;
                }
                st.obj.transform.localScale = Vector3.one * rig.scaleFactor;
                Color color = followmenutheme ? backgroundColor.GetCurrentColor() : rig.IsTagged() ? Color.red : rig.playerColor;
                color.a = 0.7f;
                st.text.color = color;
                st.obj.transform.position = rig.transform.position;
                st.obj.transform.LookAt(st.obj.transform.position + camRot * Vector3.forward, camRot * Vector3.up);
            }
        }

        public static void DisableHollowBoxESP()
        {
            foreach (var st in hollowBoxPool.Values)
                if (st.obj != null) Object.Destroy(st.obj);
            hollowBoxPool.Clear();
        }

        private static Dictionary<VRRig, TextMeshPro> nameTagPool = new Dictionary<VRRig, TextMeshPro>();
        public static void NameTags()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            foreach (var rig in nameTagPool.Keys.Where(r => r == null || !VRRigCache.ActiveRigs.Contains(r)).ToList())
            {
                RemoveTag(rig, TagKind.Name);
                nameTagPool.Remove(rig);
            }
            Camera cam = Camera.main;
            if (cam == null) return;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                string nick = rig.Creator.NickName.CleanString();
                if (!nameTagPool.TryGetValue(rig, out var tag) || tag == null)
                {
                    tag = CreateText(followheadmesh ? rig.headMesh.transform : rig.transform, TextAlignmentOptions.Center, nick,
                        followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor, 0.7f);
                    nameTagPool[rig] = tag;
                }
                if (tag.text != nick) tag.text = nick;
                Transform parent = followheadmesh ? rig.headMesh.transform : rig.transform;
                SetTag(rig, TagKind.Name, tag, parent, followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor);
                Vector3 dir = tag.transform.position - cam.transform.position;
                tag.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }
        public static void DisableNameTags()
        {
            foreach (var pair in nameTagPool)
                RemoveTag(pair.Key, TagKind.Name);
            nameTagPool.Clear();
        }


        private static Dictionary<VRRig, TextMeshPro> IDnameTagPool = new Dictionary<VRRig, TextMeshPro>();
        public static void IdNameTags()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            foreach (var rig in IDnameTagPool.Keys.Where(r => r == null || !VRRigCache.ActiveRigs.Contains(r)).ToList())
            {
                RemoveTag(rig, TagKind.Id);
                IDnameTagPool.Remove(rig);
            }
            Camera cam = Camera.main;
            if (cam == null) return;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!IDnameTagPool.TryGetValue(rig, out var tag) || tag == null)
                {
                    tag = CreateText(followheadmesh ? rig.headMesh.transform : rig.transform,
                        TextAlignmentOptions.Center, rig.Creator.UserId,
                        followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor, 0.7f);
                    IDnameTagPool[rig] = tag;
                }
                Transform parent = followheadmesh ? rig.headMesh.transform : rig.transform;
                SetTag(rig, TagKind.Id, tag, parent, followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor);
                Vector3 dir = tag.transform.position - cam.transform.position;
                tag.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }
        public static void DisableIdNameTags()
        {
            foreach (var pair in IDnameTagPool)
                RemoveTag(pair.Key, TagKind.Id);
            IDnameTagPool.Clear();
        }

        private static Dictionary<VRRig, TextMeshPro> PlatformnameTagPool = new Dictionary<VRRig, TextMeshPro>();
        public static void PlatformNameTags()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            foreach (var rig in PlatformnameTagPool.Keys.Where(r => r == null || !VRRigCache.ActiveRigs.Contains(r)).ToList())
            {
                RemoveTag(rig, TagKind.Platform);
                PlatformnameTagPool.Remove(rig);
            }
            Camera cam = Camera.main;
            if (cam == null) return;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!PlatformnameTagPool.TryGetValue(rig, out var tag) || tag == null)
                {
                    string platform = "Quest";
                    var pp = rig.GetPhotonPlayer();
                    if (pp != null && pp.CustomProperties.TryGetValue("platform", out var platObj) && platObj != null && platObj.ToString().Contains("Steam"))
                    {
                        platform = rig.Cosmetics().Contains("LMAKT.") ? "Quest" : "Steam";
                    }
                    tag = CreateText(followheadmesh ? rig.headMesh.transform : rig.transform,
                        TextAlignmentOptions.Center, platform,
                        followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor, 0.7f);
                    PlatformnameTagPool[rig] = tag;
                }
                Transform parent = followheadmesh ? rig.headMesh.transform : rig.transform;
                SetTag(rig, TagKind.Platform, tag, parent, followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor);
                Vector3 dir = tag.transform.position - cam.transform.position;
                tag.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }
        public static void DisablePlatformNameTags()
        {
            foreach (var pair in PlatformnameTagPool)
                RemoveTag(pair.Key, TagKind.Platform);
            PlatformnameTagPool.Clear();
        }

        private static Dictionary<VRRig, TextMeshPro> FpsnametagPool = new Dictionary<VRRig, TextMeshPro>();
        public static void FpsNameTags()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            foreach (var rig in FpsnametagPool.Keys.Where(r => r == null || !VRRigCache.ActiveRigs.Contains(r)).ToList())
            {
                RemoveTag(rig, TagKind.Fps);
                FpsnametagPool.Remove(rig);
            }
            Camera cam = Camera.main;
            if (cam == null) return;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!FpsnametagPool.TryGetValue(rig, out var tag) || tag == null)
                {
                    tag = CreateText(followheadmesh ? rig.headMesh.transform : rig.transform,
                        TextAlignmentOptions.Center, "", followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor, 0.7f);
                    FpsnametagPool[rig] = tag;
                }
                int fps = rig.fps;
                if (fps > 100) tag.text = $"<color=#00ffff>{fps}</color>";
                else if (fps > 60) tag.text = $"<color=green>{fps}</color>";
                else if (fps > 30) tag.text = $"<color=yellow>{fps}</color>";
                else tag.text = $"<color=red>{fps}</color>";
                Transform parent = followheadmesh ? rig.headMesh.transform : rig.transform;
                SetTag(rig, TagKind.Fps, tag, parent, followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor);
                Vector3 dir = tag.transform.position - cam.transform.position;
                tag.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }

        public static void DisableFpsNameTags()
        {
            foreach (var pair in FpsnametagPool)
                RemoveTag(pair.Key, TagKind.Fps);
            FpsnametagPool.Clear();
        }

        private static Dictionary<VRRig, TextMeshPro> TaggednametagPool = new Dictionary<VRRig, TextMeshPro>();
        public static void TaggedNameTags()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            foreach (var rig in TaggednametagPool.Keys.Where(r => r == null || !VRRigCache.ActiveRigs.Contains(r)).ToList())
            {
                RemoveTag(rig, TagKind.Tagged);
                TaggednametagPool.Remove(rig);
            }
            var gm = GorillaGameManager.instance;
            bool casual = gm == null || gm.GameModeName() == "CASUAL";
            Camera cam = Camera.main;
            if (cam == null) return;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!TaggednametagPool.TryGetValue(rig, out var tag) || tag == null)
                {
                    tag = CreateText(followheadmesh ? rig.headMesh.transform : rig.transform,
                        TextAlignmentOptions.Center, "Not Tagged",
                        followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor, 0.7f);
                    TaggednametagPool[rig] = tag;
                }
                string wanted = casual ? "Not Tagged" : (rig.IsTagged() ? "Tagged" : "Not Tagged");
                if (tag.text != wanted) tag.text = wanted;
                Transform parent = followheadmesh ? rig.headMesh.transform : rig.transform;
                SetTag(rig, TagKind.Tagged, tag, parent, followmenutheme ? backgroundColor.GetCurrentColor() : rig.playerColor);
                Vector3 dir = tag.transform.position - cam.transform.position;
                tag.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }
        public static void DisableTaggedNameTags()
        {
            foreach (var pair in TaggednametagPool)
                RemoveTag(pair.Key, TagKind.Tagged);
            TaggednametagPool.Clear();
        }

        private static TextMeshPro CreateText(Transform parent, TextAlignmentOptions alignment, string text, Color color, float scale)
        {
            GameObject textHolder = new GameObject("nametag_arrakis");
            TextMeshPro textmesh = textHolder.GetOrAddComponent<TextMeshPro>();
            textmesh.text = text;
            textmesh.color = color;
            textmesh.alignment = alignment;
            textmesh.fontSize = scale;
            textmesh.transform.SetParent(parent, false);
            return textmesh;
        }

        static readonly List<GameObject> allLeaves = new List<GameObject>();
        public static void NoLeaves()
        {
            GameObject Forest = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest");
            if (Forest != null)
            {
                for (int i = 0; i < Forest.transform.childCount; i++)
                {
                    GameObject v = Forest.transform.GetChild(i).gameObject;
                    if (v.name.Contains(LeavesName))
                    {
                        v.SetActive(false);
                        allLeaves.Add(v);
                    }
                }
            }
        }
        public static void DisableNoLeaves()
        {
            foreach (GameObject l in allLeaves)
                l.SetActive(true);
            allLeaves.Clear();
        }

        public static string _leavesName;
        public static string LeavesName
        {
            get
            {
                if (_leavesName == null)
                {
                    var forest = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest");
                    _leavesName = forest.GetComponentsInChildren<Transform>(true)
                    .Where(t => t.name.StartsWith("UnityTempFile") && t.parent != null && t.parent == forest.transform)
                    .GroupBy(t => t.name).Where(g => g.Count() == 3)
                    .OrderByDescending(g => g.First().GetSiblingIndex()).FirstOrDefault()?.Key ?? "UnityTempFile";
                }
                return _leavesName;
            }
        }

        public static void ClearWeather()
        {
            for (int i = 0; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
                BetterDayNightManager.instance.weatherCycle[i] = BetterDayNightManager.WeatherType.None;
        }

        public static void Snowfall(bool toggle) =>
            GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Environment/WeatherDayNight/snow/").SetActive(toggle);
        public static void Rain(bool toggle) =>
            GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Environment/WeatherDayNight/rain/").SetActive(toggle);


        private static GameObject arraylistHub; // Did you know that ccmv2 was actually the best cheat menu -nova
        private static Text arraylistText; // Did you know that ccmv2 was actually the best cheat menu -nova
        private static Material mat = new Material(EspShader);
        public static (GameObject, Text) CreateTextGUI(string text, string name, TextAnchor alignment, Vector3 loctrans) // Credits colossus
        {
            GameObject HUDObj = new GameObject(name);
            Canvas canvas = HUDObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            HUDObj.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 20f;
            HUDObj.AddComponent<GraphicRaycaster>();
            RectTransform rectTransform = HUDObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(5, 5);
            HUDObj.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            GameObject menuTextObj = new GameObject();
            menuTextObj.transform.SetParent(HUDObj.transform);
            Text MenuText = menuTextObj.AddComponent<Text>();
            MenuText.text = text;
            MenuText.fontSize = 10;
            MenuText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            MenuText.rectTransform.sizeDelta = new Vector2(260, 180);
            MenuText.rectTransform.localScale = new Vector3(0.01f, 0.01f, 1f);
            MenuText.rectTransform.localPosition = loctrans;
            MenuText.material = mat;
            MenuText.alignment = alignment;
            HUDObj.transform.SetParent(Camera.main.transform);
            HUDObj.transform.position = Camera.main.transform.position;
            HUDObj.transform.rotation = Camera.main.transform.rotation;
            return (HUDObj, MenuText);
        }
        public static void Arraylist()
        {
            if (arraylistHub == null)
            {
                (arraylistHub, arraylistText) = CreateTextGUI("", "ArraylistHud", TextAnchor.UpperLeft, new Vector3(0f, 0.2f, 3.6f));
            }
            if (arraylistText != null)
                arraylistText.alignment = fliparraylist ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            StringBuilder sb = new StringBuilder();

            foreach (ButtonInfo[] category in Buttons.buttons)
            {
                foreach (ButtonInfo button in category)
                {
                    if (button != null && button.enabled && button.ShowInArraylist)
                        sb.AppendLine(button.buttonText);
                }
            }
            arraylistText.text = sb.ToString().TrimEnd();
        }
        public static void DisableArraylist()
        {
            GameObject.Destroy(arraylistHub);
            arraylistHub = null;
            GameObject.Destroy(arraylistText);
            arraylistText = null;
        }
        public static GameObject ACam;
        public static void SpectateGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;
                RaycastHit Ray = GunData.Ray;
                if (lockTarget != null && gunLocked)
                {
                    if (ACam == null)
                    {
                        ACam = new GameObject("Arrakis Camera");
                        var c = ACam.AddComponent<Camera>();
                        c.fieldOfView = 120;
                        c.depth = 4;
                        c.nearClipPlane = 0.1f;
                        c.cameraType = CameraType.Game;
                        ACam.transform.position = GorillaTagger.Instance.offlineVRRig.headConstraint.transform.position;
                        Object.DontDestroyOnLoad(ACam);
                    }
                    ACam.transform.rotation = lockTarget.head.rigTarget.rotation;
                    ACam.transform.position = lockTarget.head.rigTarget.position;
                }
                if (GetGunInput(true))
                {
                    VRRig rig = Ray.collider.GetComponentInParent<VRRig>();
                    if (!rig.IsLocal())
                    {
                        lockTarget = rig;
                        gunLocked = true;
                    }
                }
            }
            else
            {
                lockTarget = null;
                gunLocked = false;
                Object.Destroy(ACam);
            }
        }
        private static Vector3 CameraVelocity;
        public static void Freecam()
        {
            if (InputManager.GetInput(InputManager.InputType.Trigger, InputManager.Hand.Left, !XRSettings.isDeviceActive))
            {
                if (ACam == null)
                {
                    ACam = new GameObject("Arrakis Camera");
                    ACam.transform.position = GorillaTagger.Instance.headCollider.transform.position;
                }
                Camera FreeCamera = ACam.GetOrAddComponent<Camera>();
                FreeCamera.nearClipPlane = 0.01f;
                FreeCamera.cameraType = CameraType.Game;
                Vector3 inputDirection = new Vector3(InputManager.GetAxis(Hand.Left).x, InputManager.GetAxis(Hand.Right).y, InputManager.GetAxis(Hand.Left).y);
                Vector3 playerForward = GTPlayer.Instance.bodyCollider.transform.forward.X_Z();
                Vector3 playerRight = GTPlayer.Instance.bodyCollider.transform.right.X_Z();
                Vector3 velocity = inputDirection.x * playerRight + inputDirection.y * Vector3.up + inputDirection.z * playerForward;
                velocity *= Settings.flyspeed;
                CameraVelocity = Vector3.Lerp(CameraVelocity, velocity, 0.12875f);
                ACam.transform.position += CameraVelocity * Time.unscaledDeltaTime;
                ACam.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
            }
            else
            {
                FixCamera();
            }
        }
        public static void FixCamera()
        {
            if (ACam != null)
            {
                Object.Destroy(ACam.GetComponent<Camera>());
                Object.Destroy(ACam);
                ACam = null;
            }
        }
        public static float EmissionSpeed = 1f;
        public static TrailRenderer leftTrail;
        public static TrailRenderer rightTrail;
        public static Vector3 lastLeftPosition;
        public static Vector3 lastRightPosition;
        public static void EnableHandTrails()
        {
            Transform leftHand = GorillaTagger.Instance.leftHandTransform;
            Transform rightHand = GorillaTagger.Instance.rightHandTransform;
            if (leftHand != null && leftTrail == null)
            {
                leftTrail = CreateHandTrail(leftHand);
            }
            if (rightHand != null && rightTrail == null)
            {
                rightTrail = CreateHandTrail(rightHand);
            }
            lastLeftPosition = leftHand != null ? leftHand.position : Vector3.zero;
            lastRightPosition = rightHand != null ? rightHand.position : Vector3.zero;
        }

        private static Shader _espShader;
        public static Shader EspShader
        {
            get
            {
                if (_espShader != null) return _espShader;
                _espShader = Shader.Find("GUI/Text Shader") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
                return _espShader;
            }
        }

        public static Material CreateTransparentMaterial(Color color)
        {
            Material material = new Material(EspShader)
            {
                color = color
            };
            material.SetInt("_ZTest", 8);
            material.SetFloat("_ZWrite", 0f);
            material.renderQueue = 4000;
            return material;
        }

        public static Color trailColor1 = Color.white; // Add setting for this later -nova
        public static Color trailColor2 = Color.orange; // Add setting for this later -nova
        public static Color trailColor3 = new Color(1f, 0.3f, 0f); // Add setting for this later -nova
        private static TrailRenderer CreateHandTrail(Transform parent)
        {
            GameObject trailObject = new GameObject("Arrakis_HandTrail");
            trailObject.transform.SetParent(parent, false);
            trailObject.transform.localPosition = Vector3.zero;
            TrailRenderer trail = trailObject.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.05f;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.01f;
            Gradient gradient = new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(trailColor1, 0f),
                    new GradientColorKey(trailColor2, 0.5f),
                    new GradientColorKey(trailColor3, 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.5f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            };
            trail.colorGradient = gradient;
            ((Renderer)trail).material = CreateTransparentMaterial(Color.white);
            trail.emitting = false;
            return trail;
        }
        public static void HandTrails()
        {
            Transform leftHand = GorillaTagger.Instance.leftHandTransform;
            Transform rightHand = GorillaTagger.Instance.rightHandTransform;
            float frameTime = Mathf.Max(Time.deltaTime, 0.0001f);
            if (leftHand != null && leftTrail != null)
            {
                leftTrail.emitting = (leftHand.position - lastLeftPosition).magnitude / frameTime > EmissionSpeed;
                lastLeftPosition = leftHand.position;
            }
            if (rightHand != null && rightTrail != null)
            {
                rightTrail.emitting = (rightHand.position - lastRightPosition).magnitude / frameTime > EmissionSpeed;
                lastRightPosition = rightHand.position;
		    }
        }
        public static void DestroyHandTrails()
        {
            DestroyHandTrail(leftTrail);
            DestroyHandTrail(rightTrail);
        }

        public static void DestroyHandTrail(TrailRenderer trail)
        {
            if (trail == null)
            {
                return;
            }
            Object.Destroy(trail.gameObject);
            trail = null;
        }
        private static GameObject hudInstance; // pasted RIGHT from my plugin so code might be ass -sleepy
        public static void PlayerInfo()
        {
            if (hudInstance == null)
            {
                hudInstance = new GameObject("Arrakis_InfoHud"); // only changed this name bcz before it was the fslur 25 times :3 -sleepy
                hudInstance.AddComponent<PlayerInfoBehavior>();
            }
        }
        public static void CleanupPlayerInfo() // pasted RIGHT from my plugin so code might be ass -sleepy
        {
            if (hudInstance != null)
            {
                UnityEngine.Object.Destroy(hudInstance);
                hudInstance = null;
            }
        }

        public static void EntityESP(ThrowableBug.BugName bugType)
        {
            ThrowableBug[] bugs = GetThrowableBugs();
            if (bugs == null)
                return;
            foreach (ThrowableBug entity in bugs)
            {
                if (entity == null || entity.bugName != bugType)
                    continue;
                SkinnedMeshRenderer rend = entity.GetComponentInChildren<SkinnedMeshRenderer>();
                if (rend == null)
                    continue;
                rend.material.shader = EspText;
                rend.material.color = new Color(0.6f, 0.6f, 0f, 0.6f);
            }
        }

        public static void DisableEntityESP(ThrowableBug.BugName bugType)
        {
            ThrowableBug[] bugs = GetThrowableBugs();
            if (bugs == null)
                return;
            foreach (ThrowableBug entity in bugs)
            {
                if (entity == null || entity.bugName != bugType)
                    continue;
                SkinnedMeshRenderer rend = entity.GetComponentInChildren<SkinnedMeshRenderer>();
                if (rend == null)
                    continue;
                rend.material.shader = UberShader;
            }
        }

        private static List<ThrowableBug> cachedBugs = new List<ThrowableBug>();
        private static bool done = false;
        private static ThrowableBug[] GetThrowableBugs()
        {
            if (done)
                return cachedBugs.ToArray();
            done = true;
            foreach (ThrowableBug entity in GameObject.FindObjectsByType<ThrowableBug>(FindObjectsSortMode.None))
            {
                if (entity != null && !cachedBugs.Contains(entity))
                    cachedBugs.Add(entity);
            }
            return cachedBugs.ToArray();
        }

        public static void ChamRig(VRRig rig, bool enable, Color color)
        {
            if (rig == null || rig.mainSkin == null) return;
            rig.mainSkin.material.shader = enable ? EspText : UberShader;
            rig.mainSkin.material.color = enable ? color : rig.playerColor;
        }

        public static void EnableFog() =>
            GorillaTag.Rendering.ZoneShaderSettings.activeInstance.SetGroundFogValue(new Color(0.9569f, 0.6941f, 0.502f, 0.1216f), 40f, 10f, 40f); // pulled from halloween upd lel

        public static void DisableFog() =>
            GorillaTag.Rendering.ZoneShaderSettings.activeInstance.SetGroundFogValue(Color.clear, 0f, 0f, 0f);

        private sealed class RingState { public GameObject obj; public LineRenderer line; }
        private static readonly Dictionary<VRRig, RingState> ringPool = new Dictionary<VRRig, RingState>();
        public static void Ring()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> remove = null;
            foreach (var pair in ringPool)
            {
                if (pair.Key == null || !VRRigCache.ActiveRigs.Contains(pair.Key))
                {
                    remove ??= new List<VRRig>();
                    remove.Add(pair.Key);
                    if (pair.Value.obj != null) Object.Destroy(pair.Value.obj);
                }
            }
            if (remove != null)
                foreach (var r in remove) ringPool.Remove(r);
            float time = Time.time;
            float pulse = Mathf.Sin(time * 3.5f) * 0.035f;
            float radius = 0.38f + pulse;
            float bob = Mathf.Sin(time * 2.5f) * 0.025f;
            float rot = time * 70f;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!ringPool.TryGetValue(rig, out var st) || st.obj == null)
                {
                    var obj = new GameObject("Arrakis_Ring");
                    obj.transform.SetParent(rig.transform, false);
                    var line = obj.AddComponent<LineRenderer>();
                    line.useWorldSpace = false;
                    line.loop = true;
                    line.positionCount = 48;
                    line.startWidth = 0.012f;
                    line.endWidth = 0.012f;
                    line.sharedMaterial = LimbChamMat;
                    for (int i = 0; i < 48; i++)
                    {
                        float a = i * Mathf.PI * 2f / 48f;
                        line.SetPosition(i, new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)));
                    }
                    st = new RingState { obj = obj, line = line };
                    ringPool[rig] = st;
                }
                st.obj.transform.localPosition = new Vector3(0f, -0.35f + bob, 0f);
                st.obj.transform.localRotation = Quaternion.Euler(0f, rot, 0f);
                st.obj.transform.localScale = new Vector3(radius, 1f, radius);
                Color color = followmenutheme ? backgroundColor.GetCurrentColor() : rig.IsTagged() ? new Color(0.6f, 0f, 0f) : rig.playerColor;
                color.a = 0.7f;
                st.line.startColor = color;
                st.line.endColor = color;
            }
        }

        public static void DisableRing()
        {
            foreach (var st in ringPool.Values)
                if (st.obj != null) Object.Destroy(st.obj);
            ringPool.Clear();
        }


        private sealed class HaloState { public GameObject obj; public Renderer renderer; }
        private static readonly Dictionary<VRRig, HaloState> haloPool = new Dictionary<VRRig, HaloState>();

        public static void Halo()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> remove = null;
            foreach (var pair in haloPool)
            {
                if (pair.Key == null || !VRRigCache.ActiveRigs.Contains(pair.Key))
                {
                    remove ??= new List<VRRig>();
                    remove.Add(pair.Key);
                    if (pair.Value.obj != null) Object.Destroy(pair.Value.obj);
                }
            }
            if (remove != null)
                foreach (var r in remove) haloPool.Remove(r);
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!haloPool.TryGetValue(rig, out var st) || st.obj == null)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    obj.name = "Arrakis_Halo";
                    obj.transform.SetParent(rig.transform, false);
                    obj.transform.localPosition = new Vector3(0f, 0.25f, 0f);
                    obj.transform.localScale = new Vector3(0.85f, 0.003f, 0.85f);
                    var col = obj.GetComponent<Collider>();
                    if (col != null) Object.Destroy(col);
                    var rend = obj.GetComponent<Renderer>();
                    rend.material.shader = EspText;
                    st = new HaloState { obj = obj, renderer = rend };
                    haloPool[rig] = st;
                }
                Color color = followmenutheme ? backgroundColor.GetCurrentColor() : rig.IsTagged() ? new Color(0.6f, 0f, 0f) : rig.playerColor;
                color.a = 0.18f;
                st.renderer.material.color = color;
            }
        }

        public static void DisableHalo()
        {
            foreach (var st in haloPool.Values)
                if (st.obj != null) Object.Destroy(st.obj);
            haloPool.Clear();
        }


        private sealed class WireframeState { public GameObject obj; public WireframeRenderer renderer; }
        private static readonly Dictionary<VRRig, WireframeState> wireframeEspPool = new Dictionary<VRRig, WireframeState>();
        private static readonly Dictionary<VRRig, Color> originalRigColors = new Dictionary<VRRig, Color>();
        public static void WireframeESP()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> remove = null;
            foreach (var pair in wireframeEspPool)
            {
                if (pair.Key == null || !VRRigCache.ActiveRigs.Contains(pair.Key))
                {
                    remove ??= new List<VRRig>();
                    remove.Add(pair.Key);
                    if (pair.Value != null && pair.Value.obj != null) Object.Destroy(pair.Value.obj);
                }
            }
            if (remove != null)
            {
                foreach (var rig in remove)
                {
                    if (originalRigColors.TryGetValue(rig, out Color c) && rig != null && rig.mainSkin != null)
                    {
                        rig.mainSkin.material.shader = UberShader;
                        rig.mainSkin.material.color = c;
                    }
                    wireframeEspPool.Remove(rig);
                    originalRigColors.Remove(rig);
                }
            }
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!wireframeEspPool.TryGetValue(rig, out var st) || st.obj == null)
                {
                    var obj = new GameObject("Arrakis_WireframeESP");
                    Object.DontDestroyOnLoad(obj);
                    var wr = obj.AddComponent<WireframeRenderer>();
                    wr.targetRig = rig;
                    wr.wireColor = followmenutheme ? backgroundColor.GetCurrentColor() : rig.IsTagged() ? new Color(0.6f, 0f, 0f, 0.85f) : rig.playerColor;
                    originalRigColors[rig] = rig.mainSkin.material.color;
                    rig.mainSkin.material.shader = EspText;
                    rig.mainSkin.material.color = new Color(0f, 0f, 0f, 0f);
                    st = new WireframeState { obj = obj, renderer = wr };
                    wireframeEspPool[rig] = st;
                }
                Color c = followmenutheme ? backgroundColor.GetCurrentColor() : rig.IsTagged() ? new Color(0.6f, 0f, 0f, 0.85f) : rig.playerColor;
                st.renderer.wireColor = c;
            }
        }
        public static void DisableWireframeESP()
        {
            foreach (var pair in wireframeEspPool)
            {
                if (pair.Key != null && pair.Key.mainSkin != null && originalRigColors.TryGetValue(pair.Key, out Color c))
                {
                    pair.Key.mainSkin.material.shader = UberShader;
                    pair.Key.mainSkin.material.color = c;
                }
                if (pair.Value != null && pair.Value.obj != null) Object.Destroy(pair.Value.obj);
            }
            wireframeEspPool.Clear();
            originalRigColors.Clear();
        }

        private static GameObject selfWireframeCam = null;
        private static WireframeRenderer selfWireframeRenderer = null;
        private static Color originalSkinColor;
        private static bool skinHidden = false;
        public static void EnableSelfWireframeMesh()
        {
            VRRig rig = GorillaTagger.Instance.offlineVRRig;
            if (rig == null || rig.mainSkin == null) return;
            if (selfWireframeCam == null)
            {
                selfWireframeCam = new GameObject("Arrakis_WireframeCam");
                Object.DontDestroyOnLoad(selfWireframeCam);
                selfWireframeRenderer = selfWireframeCam.AddComponent<WireframeRenderer>();
                selfWireframeRenderer.targetRig = rig;
                selfWireframeRenderer.wireColor = backgroundColor.GetCurrentColor();
                originalSkinColor = rig.mainSkin.material.color;
                rig.mainSkin.enabled = false;
                skinHidden = true;
            }
        }

        public static void DisableSelfWireframeMesh()
        {
            if (selfWireframeCam != null)
            {
                VRRig rig = GorillaTagger.Instance.offlineVRRig;
                if (rig != null && skinHidden)
                {
                    rig.mainSkin.enabled = true;
                    rig.mainSkin.material.shader = UberShader;
                    rig.mainSkin.material.color = originalSkinColor;
                    skinHidden = false;
                }
                Object.Destroy(selfWireframeCam);
                selfWireframeCam = null;
                selfWireframeRenderer = null;
            }
        }

        private static Color _lastSelfWireColor;

        public static void UpdateSelfWireframeColor()
        {
            if (selfWireframeRenderer == null) return;
            Color c = backgroundColor.GetCurrentColor();
            if (c == _lastSelfWireColor) return;
            _lastSelfWireColor = c;
            selfWireframeRenderer.wireColor = c;
        }

        private sealed class BreadcrumbState { public TrailRenderer trail; public Renderer renderer; }
        private static readonly Dictionary<VRRig, BreadcrumbState> breakcujmpool = new Dictionary<VRRig, BreadcrumbState>();

        public static void Breadcrumbs()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> remove = null;
            foreach (var pair in breakcujmpool)
            {
                if (pair.Key == null || !VRRigCache.ActiveRigs.Contains(pair.Key))
                {
                    remove ??= new List<VRRig>();
                    remove.Add(pair.Key);
                    if (pair.Value.trail != null) Object.Destroy(pair.Value.trail);
                }
            }
            if (remove != null) foreach (var r in remove) breakcujmpool.Remove(r);
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.IsLocal()) continue;
                if (!breakcujmpool.TryGetValue(rig, out var st) || st.trail == null)
                {
                    var trail = rig.head.rigTarget.gameObject.GetOrAddComponent<TrailRenderer>();
                    trail.minVertexDistance = 0.05f;
                    trail.numCapVertices = 10;
                    trail.numCornerVertices = 5;
                    trail.startWidth = 0.02f;
                    trail.endWidth = 0.01f;
                    trail.time = 3f;
                    trail.name = "Arrakis_Breadcrumb";
                    var c = trail.GetComponent<Collider>();
                    if (c != null) Object.Destroy(c);
                    trail.sharedMaterial = LimbChamMat;
                    st = new BreadcrumbState { trail = trail, renderer = trail };
                    breakcujmpool[rig] = st;
                }
                Color color = followmenutheme ? backgroundColor.GetCurrentColor() : rig.IsTagged() ? new Color(0.6f, 0f, 0f) : rig.playerColor;
                color.a = 0.18f;
                st.renderer.material.color = color;
            }
        }
        public static void DisableBreadcrumbs()
        {
            foreach (var st in breakcujmpool.Values)
                if (st.trail != null) Object.Destroy(st.trail);
            breakcujmpool.Clear();
        }

        public static void fullBright(bool fullBright) =>
            GameLightingManager.instance.SetCustomDynamicLightingEnabled(fullBright);

        public static Vector3 Direction = new Vector3(1f, 0f, 0.3f).normalized;
        public static float Speed = 12f;
        public static float Density = 60f;
        public static float GustAmount = 0.5f;
        public static float GustSpeed = 0.4f;
        public static Color WindColor = new Color(1f, 1f, 1f, 0.35f);
        private static GameObject obj;
        private static ParticleSystem ps;
        private static readonly float seed = Random.value * 100f;
        public static void EnableWind()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            if (ps == null)
            {
                obj = new GameObject("WindVisual");
                Object.DontDestroyOnLoad(obj);
                ps = obj.AddComponent<ParticleSystem>();
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var rawr = ps.main;
                rawr.loop = true;
                rawr.simulationSpace = ParticleSystemSimulationSpace.World;
                rawr.startLifetime = 1.6f;
                rawr.startSize = 0.04f;
                rawr.startColor = WindColor;
                rawr.maxParticles = 500;
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(20f, 8f, 0.5f);

                var col = ps.colorOverLifetime;
                col.enabled = true;
                Gradient g = new Gradient();
                g.SetKeys(
                    new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f),
                            new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
                col.color = g;
                var rend = obj.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Stretch;
                rend.lengthScale = 6f;
                rend.velocityScale = 0.05f;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rend.receiveShadows = false;
                Shader sh = Shader.Find("Sprites/Default")
                    ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended")
                    ?? Shader.Find("Universal Render Pipeline/Unlit");
                rend.material = new Material(sh);
            }
            Vector3 dir = Direction.normalized;
            obj.transform.position = cam.transform.position - dir * 8f;
            obj.transform.rotation = Quaternion.LookRotation(dir);
            float noise = Mathf.PerlinNoise(Time.time * GustSpeed, seed);
            float gust = 1f + (noise - 0.5f) * 2f * GustAmount;
            var main = ps.main;
            main.startSpeed = Speed * gust;
            var emission = ps.emission;
            emission.rateOverTime = Density * gust;
            if (!ps.isEmitting) ps.Play();
        }
        public static void DisableWind()
        {
            if (ps != null && ps.isEmitting)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        public static void CleanPool(VRRig rig = null)
        {
            if (rig == null)
            {
                DisableLimbChams();
                DisableTracers();
                DisableBoneESP();
                DisableSelfBoneESP();
                DisableBoxESP();
                DisableHollowBoxESP();
                DisableNameTags();
                DisableIdNameTags();
                DisablePlatformNameTags();
                DisableFpsNameTags();
                DisableTaggedNameTags();
                DisableRing();
                DisableHalo();
                DisableWireframeESP();
                DisableBreadcrumbs();
                DisableRedact();
                tagStack.Clear();
            }
            else
            {
                if (limbChamPool.TryGetValue(rig, out var lc) && lc.holder != null) Object.Destroy(lc.holder);
                limbChamPool.Remove(rig);
                if (tracersPool.TryGetValue(rig, out var tr) && tr.holder != null) Object.Destroy(tr.holder);
                tracersPool.Remove(rig);
                if (boneEspPool.TryGetValue(rig, out var bl))
                    foreach (var l in bl) if (l != null) Object.Destroy(l);
                boneEspPool.Remove(rig);
                if (selfBoneEspPool.TryGetValue(rig, out var sbl))
                    foreach (var l in sbl) if (l != null) Object.Destroy(l);
                selfBoneEspPool.Remove(rig);
                if (boxEspPool.TryGetValue(rig, out var b) && b.obj != null) Object.Destroy(b.obj);
                boxEspPool.Remove(rig);
                if (hollowBoxPool.TryGetValue(rig, out var hb) && hb.obj != null) Object.Destroy(hb.obj);
                hollowBoxPool.Remove(rig);
                RemoveTag(rig, TagKind.Name); nameTagPool.Remove(rig);
                RemoveTag(rig, TagKind.Id); IDnameTagPool.Remove(rig);
                RemoveTag(rig, TagKind.Platform); PlatformnameTagPool.Remove(rig);
                RemoveTag(rig, TagKind.Fps); FpsnametagPool.Remove(rig);
                RemoveTag(rig, TagKind.Tagged); TaggednametagPool.Remove(rig);
                tagStack.Remove(rig);
                if (ringPool.TryGetValue(rig, out var r) && r.obj != null) Object.Destroy(r.obj);
                ringPool.Remove(rig);
                if (haloPool.TryGetValue(rig, out var h) && h.obj != null) Object.Destroy(h.obj);
                haloPool.Remove(rig);
                if (wireframeEspPool.TryGetValue(rig, out var w))
                {
                    if (originalRigColors.TryGetValue(rig, out Color c) && rig != null && rig.mainSkin != null)
                    {
                        rig.mainSkin.material.shader = UberShader;
                        rig.mainSkin.material.color = c;
                    }
                    if (w.obj != null) Object.Destroy(w.obj);
                }
                wireframeEspPool.Remove(rig);
                originalRigColors.Remove(rig);
                if (breakcujmpool.TryGetValue(rig, out var bc) && bc.trail != null) Object.Destroy(bc.trail);
                breakcujmpool.Remove(rig);
                if (redactPool.TryGetValue(rig, out var m) && m.root != null) Object.Destroy(m.root);
                redactPool.Remove(rig);
            }
        }

        public static void DisableRedact()
        {
            foreach (var pair in redactPool)
                if (pair.Value.root != null) GameObject.Destroy(pair.Value.root);
            redactPool.Clear();
        }

        public static void Redact()
        {
            List<VRRig> stale = null;
            foreach (var pair in redactPool)
            {
                if (pair.Key == null || !VRRigCache.ActiveRigs.Contains(pair.Key))
                {
                    stale ??= new List<VRRig>();
                    stale.Add(pair.Key);
                    if (pair.Value.root != null) GameObject.Destroy(pair.Value.root);
                }
            }
            if (stale != null)
                foreach (var r in stale) redactPool.Remove(r);
            float cellW = PlateW / GridX;
            float cellH = PlateH / GridY;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig == VRRig.LocalRig) continue;
                if (!redactPool.TryGetValue(rig, out Mosaic m))
                {
                    m = BuildMosaic();
                    redactPool[rig] = m;
                }
                Transform head = rig.headMesh.transform;
                Quaternion rot = head.rotation * Quaternion.Euler(-20f, 0f, 0f);
                Vector3 center = head.position + head.forward * 0.16f + head.up * 0.09f;
                Vector3 right = rot * Vector3.right;
                Vector3 up = rot * Vector3.up;
                int i = 0;
                for (int x = 0; x < GridX; x++)
                {
                    for (int y = 0; y < GridY; y++)
                    {
                        float ox = (x - (GridX - 1) / 2f) * cellW;
                        float oy = (y - (GridY - 1) / 2f) * cellH;
                        Transform c = m.cells[i++];
                        c.position = center + right * ox + up * oy;
                        c.rotation = rot;
                    }
                }
            }
        }

        private const int GridX = 8;
        private const int GridY = 8;
        private const float PlateW = 0.22f;
        private const float PlateH = 0.22f;
        private const float Gap = 1.02f;

        public static Dictionary<VRRig, Mosaic> redactPool = new Dictionary<VRRig, Mosaic>();
        private static Material _mosaicMat;
        private static Material MosaicMat
        {
            get
            {
                if (_mosaicMat != null) return _mosaicMat;
                _mosaicMat = new Material(EspText);
                _mosaicMat.SetInt("_ZTest", 4);
                return _mosaicMat;
            }
        }

        private static Mosaic BuildMosaic()
        {
            GameObject root = new GameObject("RedactMosaic");
            root.hideFlags = HideFlags.HideAndDontSave;
            Transform[] cells = new Transform[GridX * GridY];
            float cellW = PlateW / GridX;
            float cellH = PlateH / GridY;
            var mpb = new MaterialPropertyBlock();
            int i = 0;
            for (int x = 0; x < GridX; x++)
            {
                for (int y = 0; y < GridY; y++)
                {
                    GameObject px = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    var col = px.GetComponent<BoxCollider>();
                    if (col != null) Object.Destroy(col);
                    px.transform.SetParent(root.transform);
                    px.transform.localScale = new Vector3(cellW * Gap, cellH * Gap, 0.015f);
                    var mr = px.GetComponent<MeshRenderer>();
                    mr.sharedMaterial = MosaicMat;
                    float shade = 0.03f + Random.Range(0f, 0.15f);
                    mpb.SetColor("_Color", new Color(shade, shade, shade, 1f));
                    mr.SetPropertyBlock(mpb);
                    cells[i++] = px.transform;
                }
            }
            return new Mosaic { root = root, cells = cells };
        }
    }

    public class Mosaic
    {
        public GameObject root;
        public Transform[] cells;
    }

    public class WireframeRenderer : MonoBehaviour
    {
        public VRRig targetRig;
        public Color wireColor = Color.cyan;
        private Material wireMat;
        private SkinnedMeshRenderer skin;
        private Mesh baked;
        private (int, int)[] edgeCache;
        private bool edgesDirty = true;
        private int frameCounter;
        private const float MaxDrawDistance = 30f;

        private void OnDestroy()
        {
            if (baked != null) Destroy(baked);
            if (wireMat != null) Destroy(wireMat);
        }

        private void Awake()
        {
            wireMat = new Material(Visual.EspText);
            wireMat.SetInt("_ZTest", 8);
            wireMat.renderQueue = 4000;
            baked = new Mesh();
            baked.MarkDynamic();
        }

        private void LateUpdate()
        {
            if (targetRig == null) return;
            var current = targetRig.mainSkin;
            if (current == null) return;
            if (skin != current)
            {
                skin = current;
                edgesDirty = true;
            }
            int throttle = targetRig == VRRig.LocalRig ? 3 : 1;
            if ((frameCounter++ % (throttle + 1)) != 0) return;
            Vector3 headPos = targetRig.headMesh.transform.position;
            if (targetRig != VRRig.LocalRig)
            {
                Camera cam = Camera.main;
                if (cam == null) return;
                if ((headPos - cam.transform.position).sqrMagnitude > MaxDrawDistance * MaxDrawDistance) return;
                Vector3 vp = cam.WorldToViewportPoint(headPos);
                if (vp.z < 0f || vp.x < -0.15f || vp.x > 1.15f || vp.y < -0.15f || vp.y > 1.15f)
                    return;
            }
            skin.BakeMesh(baked);
            if (edgesDirty)
            {
                edgeCache = BuildEdges(baked);
                edgesDirty = false;
            }
        }

        private void OnRenderObject()
        {
            if (baked == null || edgeCache == null || wireMat == null) return;
            if (targetRig == null) return;

            Transform root = skin.transform;
            Vector3[] verts = baked.vertices;
            Color c = wireColor;
            c.a = 0.85f;
            wireMat.color = c;
            wireMat.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(root.localToWorldMatrix);
            GL.Begin(GL.LINES);
            GL.Color(c);
            int vlen = verts.Length;
            for (int i = 0; i < edgeCache.Length; i++)
            {
                var e = edgeCache[i];
                if (e.Item1 >= vlen || e.Item2 >= vlen) continue;
                GL.Vertex(verts[e.Item1]);
                GL.Vertex(verts[e.Item2]);
            }
            GL.End();
            GL.PopMatrix();
        }

        private static (int, int)[] BuildEdges(Mesh mesh)
        {
            int[] tris = mesh.triangles;
            var edgeSet = new HashSet<long>();
            var result = new List<(int, int)>();
            const int stride = 8;
            for (int i = 0; i < tris.Length; i += 3 * stride)
            {
                int a = tris[i], b = tris[i + 1], c = tris[i + 2];
                TryAddEdge(edgeSet, result, a, b);
                TryAddEdge(edgeSet, result, b, c);
                TryAddEdge(edgeSet, result, c, a);
            }
            return result.ToArray();
        }

        private static void TryAddEdge(HashSet<long> set, List<(int, int)> result, int a, int b)
        {
            int lo = a < b ? a : b;
            int hi = a < b ? b : a;
            long key = ((long)lo << 32) | (uint)hi;
            if (set.Add(key))
                result.Add((lo, hi));
        }
    }
}