using Il2CppSystem.Asset;
using Last.Management;
using System;
using Il2CppSystem.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using UnityEngine.U2D;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using Syldra;

namespace Magicite
{
    public sealed class ResourceExporter : MonoBehaviour
    {
        public ResourceExporter(IntPtr ptr) : base(ptr)
        {
        }
        public static ResourceExporter Instance { get; set; }
        private String _exportDirectory;
        private ResourceManager _resourceManager;
        private Dictionary<String, Dictionary<String, String>>.Enumerator _enumerator;
        private KeyValuePair<String, Dictionary<String, String>> _currentGroup;
        private DateTime _loadingStartTime;
        private DateTime _loadingLogTime;
        private Int32 _currentIndex;
        private Int32 _totalCount = 1;
        private Texture2D _blackTexture;
        private GUIStyle _guiStyle;
        private bool _exportEnabled;
        private System.Collections.Generic.List<AsyncSpriteExport> spriteExports = new System.Collections.Generic.List<AsyncSpriteExport>();
        public void Awake()
        {
            try
            {
                Instance = this;
                _exportDirectory = EntryPoint.Configuration.ExportDirectory;
                _exportEnabled = EntryPoint.Configuration.ExportEnabled;
                if (_exportDirectory == ""||!_exportEnabled)
                {
                    EntryPoint.Logger.LogInfo("Export skipped, as export directory is not defined or export is disabled.");
                    Destroy(this);
                    return;
                }

                Time.timeScale = 0.0f;
                EntryPoint.Logger.LogInfo("Export starting, waiting for ResourceManager");
                EntryPoint.Logger.LogInfo((object)$"Export directory: {_exportDirectory}");

                _blackTexture = new Texture2D(1, 1);
                _blackTexture.SetPixel(0, 0, Color.black);
                _blackTexture.Apply();

                _guiStyle = new GUIStyle();
                _guiStyle.fontSize = 48;
                _guiStyle.normal.textColor = Color.white;
                _guiStyle.alignment = TextAnchor.MiddleCenter;
            }
            catch(Exception ex)
            {
                OnExportError("Export failed during initialization", ex);
            }
        }
        public void Update()
        {
            try
            {
                if(_resourceManager is null)
                {
                    _resourceManager = ResourceManager.Instance;
                    if(_resourceManager is null)
                    {
                        return;
                    }
                }
                if (_enumerator is null)
                {
                    if (!_resourceManager.CheckLoadAssetCompleted("AssetsPath"))
                        return;
                    Dictionary<String, Dictionary<String, String>> pathMatch = null;
                    if (pathMatch is null)
                    {
                        pathMatch = AssetBundlePathMatch.Instance.originalData;
                        if (pathMatch is null) return;
                    }
                   
                    _totalCount = pathMatch.Count;
                    _enumerator = pathMatch.GetEnumerator();

                    EntryPoint.Logger.LogInfo((object)$"[Export] Exporting assets {_totalCount} listed in AssetsPath...");
                }
                
                // Must have to export not readable textures
                if (Camera.main is null)
                    return;

                foreach(AsyncSpriteExport ase in spriteExports)
                {
                    ase.Update();
                }

                if (_currentGroup != null)
                {
                    String assetGroup = _currentGroup.Key;
                    TimeSpan elapsedTime = DateTime.Now - _loadingLogTime;
                    if (!_resourceManager.CheckGroupLoadAssetCompleted(assetGroup))
                    {
                        if (elapsedTime.TotalSeconds > 5)
                        {
                            elapsedTime = DateTime.Now - _loadingStartTime;
                            EntryPoint.Logger.LogInfo((object)$"[Export ({_currentIndex} / {_totalCount})] Loading assets from [{assetGroup}]. Elapsed: {elapsedTime.TotalSeconds} sec.");
                            _loadingLogTime = DateTime.Now;
                        }

                        return;
                    }
                    
                    elapsedTime = DateTime.Now - _loadingStartTime;
                    Dictionary<String, String> assets = _currentGroup.Value;
                    EntryPoint.Logger.LogInfo((object)$"[Export ({_currentIndex} / {_totalCount})] Loaded {assets.Count} assets from [{assetGroup}] in {elapsedTime.TotalSeconds} sec. Exporting...");
                    ExportKeysDict(assets, Path.Combine(_exportDirectory, assetGroup, "keys","Export.json"));
                    Dictionary<String, Il2CppSystem.Object> loaded = _resourceManager.completeAssetDic;
                    foreach (var pair in assets)
                    {
                        String assetName = pair.Key;
                        String assetPath = pair.Value;
                        while (!_resourceManager.CheckLoadAssetCompleted(assetPath))
                        {
                            elapsedTime = DateTime.Now - _loadingStartTime;
                            EntryPoint.Logger.LogInfo((object)$"[Export ({_currentIndex} / {_totalCount})] Waiting for {assetName} to load. Elapsed: {elapsedTime.TotalSeconds} sec.");
                            _loadingLogTime = DateTime.Now;
                        }

                        Il2CppSystem.Object asset = loaded[assetPath];
                        if (asset is null)
                        {
                            EntryPoint.Logger.LogError((object)$"[Export ({_currentIndex} / {_totalCount})] \tCannot find asset [{assetName}]: {assetPath}");
                            continue;
                        }

                        String extension = GetFileExtension(assetPath);
                        String type = GetAssetType(asset);
                        if(type == "UnityEngine.Texture2D")
                        {
                            AsyncSpriteExport spr = new AsyncSpriteExport(assetPath, assetGroup);
                            spriteExports.Add(spr);
                            EntryPoint.Logger.LogInfo((object)$"Starting async sprite export for {assetName} in {assetGroup}");
                        }
                        String exportPath = assetPath + extension;

                        ExportAsset(asset, type, assetName, assetGroup, exportPath, assets);
                    }

                    //_resourceManager.DestroyGroupAsset(assetGroup);
                    _currentGroup = null;
                }

                if (_enumerator.MoveNext())
                {
                    _loadingStartTime = DateTime.Now;
                    _loadingLogTime = _loadingStartTime;
                    _currentGroup = _enumerator.current;

                    try
                    {
                        _currentIndex++;
                        _resourceManager.RequestGroupLoadAssetBundle(_currentGroup.Key);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to load assets from [{_currentGroup.Key}].", ex);
                    }
                }
                else
                {
                    foreach(AsyncSpriteExport ase in spriteExports)
                    {
                        if (!ase.IsDone())
                        {
                            return;
                        }
                    }
                    EndExport();

                }

            }
            catch(Exception ex)
            {
                OnExportError("Export failed during update", ex);
            }
        }
        private void EndExport()
        {
            EntryPoint.Logger.LogInfo((object)$"[Export ({_currentIndex} / {_totalCount})] Assets exported successfully.");
            EntryPoint.Configuration.DisableExport();
            Destroy(this);
        }
        private void ExportAsset(Il2CppSystem.Object asset, string type, string assetName,string group, string exportPath, Dictionary<string,string> groupData)
        {
            string fullPath = Path.Combine(_exportDirectory, group, exportPath);
            //EntryPoint.Logger.LogInfo($"{assetName}:{type}:{asset.GetIl2CppType().Name}");
            switch (type)
            {
                case "UnityEngine.Sprite":
                    string extension = Path.GetExtension(fullPath);
                    string noExtPath = fullPath.Replace(extension, "");
                    ExportSprite(asset.Cast<Sprite>(), noExtPath,extension);
                    break;
                case "UnityEngine.TextAsset":
                    ExportText(asset.Cast<TextAsset>(), fullPath);
                    break;
                case "System.Byte[]":
                    ExportBinary(asset.Cast<TextAsset>(), fullPath);
                    break;
                case "UnityEngine.Texture2D":
                    ExportTexture2D(assetName, asset.Cast<Texture2D>(), fullPath);
                    break;
                case "UnityEngine.U2D.SpriteAtlas":
                    ExportAtlas(asset.Cast<SpriteAtlas>(), fullPath,groupData,group);
                    break;
                default:
                    EntryPoint.Logger.LogInfo((object)$"Unable to export file {assetName} from group {group} as it is an unsupported type {type}");
                    break;
            }
        }
        private void ExportAtlas(SpriteAtlas asset, string fullPath, Dictionary<string,string> groupData, string groupName)
        {
            EntryPoint.Logger.LogInfo((object)$"[Export ({_currentIndex} / {_totalCount})] \tExport [{asset.name}] SpriteAtlas {fullPath}");
            PrepareDirectory(fullPath);
            AtlasData.ExportFromAtlas(asset, fullPath,groupData,Path.Combine(_exportDirectory,groupName));
        }

        private void ExportSprite(Sprite asset, string noExtPath, string textureExt)
        {
            if (!asset.packed)
            {
                string fullPath = noExtPath + ".spritedata";
                EntryPoint.Logger.LogInfo((object)$"[Export ({_currentIndex} / {_totalCount})] \tExport [{asset.name}] Sprite {fullPath}");
                PrepareDirectory(fullPath);
                SpriteData.ExportFromSprite(asset, fullPath);
                ExportTexture2D(asset.texture.name, asset.texture, noExtPath + textureExt);
            }
            else
            {
                EntryPoint.Logger.LogInfo((object)$"[Export ({_currentIndex} / {_totalCount})] \tExport skipped for packed sprite, exported with atlas.");
            }
        }

        private void ExportTexture2D(string assetName, Texture2D asset, string fullPath)
        {
            EntryPoint.Logger.LogInfo((object)$"[Export ({_currentIndex} / {_totalCount})] \tExport [{asset.name}] Texture2D {fullPath}");
            PrepareDirectory(fullPath);
            Syldra.Functions.ExportTexture(asset, fullPath);
        }

        private void ExportBinary(TextAsset asset, string fullPath)
        {
            EntryPoint.Logger.LogInfo((object)$"[Export ({_currentIndex} / {_totalCount})] \tExport [{asset.name}] System.Byte[] {fullPath}");
            PrepareDirectory(fullPath);
            File.WriteAllBytes(fullPath,asset.bytes);
        }

        private void ExportText(TextAsset asset, string fullPath)
        {
            EntryPoint.Logger.LogInfo((object)$"[Export ({_currentIndex} / {_totalCount})] \tExport [{asset.name}] TextAsset {fullPath}");
            PrepareDirectory(fullPath);
            File.WriteAllText(fullPath,asset.text);
        }
        private void ExportKeysDict(Dictionary<string,string> dict,string fullPath)
        {
            PrepareDirectory(fullPath);
            JsonDict jDict = new JsonDict(dict);
            File.WriteAllText(fullPath, JsonHandling.ToJson(jDict,true));
        }
        private static void PrepareDirectory(String fullPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        }
        public void OnExportError(string message,Exception ex)
        {
            EntryPoint.Logger.LogInfo((object)$"{message}: {ex}");
            Destroy(this);
        }

        public void OnDisable()
        {
            try
            {
                EntryPoint.Logger.LogInfo((object)$"Export stopped.");
                if (_exportEnabled)
                    Application.Quit();
            }
            catch (Exception ex)
            {
                OnExportError("Export failed during ending",ex);
            }
        }

        //TODO: Replace this with Catalog hooks
        public String GetFileExtension(String assetAddress)
        {
            string _catalogJson = File.ReadAllText(Application.streamingAssetsPath + @"/aa/catalog.json");
            Int32 index = _catalogJson.IndexOf(assetAddress);
            if (index < 0)
                return String.Empty;

            Int32 lastIndex = index + assetAddress.Length;
            if (_catalogJson.Length <= lastIndex)
                return String.Empty;

            if (_catalogJson[lastIndex] != '.')
                return String.Empty;

            Int32 quoteIndex = _catalogJson.IndexOf('"', lastIndex + 1);
            if (quoteIndex < 0)
                return String.Empty;

            string extension = _catalogJson.Substring(lastIndex, quoteIndex - lastIndex);
            if(extension == ".spriteatlas")
            {
                extension = ".atlas";
            }
            return extension;
        }
        public String GetAssetType(Il2CppSystem.Object asset)
        {
            String type = asset.GetIl2CppType().FullName;

            if (type == "UnityEngine.TextAsset")
            {
                TextAsset textAsset = asset.Cast<TextAsset>();
                if (textAsset.text == String.Empty && textAsset.bytes.Length != 0)
                    type = "System.Byte[]";
            }

            return type;
        }
        public void OnGUI()
        {
            GUI.skin.box.normal.background = _blackTexture;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);

            Single progress = (100.0f * _currentIndex) / _totalCount;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), $"Exporting ({progress:F2}%): {_currentIndex} / {_totalCount}", _guiStyle);
            GUI.Label(new Rect(0, 50, Screen.width, Screen.height), $"Group: {_currentGroup.Key}", _guiStyle);
        }
    }
}
