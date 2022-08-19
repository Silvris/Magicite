using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;

namespace Magicite
{
    [HarmonyPatch(typeof(TextAsset))]
    [HarmonyPatch(nameof(TextAsset.bytes),MethodType.Getter)]
    public sealed class TextAsset_bytesPatch
    {
        public static void Postfix(ref Il2CppStructArray<byte> __result, TextAsset __instance)
        {
            if(__result.Length > 0)
            {
                //EntryPoint.Logger.LogInfo("TextAsset.bytes.getter");
                if (__result[0] == 'M')
                {
                    if (__result[2] == 'G')//these two ifs make it so only MAGI files get pushed into the more intensive checks
                    {
                        if (__result.Length > 4)
                        {
                            string data = Encoding.UTF8.GetString(__result);
                            if (data.StartsWith("MAGI"))
                            {
                                string path = data.Replace("MAGI", "");
                                __result = Il2CppSystem.IO.File.ReadAllBytes(path);

                            }
                        }
                    }
                }
            }
        }
    }
}
