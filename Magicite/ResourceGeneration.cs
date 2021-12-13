using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.U2D;

namespace Magicite
{
    static class ResourceGeneration
    {
        public static Dictionary<string,UnityEngine.Object> DonorAssets { get; set; }
        public static Texture2D ReadTextureFromFile(String fullPath, String Name)
        {
            try
            {
                Byte[] bytes = File.ReadAllBytes(fullPath);
                Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false) { name = Name };
                texture.filterMode = FilterMode.Point;
                if (!ImageConversion.LoadImage(texture, bytes))
                    throw new NotSupportedException($"Failed to load texture from file [{fullPath}]");
                texture.hideFlags = HideFlags.HideAndDontSave;
                return texture;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static TextAsset CreateBinaryTextAsset(string fullPath)
        {
            return new TextAsset("MAGI" + fullPath);
        }
        public static Sprite CreateSprite(Texture2D tex,SpriteData sd)
        {
            Sprite spr = Sprite.Create(
                tex,
                sd.hasRect ? sd.rect : new Rect(0, 0, tex.width, tex.height),
                sd.hasPivot ? sd.pivot : new Vector2(0.5f, 0.5f),
                sd.hasPPU ? sd.pixelsPerUnit : 1f,
                0,
                SpriteMeshType.Tight,
                sd.hasBorder ? sd.border : new Vector4(0, 0, 0, 0)
                );
            tex.wrapMode = sd.hasWrap ? sd.wrapMode : TextureWrapMode.Clamp;
            spr.name = sd.name;
            spr.hideFlags = HideFlags.HideAndDontSave;
            return spr;
        }
        public static Dictionary<string,Sprite> ReadSpriteAtlas(string[] lines, string basePath)
        {
            Dictionary<string, Sprite> sds = new Dictionary<string, Sprite>();
            foreach(string line in lines)
            {
                string name = line.Replace("\n", "").Replace("\r", "");
                string path = basePath + "/" + name;
                string key = path.Replace(EntryPoint.Configuration.ImportDirectory, "");
                Sprite data;
                if (ResourceCreator.loadedFiles.ContainsKey(key))
                {
                    data = ResourceCreator.loadedFiles[key].Cast<Sprite>();
                }
                else
                {
                    data = CreateSprite(ReadTextureFromFile(path + ".png", name), new SpriteData(File.ReadAllLines(path + ".spritedata"), name));
                    ResourceCreator.loadedFiles.Add(key, data);
                }
                sds.Add(line,data);
            }
            return sds;
        }
        public static SpriteAtlas CreateSpriteAtlas(string name, string fullPath)
        {
            //Unity doesn't really support proper access to SpriteAtlases in scripting, so instead we provide a "dummy"
            //SpriteAtlas that calls its individual sprites on the GetSprite and GetSprites functions
            SpriteAtlas atlas = UnityEngine.Object.Instantiate(DonorAssets["SpriteAtlas"]).Cast<SpriteAtlas>();
            //I'm really hoping this doesn't grab the same asset twice
            atlas.name = name;
            //EntryPoint.Instance.Log.LogInfo(atlas.name);
            //now generate the needed information for our Atlas functions to run
            AtlasData ad = new AtlasData(name, Path.GetDirectoryName(fullPath), ReadSpriteAtlas(File.ReadAllLines(fullPath), Path.GetDirectoryName(fullPath)));
            AtlasManager.Atlases.Add(ad);
            return atlas;
        }
        public static TextAsset CreateTextAsset(string data)
        {
            return new TextAsset(data);
        }
    }
}