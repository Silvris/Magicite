using HarmonyLib;
using Last.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magicite
{
    [HarmonyPatch(typeof(ResourceManager),nameof(ResourceManager.CheckCompleteAsset))]
    class ResourceManager_CheckCompleteAsset
    {
        public static bool Prefix(string addressName, ResourceManager __instance, ref bool __result)
        {
            if (ResourceCreator.loadedFiles.ContainsKey(addressName))
            {
                if (__instance.completeAssetDic.ContainsKey(addressName))
                {
                    if (!(__instance.completeAssetDic[addressName].Cast<UnityEngine.Object>().GetInstanceID() == ResourceCreator.loadedFiles[addressName].GetInstanceID()))
                    {
                        //EntryPoint.Instance.Log.LogInfo(addressName);
                        __instance.completeAssetDic[addressName] = ResourceCreator.loadedFiles[addressName];
                        return true;
                    }
                    else return true;
                }
                else
                {
                   // EntryPoint.Instance.Log.LogInfo(addressName);
                    __instance.completeAssetDic.Add(addressName, ResourceCreator.loadedFiles[addressName]);
                    __result = true;
                    return true;
                }
            }
            else
            {
                return true;
            }

        }
    }
}
