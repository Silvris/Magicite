using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Magicite
{
    //[HarmonyPatch(typeof(TextAsset))]
    //[HarmonyPatch("~TextAsset")]
    public sealed class TextAsset_dtorPatch
    {
        public static void Prefix(TextAsset __instance)
        {
            BinaryAssetManager.Instance.Remove(__instance.name);
        }
    }
}
