using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
            TextAsset binary = new TextAsset("MAGI" + fullPath);
            binary.hideFlags = HideFlags.HideAndDontSave;
            return binary;
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
                string[] data = line.Split(';');
                if (!(data.Length > 1)) throw new FormatException("SpriteAtlas Entry must have name and path separated by \";\"");
                string name = data[0].Replace("\n", "").Replace("\r", "");
                string path = basePath + "/" + data[1];
                string key = path.Replace(EntryPoint.Configuration.ImportDirectory, "");
                Sprite spr = null;
                if(name != "")
                {
                    if (ResourceCreator.loadedFiles.ContainsKey(key))
                    {
                        spr = ResourceCreator.loadedFiles[key].Cast<Sprite>();
                    }
                    else
                    {
                        SpriteData sd = new SpriteData(File.ReadAllLines(path + ".spritedata"), name);
                        Texture2D tex;
                        if (sd.hasTO)
                        {
                            EntryPoint.Logger.LogInfo(basePath + "/" + sd.textureOverride + ".png");
                            tex = ReadTextureFromFile(Path.Combine(basePath,sd.textureOverride) + ".png", Path.GetFileName(sd.textureOverride));
                        }
                        else
                        {
                            tex = ReadTextureFromFile(path + ".png", name);
                        }
                        spr = CreateSprite(tex, sd);
                        ResourceCreator.loadedFiles.Add(key, spr);
                    }
                    sds.Add(name, spr);
                }

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
            //EntryPoint.Logger.LogInfo(atlas.name);
            //now generate the needed information for our Atlas functions to run
            AtlasData ad = new AtlasData(name, Path.GetDirectoryName(fullPath), ReadSpriteAtlas(File.ReadAllLines(fullPath), Regex.Replace(fullPath, "/Assets/GameAssets/.*$", "")));
            AtlasManager.Atlases.Add(ad);
            atlas.hideFlags = HideFlags.HideAndDontSave;
            return atlas;
        }
        public static TextAsset CreateTextAsset(string data)
        {
            TextAsset text =  new TextAsset(data);
            text.hideFlags = HideFlags.HideAndDontSave;
            return text;
        }
    }

    public static class TextureWriting
    {
        //thanks to Albeoris for these
        public static Texture2D GetFragment(Texture2D texture, Int32 x, Int32 y, Int32 width, Int32 height)
        {
            if (texture == null)
                return null;

            Texture2D result = new Texture2D(width, height, texture.format, false);
            Color[] colors = texture.GetPixels(x, y, width, height);
            result.SetPixels(colors);
            result.Apply();
            return result;
        }

        public static Texture2D CopyAsReadable(Texture texture)
        {
            if (texture == null)
                return null;

            RenderTexture oldTarget = Camera.main.targetTexture;
            RenderTexture oldActive = RenderTexture.active;

            Texture2D result = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);

            RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            try
            {
                Camera.main.targetTexture = rt;
                //Camera.main.Render();
                Graphics.Blit(texture, rt);

                RenderTexture.active = rt;
                result.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            }
            finally
            {
                RenderTexture.active = oldActive;
                Camera.main.targetTexture = oldTarget;
                RenderTexture.ReleaseTemporary(rt);
            }

            return result;
        }

        public static void WriteTextureToFile(Texture2D texture, String outputPath)
        {
            Byte[] data;
            String extension = Path.GetExtension(outputPath);
            switch (extension)
            {
                case ".png":
                    data = ImageConversion.EncodeToPNG(texture);
                    break;
                case ".jpg":
                    data = ImageConversion.EncodeToJPG(texture);
                    break;
                case ".tga":
                    data = ImageConversion.EncodeToTGA(texture);
                    break;
                default:
                    throw new NotSupportedException($"Not supported type [{extension}] of texture [{texture.name}]. Path: [{outputPath}]");
            }

            File.WriteAllBytes(outputPath, data);
        }

        public static void ExportTexture(Texture2D asset, String fullPath)
        {
            if (asset.isReadable)
            {
                WriteTextureToFile(asset, fullPath);
            }
            else
            {
                Texture2D readable = CopyAsReadable(asset);
                WriteTextureToFile(readable, fullPath);
                UnityEngine.Object.Destroy(readable);
            }
        }
    }

}