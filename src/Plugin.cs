using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace IceItemIcons
{
    [BepInPlugin("local.iceitemicons", "Ice Item Icons", "1.0.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private Harmony _harmony;
        internal static BepInEx.Logging.ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony("local.iceitemicons");
            _harmony.PatchAll(typeof(Plugin).Assembly);
            Log.LogInfo("Ice Item Icons loaded; only IceShoes and IceSkates icons are overridden.");
        }

        private void OnDestroy()
        {
            if (_harmony != null) _harmony.UnpatchSelf();
        }
    }

    [HarmonyPatch(typeof(ObjectDB), "Awake")]
    internal static class IconPatch
    {
        private sealed class IconDef
        {
            internal readonly string Prefab;
            internal readonly string Resource;
            internal IconDef(string prefab, string resource) { Prefab = prefab; Resource = resource; }
        }

        private static readonly IconDef[] Icons = {
            new IconDef("IceShoes", "IceItemIcons.Assets.IceSpikes.png"),
            new IconDef("IceSkates", "IceItemIcons.Assets.IceSkates.png")
        };
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        private static void Postfix(ObjectDB __instance)
        {
            if (__instance == null || __instance.m_items == null) return;
            foreach (GameObject prefab in __instance.m_items)
            {
                if (prefab == null) continue;
                IconDef def = Array.Find(Icons, x => string.Equals(x.Prefab, prefab.name, StringComparison.OrdinalIgnoreCase));
                if (def == null) continue;
                ItemDrop drop = prefab.GetComponent<ItemDrop>();
                if (drop == null || drop.m_itemData == null || drop.m_itemData.m_shared == null || drop.m_itemData.m_shared.m_icons == null || drop.m_itemData.m_shared.m_icons.Length == 0)
                {
                    Plugin.Log.LogWarning("No item icon slot found for " + def.Prefab + ".");
                    continue;
                }
                Sprite sprite = Load(def, drop.m_itemData.m_shared.m_icons[0]);
                if (sprite == null) continue;
                Sprite[] replacement = new Sprite[drop.m_itemData.m_shared.m_icons.Length];
                for (int i = 0; i < replacement.Length; i++) replacement[i] = sprite;
                drop.m_itemData.m_shared.m_icons = replacement;
                Plugin.Log.LogInfo("Applied icon to " + def.Prefab + ".");
            }
        }

        private static bool LoadPng(Texture2D texture, byte[] bytes)
        {
            Assembly assembly = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "UnityEngine.ImageConversionModule");
            if (assembly == null) { try { assembly = Assembly.Load("UnityEngine.ImageConversionModule"); } catch { return false; } }
            Type type = assembly.GetType("UnityEngine.ImageConversion");
            MethodInfo method = type == null ? null : type.GetMethod("LoadImage", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }, null);
            return method != null && (bool)method.Invoke(null, new object[] { texture, bytes, true });
        }        private static Sprite Load(IconDef def, Sprite source)
        {
            Sprite cached;
            if (Cache.TryGetValue(def.Prefab, out cached)) return cached;
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(def.Resource))
                {
                    if (stream == null) { Plugin.Log.LogWarning("Embedded icon missing: " + def.Resource); return null; }
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
                    if (!LoadPng(texture, bytes)) { UnityEngine.Object.Destroy(texture); return null; }
                    texture.name = "IceItemIcons_" + def.Prefab + "Texture";
                    texture.filterMode = FilterMode.Bilinear;
                    texture.wrapMode = TextureWrapMode.Clamp;
                    float ppu = source != null ? source.pixelsPerUnit : 100f;
                    Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), ppu);
                    sprite.name = "IceItemIcons_" + def.Prefab;
                    Cache[def.Prefab] = sprite;
                    return sprite;
                }
            }
            catch (Exception ex) { Plugin.Log.LogWarning("Could not load icon for " + def.Prefab + ": " + ex.Message); return null; }
        }
    }
}


