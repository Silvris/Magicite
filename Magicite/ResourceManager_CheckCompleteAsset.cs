using HarmonyLib;
using Last.Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magicite
{
    [HarmonyPatch(typeof(ResourceManager),nameof(ResourceManager.CheckCompleteAsset))]
    public static class ResourceManager_CheckCompleteAsset
    {
        public static List<int> knownAssets = new List<int>();
        public static bool Prefix(string addressName, ResourceManager __instance, ref bool __result)
        {
            EntryPoint.Logger.LogInfo($"CheckCompleteAsset:{addressName}");
            if (ResourceCreator.OurFilePaths.ContainsKey(addressName))
            {
                string filePath = ResourceCreator.OurFilePaths[addressName];
                if (__instance.completeAssetDic.ContainsKey(addressName))
                {
                    if (!knownAssets.Contains(__instance.completeAssetDic[addressName].Cast<UnityEngine.Object>().GetInstanceID())){
                        __instance.completeAssetDic[addressName] = ResourceCreator.LoadAsset(filePath, Path.GetExtension(filePath));
                    }
                }
                else
                {
                    __instance.completeAssetDic.Add(addressName, ResourceCreator.LoadAsset(filePath, Path.GetExtension(filePath)));
                }
            }
            return true;//there is never a reason to return false here
            /*keeping in case I take the mode route
            //EntryPoint.Logger.LogInfo("ResourceManager.CheckCompleteAsset");
            if (ResourceCreator.loadedFiles.ContainsKey(addressName))
            {
                if (__instance.completeAssetDic.ContainsKey(addressName))
                {
                    if (!(__instance.completeAssetDic[addressName].Cast<UnityEngine.Object>().GetInstanceID() == ResourceCreator.loadedFiles[addressName].GetInstanceID()))
                    {
                        //EntryPoint.Logger.LogInfo(addressName);
                        __instance.completeAssetDic[addressName] = UnityEngine.Object.Instantiate(ResourceCreator.loadedFiles[addressName]);
                        return true;
                    }
                    else return true;
                }
                else
                {
                   // EntryPoint.Logger.LogInfo(addressName);
                    __instance.completeAssetDic.Add(addressName, UnityEngine.Object.Instantiate(ResourceCreator.loadedFiles[addressName]));
                    //__result = true;
                    return true;
                }
            }
            else
            {
                return true;
            }*/


        }
    }
}
