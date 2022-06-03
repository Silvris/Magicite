using HarmonyLib;
using Last.Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Magicite
{
    [HarmonyPatch(typeof(ResourceManager),nameof(ResourceManager.IsLoadAssetCompleted),new Type[] { typeof(string)})]
    [HarmonyAfter("Memoria.FFPR")]//always run after Memoria's, this *probably* does nothing since Memoria hits a different hook
    //but changing to a postfix probably forces it to run after now lol
    public static class ResourceManager_IsLoadAssetCompleted
    {
        public static List<int> knownAssets = new List<int>();
        public static void Postfix(string addressName, ResourceManager __instance)
        {
            if(true)
            {
                //EntryPoint.Logger.LogInfo($"IsLoadAssetCompleted:{addressName}");
                if (ResourceCreator.OurFilePaths.ContainsKey(addressName))
                {
                    string filePath = ResourceCreator.OurFilePaths[addressName];
                    //EntryPoint.Logger.LogInfo(filePath);
                    string ext = Path.GetExtension(filePath);
                    //EntryPoint.Logger.LogInfo(ext);
                    bool isPartial = (ext == ".csv" || ext == ".txt");
                    if (isPartial)
                    {
                        //EntryPoint.Logger.LogInfo($"filePath:{filePath}");
                        if (__instance.completeAssetDic.ContainsKey(addressName))
                        {
                            //EntryPoint.Logger.LogInfo("ContainsKey");
                            if (!knownAssets.Contains(__instance.completeAssetDic[addressName].Cast<UnityEngine.Object>().GetInstanceID()))
                            {
                                //EntryPoint.Logger.LogInfo("!KnownAssets");
                                UnityEngine.Object asset = ResourceCreator.LoadAsset(filePath, Path.GetExtension(filePath), __instance.completeAssetDic[addressName]);
                                __instance.completeAssetDic[addressName] = asset;
                                knownAssets.Add(asset.GetInstanceID());
                            }
                            else
                            {
                                //EntryPoint.Logger.LogInfo("KnownAssets.Contains");
                                //TextAsset asset = __instance.completeAssetDic[addressName].Cast<TextAsset>();
                                //EntryPoint.Logger.LogInfo(asset.name);
                                //EntryPoint.Logger.LogInfo(asset.text);
                            }
                        }
                    }
                }

            }
            //there is never a reason to return false here
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

