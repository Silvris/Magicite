using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Magicite
{
    public class JsonDict
    {
        public List<string> keys { get; set; }
        public List<string> values { get; set; }
        //this is the result of 4+ hours of fighting json libraries
        public JsonDict()
        {
            keys = new List<string>();
            values = new List<string>();
        }

        public void MergeDict(JsonDict donor)
        {
            foreach(string key in donor.keys)
            {
                if (!keys.Contains(key))
                {
                    keys.Add(key);
                    values.Add(donor.values[donor.keys.FindIndex(x => x == key)]);//if this fails..... I give up
                }
            }
        }
    }
    static class JsonHandling
    {
        public static JsonSerializerOptions options = new JsonSerializerOptions() { };
        public static JsonDict MergeJsonDictsInPath(string path,string group)
        {
            JsonDict baseFile = new JsonDict();
            foreach (string file in Directory.GetFiles(path))
            {

                JsonDict obj = JsonSerializer.Deserialize<JsonDict>(File.ReadAllText(file));
                if (obj != null)
                {
                    baseFile.MergeDict(obj);
                }
            }
            //EntryPoint.Instance.Log.LogInfo(group);
            return baseFile;
        }

        public static string ToJson(JsonDict obj)
        {
            string rtn = JsonSerializer.Serialize(obj); ;
            return rtn;
        }
    }
}
