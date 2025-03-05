using Last.Management;
using System;
using Il2CppSystem.Collections.Generic;
using BepInEx.IL2CPP;
using System.IO;
using System.Linq;
//using System.Text.Json;
using UnityEngine;
using Il2CppSystem.Asset;
using System.Text.RegularExpressions;
using Syldra;

namespace Magicite
{
    public sealed class ResourceCreator : MonoBehaviour
    {
        public static ResourceManager resourceManager { get; private set; }
        public static Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Collections.Generic.Dictionary<string, string>> pathMatch { get; private set; }
        public static string ImportDirectory { get; set; }
        public static Il2CppSystem.Collections.Generic.Dictionary<String, Il2CppSystem.Collections.Generic.Dictionary<String, String>> OurFiles { get; set; }
        public static System.Collections.Generic.Dictionary<String, UnityEngine.Object> loadedFiles { get; set; }
        public static System.Collections.Generic.List<object> loaded { get; set; }
        public static System.Collections.Generic.Dictionary<String,String> OurFilePaths { get; set; }
        public static System.Collections.Generic.Dictionary<String,PartialAsset> PartialAssets { get; set; }
        public static System.Collections.Generic.Dictionary<String, PartialAsset> OutputPartials { get; set; }
        private bool isDisabled = false;
        public ResourceCreator(IntPtr ptr) : base(ptr)
        {
        }
        public ResourceCreator()
        {

        }
        public void Awake()
        {
            try
            {
                OurFiles = new Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Collections.Generic.Dictionary<string, string>>(); // set to a instance to not crash when disabled
                OurFilePaths = new System.Collections.Generic.Dictionary<string, string>();
                ImportDirectory = EntryPoint.Configuration.ImportDirectory;
                loadedFiles = new System.Collections.Generic.Dictionary<string, UnityEngine.Object>();
                loaded = new System.Collections.Generic.List<object>();
                PartialAssets = new System.Collections.Generic.Dictionary<string, PartialAsset>();
                OutputPartials = new System.Collections.Generic.Dictionary<string, PartialAsset>();
                if (EntryPoint.Configuration.ExportEnabled)
                {
                    isDisabled = true;
                }
            }
            catch(Exception ex)
            {
                EntryPoint.Logger.LogError((object)$"[ResourceCreator].ctor:{ex}");
                isDisabled = true;
            }
        }

        public void Update()
        {
            if(resourceManager is null)
            {
                resourceManager = ResourceManager.Instance;
                if (resourceManager is null)
                    return;
            }
            if(pathMatch is null)
            {
                pathMatch = AssetBundlePathMatch.Instance.originalData;
                if (pathMatch is null) return;
            }
            else
            {
                if (!isDisabled)
                {

                    if (ImportDirectory != String.Empty || ImportDirectory != null)
                    {
                        AddFiles();
                        isDisabled = true;
                    }
                    else
                    {
                        isDisabled = true;
                    }
                    //break;
                }
            }

        }
        public static UnityEngine.Object? LoadAsset(string fullPath, string addressName, string ext, Il2CppSystem.Object originalAsset)
        {
            fullPath = fullPath.Replace("\\", "/");
            //EntryPoint.Logger.LogInfo(fullPath);
            //EntryPoint.Logger.LogInfo(ext);
            //EntryPoint.Logger.LogInfo(originalAsset == null);
            switch (ext)
            {
                case ".csv":
                    if (fullPath == "PartialAsset.csv" && originalAsset != null)
                    {
                        TextAsset text = originalAsset.Cast<TextAsset>();
                        string name = "";
                        if (text.name != String.Empty) name = text.name;
                        else name = Path.GetFileName(addressName);
                        if (PartialAssets.ContainsKey(name))
                        {
                            CSVData partialData = (CSVData)PartialAssets[name];
                            CSVData baseData = new CSVData(name, text);
                            baseData.MergeAsset(partialData);
                            TextAsset t = baseData.ToAsset();
                            //EntryPoint.Logger.LogInfo(t.text);
                            if (!OutputPartials.ContainsKey(name)) OutputPartials.Add(name, baseData);
                            else OutputPartials[name] = baseData;
                            t.hideFlags = HideFlags.HideAndDontSave;
                            //if (!loadedFiles.ContainsKey(text.name)) loadedFiles.Add(text.name, t);
                            return t;
                        }
                        else
                        {
                            //can't partial, attempt to load a full path
                            //note, this *will* throw an exception, but it'll definitely make you aware that it is a problem
                            TextAsset ntext = ResourceGeneration.CreateTextAsset(File.ReadAllText(fullPath));
                            ntext.name = Path.GetFileNameWithoutExtension(fullPath);
                            return ntext;
                        }
                    }
                    else
                    {
                        //can't partial, attempt to load a full path
                        TextAsset text = ResourceGeneration.CreateTextAsset(File.ReadAllText(fullPath));
                        text.name = Path.GetFileNameWithoutExtension(fullPath);
                        return text;
                    }
                case ".txt":
                    EntryPoint.Logger.LogInfo(originalAsset == null);
                    if(fullPath == "PartialAsset.txt" && originalAsset != null)
                    {
                        TextAsset text = originalAsset.Cast<TextAsset>();
                        string name = "";
                        if (text.name != String.Empty) name = text.name;
                        else name = Path.GetFileName(addressName);
                        if (PartialAssets.ContainsKey(name))
                        {
                            TXTData partialData = (TXTData)PartialAssets[name];
                            TXTData baseData = new TXTData(name, text);
                            baseData.MergeAsset(partialData);
                            TextAsset t = baseData.ToAsset();
                            if (!OutputPartials.ContainsKey(name))
                            {
                                OutputPartials.Add(name, baseData);
                            }
                            else OutputPartials[name] = baseData;
                            t.hideFlags = HideFlags.HideAndDontSave;
                            return t;
                        }
                        else
                        {
                            //can't partial, attempt to load a full path
                            TextAsset ntext = ResourceGeneration.CreateTextAsset(File.ReadAllText(fullPath));
                            ntext.name = Path.GetFileNameWithoutExtension(fullPath);
                            ntext.hideFlags = HideFlags.HideAndDontSave;
                            return ntext;
                        }
                    }
                    else
                    {
                        //can't partial, attempt to load a full path
                        TextAsset text = ResourceGeneration.CreateTextAsset(File.ReadAllText(fullPath));
                        text.name = Path.GetFileNameWithoutExtension(fullPath);
                        text.hideFlags = HideFlags.HideAndDontSave;
                        return text;
                    }
                case ".json":
                    TextAsset asset = ResourceGeneration.CreateTextAsset(File.ReadAllText(fullPath));
                    asset.name = Path.GetFileNameWithoutExtension(fullPath);
                    asset.hideFlags = HideFlags.HideAndDontSave;
                    return asset;
                case ".png":
                    //check for .spriteData, with we define asset as Sprite, without we just load T2D itself
                    Texture2D tex = ResourceGeneration.ReadTextureFromFile(fullPath, Path.GetFileNameWithoutExtension(fullPath));
                    tex.hideFlags = HideFlags.HideAndDontSave;
                    if (File.Exists(Path.ChangeExtension(fullPath, ".spriteData")))
                    {
                        SpriteData sd = new SpriteData(File.ReadAllLines(Path.ChangeExtension(fullPath, ".spriteData")), Path.GetFileNameWithoutExtension(fullPath));
                        Sprite spr = ResourceGeneration.CreateSprite(tex, sd);
                        spr.hideFlags = HideFlags.HideAndDontSave;
                        return spr;
                    }
                    else
                    {
                        return tex;
                    }
                case ".spritedata":

                    //spritedata likely with texture override, allowing for sprite sheet use
                    string[] strings = File.ReadAllLines(fullPath);
                    EntryPoint.Logger.LogInfo(strings.Length);
                    EntryPoint.Logger.LogInfo(fullPath);
                    SpriteData sa = new SpriteData(strings, Path.GetFileNameWithoutExtension(fullPath));
                    loaded.Add(sa);
                    
                    if (sa.hasTO)
                    {
                        //spritedata defines texture to use
                        Texture2D texOver = ResourceGeneration.ReadTextureFromFile(Path.Combine(Regex.Replace(fullPath, "/Assets/GameAssets/.*$", ""),sa.textureOverride) + ".png", Path.GetFileNameWithoutExtension(sa.textureOverride));
                        Sprite spr = ResourceGeneration.CreateSprite(texOver, sa);
                        spr.hideFlags = HideFlags.HideAndDontSave;
                        return spr;
                    }
                    else
                    {
                        Texture2D defTex = new Texture2D(1, 1);
                        Sprite spr = ResourceGeneration.CreateSprite(defTex, sa);
                        spr.hideFlags = HideFlags.HideAndDontSave;
                        return spr;
                    }
                case ".atlas":
                    //EntryPoint.Logger.LogInfo($"fullPath:{fullPath}, regex:{Regex.Replace(fullPath, "/Assets/GameAssets/.*$", "")}");
                    return ResourceGeneration.CreateSpriteAtlas(Path.GetFileNameWithoutExtension(fullPath), fullPath);
                case ".bytes":
                    TextAsset binary = ResourceGeneration.CreateBinaryTextAsset(Path.GetFileNameWithoutExtension(fullPath),fullPath);
                    binary.hideFlags = HideFlags.HideAndDontSave;
                    return binary;
                    
                default:
                    EntryPoint.Logger.LogError((object)(object)$"Failed to load asset of type: {ext}");
                    return null;
            }
        }
        public bool ImportableFile(string path)
        {
            switch (Path.GetExtension(path))
            {
                case ".png":
                case ".csv":
                case ".json":
                case ".txt":
                case ".atlas":
                case ".bytes":
                case ".spritedata":
                    return true;
                default:
                    return false;
            }
        }
        public void GenOurFiles()
        {
            if (!Directory.Exists(ImportDirectory))
            {
                EntryPoint.Logger.LogError((object)$"Import directory \"{ImportDirectory}\" does not exist!");
                return;
            }
            System.Collections.Generic.List<string> mods = new System.Collections.Generic.List<string>();
            JsonDict baseAssetsPathData = new JsonDict();
            try
            {
                System.Collections.Generic.List<string> mod = Directory.GetDirectories(ImportDirectory).ToList();
                foreach (string m in mod)
                {
                    mods.Add(m);
                }
            }
            catch (Exception ex)
            {
                EntryPoint.Logger.LogError((object)$"[ResourceCreator.AddFiles]: {ex}");
                return;
            }
            foreach(string mod in mods)
            {
                //EntryPoint.Logger.LogInfo(Path.GetFileName(mod));
                System.Collections.Generic.List<string> groups = new System.Collections.Generic.List<string>();
                try
                {
                    System.Collections.Generic.List<string> group = Directory.GetDirectories(mod).ToList();
                    foreach (string g in group)
                    {
                        groups.Add(g.ToLower());
                    }
                }
                catch (Exception ex)
                {
                    EntryPoint.Logger.LogError((object)$"[ResourceCreator.AddFiles]: {ex}");
                    return;
                }
                JsonDict assetsPathData = new JsonDict();
                foreach (String group in groups)
                {
                    //ModComponent.Log.LogInfo(group + "/keys.json");
                    /*
                    if(File.Exists(group + "/keys.json"))
                    {
                        keys.Add(File.ReadAllText(group + "/keys.json"));
                    }*/
                    //EntryPoint.Logger.LogInfo(Path.GetFileName(group));
                    if (Directory.Exists(group + "/keys"))
                    {
                        if (assetsPathData.keys.Contains(Path.GetFileName(group)))
                        {
                            JsonDict present = JsonHandling.FromJson(assetsPathData.GetValue(Path.GetFileName(group)));
                            present.MergeDict(JsonHandling.MergeJsonDictsInPath(Path.Combine(group, "keys"), Path.GetFileName(group)));
                            assetsPathData.SetValue(Path.GetFileName(group), JsonHandling.ToJson(present));
                            //EntryPoint.Logger.LogInfo("ContainsKey");
                            //EntryPoint.Logger.LogInfo(JsonHandling.ToJson(present));
                        }
                        else
                        {
                            assetsPathData.SetValue(Path.GetFileName(group), JsonHandling.ToJson(JsonHandling.MergeJsonDictsInPath(Path.Combine(group, "keys"), Path.GetFileName(group))));
                            //EntryPoint.Logger.LogInfo("KeyNotPresent");
                            //EntryPoint.Logger.LogInfo(assetsPathData.GetValue(Path.GetFileName(group)));
                        }

                    }
                }
                baseAssetsPathData.MergeDict(assetsPathData);
                //EntryPoint.Logger.LogInfo(JsonHandling.ToJson(baseAssetsPathData));
            }
            String assetsPathtxt = JsonHandling.ToJson(baseAssetsPathData);    
            //EntryPoint.Logger.LogInfo(assetsPathtxt);
            OurFiles = AssetPathUtilty.Parse(assetsPathtxt);
        }
        public void FillOurFiles()
        {
            if (OurFiles != null)
            {
                //kinda assuming it'll return null if the data is wrong
                foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, Il2CppSystem.Collections.Generic.Dictionary<string, string>> group in OurFiles)
                {
                    //EntryPoint.Logger.LogInfo(group.Key);
                    System.Collections.Generic.List<Il2CppSystem.Collections.Generic.KeyValuePair<string, string>> removes = new System.Collections.Generic.List<Il2CppSystem.Collections.Generic.KeyValuePair<string, string>>();
                    foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, string> kvp in group.Value)
                    {
                        //EntryPoint.Logger.LogInfo(kvp.Key);
                        if (!loadedFiles.ContainsKey(kvp.value)) //this allows us to register sprites loaded by atlas generation, and then ignore them here
                        {
                            //EntryPoint.Logger.LogInfo(group.key);
                            //EntryPoint.Logger.LogInfo(kvp.value);
                            //adding a directory check here, just to further prevent failure
                            //especially while GameObjects cannot be added yet
                            var directories = Directory.GetDirectories(ImportDirectory);
                            //EntryPoint.Logger.LogInfo(Path.GetDirectoryName(Path.Combine(ImportDirectory, group.key, kvp.value)));
                            if (true)
                            {
                                System.Collections.Generic.List<string> files = new System.Collections.Generic.List<string>();
                                foreach(string dir in directories)
                                {
                                    if (Directory.Exists(Path.Combine(ImportDirectory, dir, group.key, Path.GetDirectoryName(kvp.value))))
                                    {
                                        var fileGrab = Directory.GetFiles(Path.Combine(ImportDirectory, dir, group.key), $"{kvp.value}.*", SearchOption.AllDirectories);

                                        foreach (string file in fileGrab)
                                        {
                                            if (ImportableFile(file))
                                            {
                                                files.Add(file);
                                            }
                                        }
                                    }

                                }
                                if (files.Count > 0)
                                {
                                    //at least one file exists, we're gonna just use the first one as to force unique keys
                                    string file = files[0];
                                    string ext = Path.GetExtension(file);
                                    if (ext == ".spritedata")
                                    {
                                        foreach (string fTest in files)
                                        {
                                            if (Path.GetExtension(fTest) == ".png")
                                            {
                                                file = fTest;
                                                ext = Path.GetExtension(fTest);
                                            }
                                        }
                                    }
                                    //EntryPoint.Logger.LogInfo(file);
                                    //EntryPoint.Logger.LogInfo(ext);
                                    if (ext == ".csv"||ext == ".txt")
                                    {
                                        //partials
                                        PartialAsset asset;
                                        if (ext == ".csv") asset = new CSVData(Path.GetFileNameWithoutExtension(file));
                                        else asset = new TXTData(Path.GetFileNameWithoutExtension(file));
                                        foreach(string pfile in files)
                                        {
                                            asset.MergeData(new TextAsset(File.ReadAllText(pfile)));
                                        }
                                        string key = Path.GetFileNameWithoutExtension(file);
                                        //EntryPoint.Logger.LogInfo(PartialAssets.ContainsKey(Path.GetFileNameWithoutExtension(file)));
                                        if (PartialAssets.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                                        {

                                            PartialAssets[Path.GetFileNameWithoutExtension(file)].MergeAsset(asset);
                                        }
                                        else 
                                        { 

                                            PartialAssets.Add(Path.GetFileNameWithoutExtension(file), asset);
                                        }
                                        file = "PartialAsset" + ext;
                                    }
                                    if (ext != String.Empty)
                                    {
                                        OurFilePaths.Add(kvp.value,file);
                                        //EntryPoint.Logger.LogInfo(file);
                                        if (resourceManager.completeAssetDic.ContainsKey(kvp.value))
                                        {
                                            UnityEngine.Object? asset = LoadAsset(file, kvp.value, ext,resourceManager.completeAssetDic[kvp.value]);
                                            if(asset != null)
                                            {
                                                //EntryPoint.Logger.LogInfo(asset);
                                                resourceManager.completeAssetDic[kvp.value] = asset;
                                            }
                                        }
                                        //there used to be an else, but I realized that since the resourceManager hook should catch every file
                                        //that we can just need to worry about ones already present in the assetDic
                                    }
                                }
                                else
                                {
                                    removes.Add(kvp);
                                }
                            }
                            
                        }

                    }
                    foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, string> kvp in removes)
                    {
                        group.Value.Remove(kvp.key);
                    }
                    if (pathMatch.ContainsKey(group.key))
                    {
                        Il2CppSystem.Collections.Generic.Dictionary<string, string> currentData = pathMatch[group.key];
                        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, string> kvp in group.value)
                        {
                            if (currentData.ContainsKey(kvp.key))
                            {
                                currentData[kvp.key] = kvp.value; //this lets you reroute certain files?
                            }
                            else
                            {
                                currentData.Add(kvp.key, kvp.value);
                            }
                        }
                    }
                    else
                    {
                        pathMatch.Add(group.key, group.value);
                    }
                }
            }
        
    }
        public void AddFiles()
        {
            GenOurFiles();
            FillOurFiles();//testing to see if it wouldn't be more efficient to just jit file generation
            //could potentially set it as a mode for "fast but inefficient" vs "slow but memory efficient"
        }

    }
}
