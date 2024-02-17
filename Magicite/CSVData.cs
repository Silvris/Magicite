using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Magicite
{
    public class CSVData : PartialAsset
    {
        public string Name;
        public Dictionary<string, string> entries;
        public static Regex RegexTarget = new Regex("(\\d+) *,([\\s\\S]+)");
        public string key = "";

        public CSVData(string name, TextAsset asset)
        {
            string[] lines = asset.text.Split('\n');
            Name = name;
            entries = new Dictionary<string, string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    key = lines[i].Replace("\r","");
                }
                else
                {
                    AddToDict(lines[i]);
                }
            }
        }
        public CSVData(string name)
        {
            Name = name;
            entries = new Dictionary<string, string>();
        }
        private void AddToDict(string line)
        {
            line = line.Replace("\r", "");
            Match match = RegexTarget.Match(line);
            GroupCollection groups = match.Groups;
            string g1 = groups[1].Value;
            string g2 = groups[2].Value;
            if (g1.Equals(string.Empty)) return;
            if (g2.Equals(string.Empty)) return;
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
                if (i == 0)
                {
                    if(key != String.Empty)
                    {
                        if (lines[i].Replace("\r","") != key)
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    else
                    {
                        key = lines[i].Replace("\r", "");
                    }
                }
                else
                {
                    AddToDict(lines[i]);
                }
            }
        }

        public TextAsset ToAsset()
        {
            string output = key + "\r\n";
            foreach(KeyValuePair<string,string> kvp in entries)
            {
                output += kvp.Key + ",";
                output += kvp.Value + "\r\n";
            }
            return new TextAsset(output) { name = Name };
        }
        public void MergeAsset(PartialAsset asset)
        {
            if (!(asset is CSVData))
            {
                throw new NotImplementedException();
            }
            CSVData n = (CSVData)asset;
            if(n.key != key)
            {
                throw new InvalidOperationException($"Incorrect key - Original:{key} Merging:{n.key}");
            }
            foreach (KeyValuePair<string, string> kvp in n.entries)
            {
                if (entries.ContainsKey(kvp.Key))
                {
                    entries[kvp.Key] = kvp.Value;
                }
                else
                {
                    entries.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
