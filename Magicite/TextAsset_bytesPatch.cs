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
        public static void Postfix(ref Il2CppStructArray<byte> __result)
        {
            if(__result.Length > 4)
            {
                string data = Encoding.UTF8.GetString(__result);
                if (data.StartsWith("MAGI"))
                {
                    string path = data.Replace("MAGI", "");
                    __result = File.ReadAllBytes(path);
                }
            }
        }
    }
}
