using Last.Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Magicite
{
    public class AsyncSpriteExport
    {
        public AsyncOperationHandle<Sprite> handle { get; set; }
        private string AssetPath { get; set; }
        private string AssetGroup { get; set; }
        private string SpriteDataExt = ".spritedata";
        private string _exportDirectory = EntryPoint.Configuration.ExportDirectory;
        private bool IsExportFinish { get; set; }
        public AsyncSpriteExport(string assetPath, string assetGroup)
        {
            //assetPath is extensionless, meaning only assetGroup and assetPath need to be passed in
            handle = AddressableAssetWrapper.LoadAssetSprite(assetPath);
            AssetPath = assetPath;
            AssetGroup = assetGroup;
            IsExportFinish = false;
            EntryPoint.LastHandle = handle;
        }
        public void Update()
        {
            //EntryPoint.Logger.LogInfo($"IsExportFinish:{IsExportFinish}");
            if (!IsExportFinish)
            {
                if (handle.DebugName != "InvalidHandle")
                {
                    if (!handle.IsDone)
                    {
                        //EntryPoint.Logger.LogInfo($"assetPath:{AssetPath} PercentComplete:{handle.PercentComplete}");
                        return;
                    }
                    //operation is done, check for export
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Sprite asset = handle.Result;
                        SpriteData.ExportFromSprite(asset, Path.Combine(_exportDirectory, AssetGroup, AssetPath + SpriteDataExt));
                        //handle.Release();
                        EntryPoint.Logger.LogInfo($"Async sprite export succeeded for {AssetPath}: Status:{handle.Status}");
                        IsExportFinish = true;
                    }
                    else
                    {
                        EntryPoint.Logger.LogInfo($"Async sprite export failed for {AssetPath}: Status:{handle.Status}");
                        //handle.Release();
                        IsExportFinish = true;
                    }
                }
            }
        }
        public bool IsDone()
        {
            return IsExportFinish;
        }
    }
}
