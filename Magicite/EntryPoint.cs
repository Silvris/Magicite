using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.U2D;

namespace Magicite
{
    [BepInPlugin("silvris.magicite", "Magicite Loader", "1.0.0.0")]
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
        public override void Load()
        {
            Instance = this;
            Configuration = new Configuration();
            Log.LogInfo("Loading...");
            ClassInjector.RegisterTypeInIl2Cpp<ResourceCreator>(); //todo: make a more efficient method of injecting here (or move to BepInEx that auto-injects)
            String name = typeof(ResourceCreator).FullName;
            Log.LogInfo($"Initializing in-game singleton: {name}");
            GameObject singleton = new GameObject(name);
            singleton.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(singleton);
            Log.LogInfo("Adding ModComponent to singleton...");
            ResourceCreator component = singleton.AddComponent<ResourceCreator>();
            if (component is null)
            {
                GameObject.Destroy(singleton);
                throw new Exception($"The object is missing the required component: {name}");
            }
            PatchMethods();
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
