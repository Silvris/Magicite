using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.U2D;

namespace Magicite
{
    public static class AtlasManager
    {
        public static List<AtlasData> Atlases = new List<AtlasData>();
    }
    public class AtlasData
    {
        public Dictionary<string,SpriteData> Sprites = new Dictionary<string, SpriteData>();
        public string Name { get; set; }
        public string BasePath { get; set; }
        public AtlasData(string name, string basePath, Dictionary<string, SpriteData> sprites)
        {
            EntryPoint.Instance.Log.LogInfo("AtlasData.ctor");
            Name = name;
            BasePath = basePath;
            Sprites = sprites;
        }

        public Sprite GetSprite(string name)
        {
            EntryPoint.Instance.Log.LogInfo("AtlasData.GetSprite");
            if (Sprites.ContainsKey(name))
            {
                return ResourceGeneration.CreateSprite(ResourceGeneration.ReadTextureFromFile(BasePath + "/" + name + ".png", name), Sprites[name]);
            }
            else
            {
                return Sprite.Create(new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
        }
        public Sprite[] GetSprites()
        {
            EntryPoint.Instance.Log.LogInfo("AtlasData.GetSprites");
            List<Sprite> sprites = new List<Sprite>();
            foreach(KeyValuePair<string, SpriteData> sp in Sprites)
            {
                sprites.Add(ResourceGeneration.CreateSprite(ResourceGeneration.ReadTextureFromFile(BasePath + "/" + sp.Key + ".png", sp.Key), sp.Value));
            }
            return sprites.ToArray();
        }
    }
    [HarmonyPatch(typeof(SpriteAtlas),nameof(SpriteAtlas.GetSprite))]
    public sealed class SpriteAtlas_GetSprite
    {
        public static bool Prefix(string name, SpriteAtlas __instance,ref Sprite __result)
        {
            EntryPoint.Instance.Log.LogInfo("SpriteAtlas.GetSprite");
            AtlasData ad = AtlasManager.Atlases.Find(x => x.Name == __instance.name);
            if(ad != null)
            {
                __result = ad.GetSprite(name);
                EntryPoint.Instance.Log.LogInfo(__result.name);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    [HarmonyPatch(typeof(SpriteAtlas), nameof(SpriteAtlas.GetSprites), new Type[] { typeof(Il2CppReferenceArray<Sprite>)})]
    public sealed class SpriteAtlas_GetSprites
    {
        public static bool Prefix(ref Il2CppReferenceArray<Sprite> sprites, SpriteAtlas __instance, ref int __result)
        {
            EntryPoint.Instance.Log.LogInfo("SpriteAtlas.GetSprites");
            //EntryPoint.Instance.Log.LogInfo($"{AtlasManager.Atlases[0].Name} == {__instance.name} = {AtlasManager.Atlases[0].Name == __instance.name}");
            AtlasData ad = AtlasManager.Atlases.Find(x => x.Name == __instance.name);
            //EntryPoint.Instance.Log.LogInfo(ad.Name);
            if (ad != null)
            {
                __result = ad.Sprites.Count;
                Sprite[] sprs = ad.GetSprites();
                if (sprs.Length != sprites.Length) return true;
                for(int i = 0; i < sprs.Length; i++)
                {
                    sprites[i] = sprs[i];
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    [HarmonyPatch(typeof(SpriteAtlas))]
    [HarmonyPatch(nameof(SpriteAtlas.spriteCount),MethodType.Getter)]
    public sealed class SpriteAtlas_spriteCount
    {
        public static void Postfix(SpriteAtlas __instance, ref int __result)
        {
            AtlasData ad = AtlasManager.Atlases.Find(x => x.Name == __instance.name);
            if (ad != null)
            {
                __result = ad.Sprites.Count;
            }
        }
    }
}
