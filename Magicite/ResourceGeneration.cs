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
using UnityEngine;
using UnityEngine.U2D;
using Syldra;

namespace Magicite
{
    static class ResourceGeneration
    {
        public static Dictionary<string,UnityEngine.Object> DonorAssets { get; set; }
        public static Texture2D ReadTextureFromFile(String fullPath, String Name)
        {
            return Syldra.Functions.ReadTextureFromFile(fullPath, Name);

        }
        public static TextAsset CreateBinaryTextAsset(string name ,string fullPath)
        {
            TextAsset binary = new TextAsset("MAGI" + fullPath) { name = name };
            binary.hideFlags = HideFlags.HideAndDontSave;
            return binary;
        }
        public static Sprite CreateSprite(Texture2D tex,SpriteData sd)
        {
            return sd.CreateSpriteFromData(tex);
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
                            //EntryPoint.Logger.LogInfo(basePath + "/" + sd.textureOverride + ".png");
                            spr = sd.CreateSpriteFromData(null,basePath);
                        }
                        else
                        {
                            tex = ReadTextureFromFile(path + ".png", name);
                            spr = sd.CreateSpriteFromData(tex);
                        }
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
            AtlasHolder.Atlases.Add(ad);
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


}