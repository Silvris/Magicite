using HarmonyLib;
using Last.Data.Master;

namespace Magicite
{
    //debug hook, used to look at each line of CSV as it is read
    //[HarmonyPatch(typeof(Monster),nameof(Monster.CreateMaster))]
    public sealed class Content_CreateMaster
    {
        public static void Prefix(string masterLine)
        {
            EntryPoint.Logger.LogInfo("CreateMaster");
            EntryPoint.Logger.LogInfo(masterLine);
            return;
        }
    }
}
