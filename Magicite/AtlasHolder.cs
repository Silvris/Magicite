using HarmonyLib;
using Syldra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.U2D;

namespace Magicite
{
    public static class AtlasHolder
    {
        public static List<AtlasData> Atlases = new List<AtlasData>();
    }
    public class AtlasData
    {
        public Dictionary<string,Sprite> Sprites = new Dictionary<string, Sprite>();
        public string Name { get; set; }
        public string BasePath { get; set; }
        public AtlasData(string name, string basePath, Dictionary<string, Sprite> sprites)
        {
            //EntryPoint.Logger.LogInfo("AtlasData.ctor");
            Name = name;
            BasePath = basePath;
            Sprites = sprites;
        }
        public static bool ExportFromAtlas(SpriteAtlas atlas, string fullPath, Il2CppSystem.Collections.Generic.Dictionary<string,string> group,string ExportDirectory)
        {
            try
            {
                List<Texture2D> textures = new List<Texture2D>();//in theory, there *should* only be one per
                //but I also think Square Enix broke that should

                Il2CppReferenceArray<Sprite> sprites = new Sprite[atlas.spriteCount];//has to be of type Il2CppReferenceArray apparently
                atlas.GetSprites(sprites);
                //il2cppsystem doesn't support linq, so just make the list
                List<string> keys = new List<string>();
                foreach(string key in group.keys)
                {
                    keys.Add(key);
                }
                string outData = "";
                foreach (Sprite spr in sprites.ToList())
                {
                    if(spr != null)
                    {
                        string sprName = spr.name.Replace("(Clone)", "");
                        string sprBase = "";
                        if (group.ContainsKey(sprName)) sprBase = Path.GetDirectoryName(group[sprName]);
                        else
                        {
                            //likely a nested name like attribute/Icon
                            //use linq to find matching
                            var matches = keys.Where(x => x.Contains(sprName));
                            //now iterate over these for the first one that does not already exist within our export directory
                            //this could easily be inaccurate, but it should work well enough
                            foreach(string key in matches)
                            {
                                if(!File.Exists(Path.Combine(ExportDirectory,Path.GetDirectoryName(group[key]),sprName) + ".spritedata"))
                                {
                                    sprBase = Path.GetDirectoryName(group[key]);
                                    break;
                                }
                            }
                        }
                        string texPath = sprBase + "/" + spr.texture.name;
                        string dataPath = sprBase + "/" + spr.name.Replace("(Clone)", "");
                        outData += $"{sprName};{dataPath}\n";
                        if (!textures.Contains(spr.texture))
                        {
                            Functions.ExportTexture(spr.texture, Path.Combine(ExportDirectory,texPath + ".png"));
                            textures.Add(spr.texture);
                        }
                        SpriteData.ExportFromSprite(spr, Path.Combine(ExportDirectory,dataPath + ".spritedata"), true,texPath);//texPath gets generated regardless if the tex is already exported
                    }
                }
                File.WriteAllText(fullPath, outData);

                return true;
            }
            catch (Exception ex)
            {
                EntryPoint.Logger.LogInfo((object)$"Error occured while exporting sprite {atlas.name}: {ex}");
                return false;
            }
        }

        public Sprite GetSprite(string name)
        {
            //EntryPoint.Logger.LogInfo("AtlasData.GetSprite");
            if (Sprites.ContainsKey(name))
            {
                return UnityEngine.Object.Instantiate(Sprites[name]);
            }
            else
            {
                EntryPoint.Logger.LogWarning((object)$"Atlas: {Name} does not have sprite: {name}");
                return Sprite.Create(new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
        }
        public Sprite[] GetSprites()
        {
            //EntryPoint.Logger.LogInfo("AtlasData.GetSprites");
            List<Sprite> sprites = new List<Sprite>();
            foreach(KeyValuePair<string, Sprite> sp in Sprites)
            {
                sprites.Add(UnityEngine.Object.Instantiate(sp.Value));
            }
            return sprites.ToArray();
        }
    }

    [HarmonyPatch(typeof(SpriteAtlas),nameof(SpriteAtlas.GetSprite))]
    public sealed class SpriteAtlas_GetSprite
    {
        public static bool Prefix(string name, SpriteAtlas __instance,ref Sprite __result)
        {
            //EntryPoint.Logger.LogInfo("SpriteAtlas.GetSprite");
            AtlasData ad = AtlasHolder.Atlases.Find(x => x.Name == __instance.name);
            if(ad != null)
            {
                __result = ad.GetSprite(name);
                //EntryPoint.Logger.LogInfo(__result.name);
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
            //EntryPoint.Logger.LogInfo("SpriteAtlas.GetSprites");
            //EntryPoint.Logger.LogInfo($"{AtlasHolder.Atlases[0].Name} == {__instance.name} = {AtlasHolder.Atlases[0].Name == __instance.name}");
            AtlasData ad = AtlasHolder.Atlases.Find(x => x.Name == __instance.name);
            //EntryPoint.Logger.LogInfo(ad.Name);
            if (ad != null)
            {
                __result = ad.Sprites.Count;
                Sprite[] sprs = ad.GetSprites();
                if (sprs.Length != sprites.Length) return true;//might need to remove this safeguard
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
            //EntryPoint.Logger.LogInfo("SpriteAtlas.spriteCount.getter");
            AtlasData ad = AtlasHolder.Atlases.Find(x => x.Name == __instance.name);
            if (ad != null)
            {
                __result = ad.Sprites.Count;
            }
        }
    }
}
