using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Magicite
{
    public class TXTData : PartialAsset
    {
        public string Name;
        public Dictionary<string, string> entries;
        public static Regex RegexTarget = new Regex("(.+)\t(.*)");
        public string key = "";

        public TXTData(string name, TextAsset asset)
        {
            Name = name;
            entries = new Dictionary<string, string>();
            string[] lines = asset.text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                    AddToDict(lines[i]);
            }
        }
        public TXTData(string name)
        {
            Name = name;
            entries = new Dictionary<string, string>();
        }
        private void AddToDict(string line)
        {
            //EntryPoint.Logger.LogInfo(line);
            line = line.Replace("\r", "");
            Match match = RegexTarget.Match(line);
            GroupCollection groups = match.Groups;
            string g1 = groups[1].Value;
            string g2 = groups[2].Value;
            //EntryPoint.Logger.LogInfo(g1);
            //EntryPoint.Logger.LogInfo(g2);
            if (g1.Equals(string.Empty)) return;
            if (entries.ContainsKey(g1))
            {
                entries[g1] = g2;
            }
            else
            {
                entries.Add(g1, g2);
            }
        }
        public void MergeData(TextAsset asset)
        {
            string[] lines = asset.text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                    AddToDict(lines[i]);
            }
        }

        public TextAsset ToAsset()
        {
            string output = String.Empty;
            foreach (KeyValuePair<string, string> kvp in entries)
            {
                output += kvp.Key + "\t";
                output += kvp.Value + "\r\n";
            }
            return new TextAsset(output) { name = Name };
        }

        public void MergeAsset(PartialAsset asset)
        {
            if(!(asset is TXTData))
            {
                throw new NotImplementedException();
            }
            TXTData n = (TXTData)asset;
            foreach(KeyValuePair<string,string> kvp in n.entries)
            {
                if (entries.ContainsKey(kvp.Key))
                {
                    entries[kvp.Key] = kvp.Value;
                }
                else
                {
                    entries.Add(kvp.Key,kvp.Value);
                }
            }
        }
    }
}
