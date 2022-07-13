using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magicite
{
    public enum BinaryAsyncStatus
    {
        None,
        Awaiting,
        Success,
        Failed
    }
    public class BinaryAsset
    {
        private string _Name;
        private string _Path;
        private byte[] _Data;
        public string Name { get => _Name; set => _Name = value; }
        public string Path { get => _Path; set => _Path = value; }
        public byte[] Data { get => _Data; set => _Data = value; }
        public BinaryAsyncStatus Status = BinaryAsyncStatus.None;

        public BinaryAsset(string name, string path)
        {
            Status = BinaryAsyncStatus.Awaiting;
            Name = name;
            Path = path;
            Task.Run(async () => await LoadFile());
        }

        public async Task LoadFile()
        {
            try
            {
                await LoadFileAsync();
                Status = BinaryAsyncStatus.Success;
            }
            catch(Exception ex)
            {
                EntryPoint.Logger.LogError($"Error loading binary asset:{ex}");
                Status = BinaryAsyncStatus.Failed;
            }
        }

        async Task LoadFileAsync()
        {
            FileStream stream = new FileStream(Path, FileMode.Open,FileAccess.Read,FileShare.Read,4096,useAsync:true);
            Data = new byte[stream.Length];
            await stream.ReadAsync(Data, 0, (int)stream.Length);
        }

    }
    public class BinaryAssetManager
    {
        private Dictionary<string, BinaryAsset> binaryData { get; set; }
        public static BinaryAssetManager Instance = new BinaryAssetManager();
        private BinaryAssetManager()
        {
            binaryData = new Dictionary<string, BinaryAsset>();
        }

        public bool Register(string name, string path)
        {
            if(name != String.Empty)
            {
                if (!binaryData.ContainsKey(name))
                    binaryData.Add(name, new BinaryAsset(name, path));
                //else binaryData[name] = new BinaryAsset(name, path);
                return true;
            }
            else
            {
                return false;
            }
        }

        public BinaryAsset GetBinary(string name)
        {
            if (binaryData.ContainsKey(name))
            {
                return binaryData[name];
            }
            else
            {
                return null;
            }
        }

        public void Remove(string name)
        {
            if (binaryData.ContainsKey(name))
            {
                binaryData.Remove(name);
            }
        }
    }
}
