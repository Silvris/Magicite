using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
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
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace Magicite
{
    [BepInPlugin("silvris.magicite", "Magicite Loader", "2.0.0.0")]
    [BepInProcess("FINAL FANTASY.exe")]
    [BepInProcess("FINAL FANTASY II.exe")]
    [BepInProcess("FINAL FANTASY III.exe")]
    [BepInProcess("FINAL FANTASY IV.exe")]
    [BepInProcess("FINAL FANTASY V.exe")]
    [BepInProcess("FINAL FANTASY VI.exe")]
    public class EntryPoint : BasePlugin
    {
        public static EntryPoint Instance { get; private set; }
        public static Configuration Configuration { get; private set; }
        public static ManualLogSource Logger { get; set; }
        public static AsyncOperationHandle LastHandle { get; set; }
        public override void Load()
        {
            try
            {

                Instance = this;
                Configuration = new Configuration();
                Logger = this.Log;
                Log.LogInfo("Loading...");
                if(Paths.BepInExVersion.Major < 6 |
                //don't check minor/patch because they're zero and should work going into later versions (or else I need to update anyways)
                //I don't know the exact version, but 500 *should* be good enough of a filter
                //this is gonna have to get updated eventually once IL2CPP gets a stable release
                Convert.ToInt32(Paths.BepInExVersion.PreRelease.Split('.')[1]) <= 500) throw new Exception($"BepInEx BE version too low to run this plugin! Current Version: Major:{Paths.BepInExVersion.Major} Minor:{Paths.BepInExVersion.Minor} Patch:{Paths.BepInExVersion.Patch} PreRelease:{Paths.BepInExVersion.PreRelease}");
                ClassInjector.RegisterTypeInIl2Cpp<ResourceCreator>(); //todo: make a more efficient method of injecting here (or move to BepInEx that auto-injects)
                ClassInjector.RegisterTypeInIl2Cpp<ResourceExporter>();
                String name = typeof(ResourceCreator).FullName;
                Log.LogInfo($"Initializing in-game singleton: {name}");
                GameObject singleton = new GameObject(name);
                singleton.hideFlags = HideFlags.HideAndDontSave;
                GameObject.DontDestroyOnLoad(singleton);
                Log.LogInfo("Adding ResourceCreator to singleton...");
                ResourceExporter exporter = singleton.AddComponent<ResourceExporter>();
                if (exporter is null)
                {
                    GameObject.Destroy(singleton);
                    throw new Exception($"The object is missing the required component: {name}");
                }
                ResourceCreator component = singleton.AddComponent<ResourceCreator>();
                if (component is null)
                {
                    GameObject.Destroy(singleton);
                    throw new Exception($"The object is missing the required component: {name}");
                }
                Assembly self = Assembly.GetExecutingAssembly();
                AssetBundle reqs = AssetBundle.LoadFromFile(Path.GetDirectoryName(self.Location) + "/Magicite.bundle");
                ResourceGeneration.DonorAssets = new Dictionary<string, UnityEngine.Object>();
                ResourceGeneration.DonorAssets.Add("SpriteAtlas", reqs.LoadAsset<SpriteAtlas>("Magicite/DonorAtlas.spriteatlas"));
                ResourceGeneration.DonorAssets["SpriteAtlas"].hideFlags = HideFlags.HideAndDontSave;
                reqs.Unload(false);
                PatchMethods();
            }
            catch(Exception ex)
            {
                Log.LogError(ex);
                Unload();
            }
        }
        private void PatchMethods()
        {
            try
            {
                Log.LogInfo("Patching methods...");
                Harmony harmony = new Harmony("silvris.magicite");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to patch methods.", ex);
            }
        }
    }
}
