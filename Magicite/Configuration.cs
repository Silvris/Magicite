using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Magicite
{
    public class Configuration
    {
        //Import directory, how configurable should this be?
        //Does importing through resources even work with the main Resources directory?
        //seemingly not, which means hooks will be required for audio and sprite atlases
        public ConfigEntry<string> _ImportDirectory { get; set; }
        public string StreamingAssets = Application.streamingAssetsPath;
        public string DataPath = Application.dataPath;
        public string PersistentData = Application.persistentDataPath;
        public Configuration()
        {
            ConfigFile file = EntryPoint.Instance.Config;
            _ImportDirectory = file.Bind(new ConfigDefinition("Magicite Paths", "Import Directory"), "%StreamingAssets%/Magicite", new ConfigDescription("The import directory for custom asset files.\n\n Available replacements:\n%StreamingAssets% - StreamingAssets folder\n%DataPath% - \"FINAL FANTASY_Data\" folder\n%PersistentData% - \"AppData/LocalLow/SQUARE ENIX, Inc_/FINAL FANTASY\""));
        }
        public string ImportDirectory => _ImportDirectory.Value.Replace("%StreamingAssets%", StreamingAssets).Replace("%DataPath%", DataPath).Replace("%PersistentData%", PersistentData);
    }
}
