using Last.Management;
using System;
using Il2CppSystem.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Text.Json;
using System.Threading.Tasks;
//using System.Web.Script.Serialization;
using UnityEngine;
using Il2CppSystem.Asset;

namespace Magicite
{
    public sealed class ResourceCreator : MonoBehaviour
    {
        public static ResourceManager resourceManager { get; private set; }
        public static Dictionary<string, Dictionary<string, string>> pathMatch { get; private set; }
        public static string ImportDirectory { get; set; }
        public static Dictionary<String, Dictionary<String, String>> OurFiles { get; set; }
        public static Dictionary<String, UnityEngine.Object> loadedFiles { get; set; }
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
                OurFiles = new Dictionary<string, Dictionary<string, string>>(); // set to a instance to not crash when disabled
                ImportDirectory = EntryPoint.Configuration.ImportDirectory;
                loadedFiles = new Dictionary<string, UnityEngine.Object>();
            }
            catch(Exception ex)
            {
                EntryPoint.Instance.Log.LogError($"[ResourceCreator].ctor:{ex}");
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
        public static UnityEngine.Object LoadAsset(string fullPath, string ext)
        {
            switch (ext)
            {
                case ".csv":
                case ".txt":
                case ".json":
                    TextAsset asset = ResourceGeneration.CreateTextAsset(File.ReadAllText(fullPath));
                    return asset;
                case ".png":
                    //check for .spriteData, with we define asset as Sprite, without we just load T2D itself
                    Texture2D tex = ResourceGeneration.ReadTextureFromFile(fullPath, Path.GetFileNameWithoutExtension(fullPath));
                    if (File.Exists(Path.ChangeExtension(fullPath, ".spriteData")))
                    {
                        SpriteData sd = new SpriteData(File.ReadAllLines(Path.ChangeExtension(fullPath, ".spriteData")), Path.GetFileNameWithoutExtension(fullPath));
                        Sprite spr = ResourceGeneration.CreateSprite(tex, sd);
                        return spr;
                    }
                    else
                    {
                        return tex;
                    }
                case ".atlas":
                    //EntryPoint.Instance.Log.LogInfo(fullPath);
                    return ResourceGeneration.CreateSpriteAtlas(Path.GetFileNameWithoutExtension(fullPath), fullPath);
                case ".bytes":
                    TextAsset binary = ResourceGeneration.CreateBinaryTextAsset(fullPath);
                    return binary;
                default:
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
                    return true;
                default:
                    return false;
            }
        }
        public void AddFiles()
        {
            List<string> groups = new List<string>();
            try
            {
                System.Collections.Generic.List<string> group = Directory.GetDirectories(ImportDirectory).ToList();
                foreach (string g in group)
                {
                    groups.Add(g);
                }
            }
            catch(Exception ex)
            {
                EntryPoint.Instance.Log.LogError($"[ResourceCreator.AddFiles]: {ex}");
                return;
            }
            JsonDict assetsPathData = new JsonDict();
            foreach(String group in groups)
            {
                //ModComponent.Log.LogInfo(group + "/keys.json");
                /*
                if(File.Exists(group + "/keys.json"))
                {
                    keys.Add(File.ReadAllText(group + "/keys.json"));
                }*/
                if(Directory.Exists(group + "/keys"))
                {
                    assetsPathData.keys.Add(Path.GetFileName(group));
                    assetsPathData.values.Add(JsonHandling.ToJson(JsonHandling.MergeJsonDictsInPath(group + "/keys", Path.GetFileName(group))));
                }
            }
            String assetsPathtxt = JsonHandling.ToJson(assetsPathData);
            //EntryPoint.Instance.Log.LogInfo(assetsPathtxt);
            OurFiles = AssetPathUtilty.Parse(assetsPathtxt);
            if(OurFiles != null)
            {
                //kinda assuming it'll return null if the data is wrong
                foreach (KeyValuePair<string, Dictionary<string,string>> group in OurFiles)
                {
                    foreach (KeyValuePair<string, string> kvp in group.Value)
                    {
                        //EntryPoint.Instance.Log.LogInfo(group.key);
                        //EntryPoint.Instance.Log.LogInfo(kvp.value);
                        var fileGrab = Directory.GetFiles(ImportDirectory + $"\\{group.key}", $"{kvp.value}.*");
                        List<string> files = new List<string>();
                        foreach (string file in fileGrab)
                        {
                            if (ImportableFile(file))
                            {
                                files.Add(file);
                            }
                        }
                        if(files.Count > 0)
                        {
                            //at least one file exists, we're gonna just use the first one as to force unique keys
                            string file = files[0];
                            string ext = Path.GetExtension(file);
                            if(ext != String.Empty)
                            {
                                UnityEngine.Object asset = LoadAsset(file, ext);
                                //EntryPoint.Instance.Log.LogInfo(file);
                                //EntryPoint.Instance.Log.LogInfo(asset);
                                if(asset != null)
                                {
                                    if (resourceManager.completeAssetDic.ContainsKey(kvp.value))
                                    {
                                        resourceManager.completeAssetDic[kvp.value] = asset;
                                    }
                                    //there used to be an else, but I realized that since the resourceManager hook should catch every file
                                    //that we can just need to worry about ones already present in the assetDic
                                    loadedFiles.Add(kvp.value, asset);//this should let us maintain it
                                }
                            }
                        }
                    }
                    if (pathMatch.ContainsKey(group.key))
                    {
                        Dictionary<string, string> currentData = pathMatch[group.key];
                        foreach(KeyValuePair<string,string> kvp in group.value)
                        {
                            if (currentData.ContainsKey(kvp.key))
                            {
                                currentData[kvp.key] = kvp.value; //this lets you reroute certain files?
                            }
                            else
                            {
                                currentData.Add(kvp.key,kvp.value);
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

    }
}
