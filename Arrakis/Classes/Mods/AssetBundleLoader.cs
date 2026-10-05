/*
 * Arrakis | Classes/Mods/AssetBundleLoader.cs
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
using System.IO;
using System.Reflection;
using Arrakis;
using UnityEngine;

public static class AssetBundleLoader
{
    private static readonly Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
    private static readonly Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
    private static readonly Dictionary<string, List<string>> bundleObjects = new Dictionary<string, List<string>>();
    public static GameObject LoadBundle(string bundleName, Vector3 pos, Quaternion rot, string id)
    {
        AssetBundle bundle = GetBundle(bundleName);
        if (bundle == null)
            return null;
        GameObject[] assets = bundle.LoadAllAssets<GameObject>();
        if (assets.Length == 0)
        {
            CustomConsole.Log("No prefab in bundle: " + bundleName, CustomConsole.LogType.Warning);
            return null;
        }
        GameObject obj = Object.Instantiate(assets[0], pos, rot);
        obj.name = id;
        objects[id] = obj;
        if (!bundleObjects.TryGetValue(bundleName, out List<string> ids))
        {
            ids = new List<string>();
            bundleObjects[bundleName] = ids;
        }
        ids.Add(id);
        return obj;
    }
    private static AssetBundle GetBundle(string name)
    {
        AssetBundle bundle;
        if (bundles.TryGetValue(name, out bundle))
            return bundle;
        string resourceName = "Arrakis.Resources.Bundles." + name;
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                CustomConsole.Log("Missing embedded bundle: " + resourceName, CustomConsole.LogType.Error);
                return null;
            }
            byte[] data = new byte[stream.Length];
            int offset = 0;
            int read;
            while (offset < data.Length && (read = stream.Read(data, offset, data.Length - offset)) > 0)
                offset += read;
            bundle = AssetBundle.LoadFromMemory(data);
        }
        if (bundle != null)
        {
            bundles.Add(name, bundle);
            CustomConsole.Log("Loaded bundle: " + resourceName, CustomConsole.LogType.Info);
        }
        return bundle;
    }
    public static bool MoveObject(string id, Vector3 pos, Quaternion rot)
    {
        GameObject obj;
        if (!objects.TryGetValue(id, out obj))
            return false;
        if (obj == null)
            return false;
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        return true;
    }
    public static bool DeleteBundle(string bundleName)
    {
        AssetBundle bundle;
        if (!bundles.TryGetValue(bundleName, out bundle))
            return false;
        if (bundleObjects.TryGetValue(bundleName, out List<string> ids))
        {
            foreach (string id in ids)
            {
                if (objects.TryGetValue(id, out GameObject obj))
                {
                    if (obj != null)
                        Object.Destroy(obj);
                    objects.Remove(id);
                }
            }
            bundleObjects.Remove(bundleName);
        }
        bundle.Unload(false);
        bundles.Remove(bundleName);
        return true;
    }
    public static GameObject GetObject(string id)
    {
        GameObject obj;
        objects.TryGetValue(id, out obj);
        return obj;
    }
    public static bool Exists(string id)
    {
        GameObject obj;
        return objects.TryGetValue(id, out obj)
            && obj != null;
    }
    public static bool RemoveObject(string id)
    {
        GameObject obj;
        if (!objects.TryGetValue(id, out obj))
            return false;
        if (obj != null)
            Object.Destroy(obj);
        objects.Remove(id);
        foreach (List<string> ids in bundleObjects.Values)
            ids.Remove(id);
        return true;
    }
    public static void ClearObjects()
    {
        foreach (GameObject obj in objects.Values)
        {
            if (obj != null)
                Object.Destroy(obj);
        }
        objects.Clear();
        foreach (List<string> ids in bundleObjects.Values)
            ids.Clear();
    }
    public static void UnloadBundles()
    {
        foreach (List<string> ids in bundleObjects.Values)
        {
            foreach (string id in ids)
            {
                if (objects.TryGetValue(id, out GameObject obj))
                {
                    if (obj != null)
                        Object.Destroy(obj);
                    objects.Remove(id);
                }
            }
        }
        bundleObjects.Clear();
        foreach (AssetBundle bundle in bundles.Values)
            bundle.Unload(false);
        bundles.Clear();
    }
    public static Texture2D LoadTexture(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string fullName = $"Arrakis.Resources.Images.{resourceName}";
        using Stream stream = assembly.GetManifestResourceStream(fullName);
        if (stream == null)
        {
            CustomConsole.Log($"Embedded resource not found: {fullName}", CustomConsole.LogType.Error);
            return null;
        }
        byte[] data = new byte[stream.Length];
        stream.Read(data, 0, data.Length);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(data))
        {
            Object.Destroy(texture);
            return null;
        }
        texture.name = resourceName;
        return texture;
    }
}