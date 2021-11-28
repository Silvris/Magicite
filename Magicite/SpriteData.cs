using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Magicite
{
    public class SpriteData
    {
        public bool hasRect = false;
        public bool hasPivot = false;
        public bool hasBorder = false;
        public bool hasPPU = false;
        public bool hasWrap = false;
        public string name = "";
        public Rect rect;
        public Vector2 pivot;
        public Vector4 border;
        public Single pixelsPerUnit;
        public TextureWrapMode wrapMode;

        public SpriteData(string[] strings, string Name)
        {
            name = Name;
            rect = new Rect();
            pivot = new Vector2();
            border = new Vector4();
            wrapMode = TextureWrapMode.Clamp;

            foreach(string datatype in strings)
            {
                
                List<string> kvp = new List<string>(datatype.Split('='));
                foreach(string v in kvp)
                {
                    v.Trim();
                }
                if(kvp.Count != 2)
                {
                    EntryPoint.Instance.Log.LogWarning($"SpriteData [{name}]: Invalid entry (unable to distinguish key)");
                    return;
                }
                EntryPoint.Instance.Log.LogInfo(kvp[0].ToLower());
                switch (kvp[0].ToLower())
                {
                    case "rect":
                        SetRect(kvp[1]);
                        break;
                    case "pivot":
                        SetPivot(kvp[1]);
                        break;
                    case "border":
                        SetBorder(kvp[1]);
                        break;
                    case "pixelsperunit":
                        SetPPU(kvp[1]);
                        break;
                    case "wrapmode":
                        SetWrap(kvp[1]);
                        break;
                    default:
                        EntryPoint.Instance.Log.LogWarning($"SpriteData [{name}]: Unknown key \"{kvp[0]}\"");
                        break;

                }
            }
        }

        public void SetRect(string input)
        {
            string[] vals = input.Replace("[","").Replace("]","").Split(',');
            if(vals.Length != 4)
            {
                EntryPoint.Instance.Log.LogWarning($"SpriteData [{name}]: Invalid rect length. Expected 4, got {vals.Length}.");
                return;
            }
            rect.x = Convert.ToSingle(vals[0]);
            rect.y = Convert.ToSingle(vals[1]);
            rect.width = Convert.ToSingle(vals[2]);
            rect.height = Convert.ToSingle(vals[3]);
            //ModComponent.Log.LogInfo($"{rect.x} {rect.y} {rect.width} {rect.height}");
            hasRect = true;
        }
        public void SetPivot(string input)
        {
            string[] vals = input.Replace("[", "").Replace("]", "").Split(',');
            if (vals.Length != 2)
            {
                EntryPoint.Instance.Log.LogInfo($"SpriteData [{name}]: Invalid pivot length. Expected 2, got {vals.Length}.");
                return;
            }
            pivot.x = Convert.ToSingle(vals[0]);
            pivot.y = Convert.ToSingle(vals[1]);
            //ModComponent.Log.LogInfo($"{pivot.x} {pivot.y}");
            hasPivot = true;
        }
        public void SetBorder(string input)
        {
            string[] vals = input.Replace("[", "").Replace("]", "").Split(',');
            if (vals.Length != 4)
            {
                EntryPoint.Instance.Log.LogInfo($"SpriteData [{name}]: Invalid border length. Expected 4, got {vals.Length}.");
                return;
            }
            border.x = Convert.ToSingle(vals[0]);
            border.y = Convert.ToSingle(vals[1]);
            border.z = Convert.ToSingle(vals[2]);
            border.w = Convert.ToSingle(vals[3]);
            //ModComponent.Log.LogInfo($"{border.x} {border.y} {border.z} {border.w}");
            hasBorder = true;
        }
        public void SetPPU(string input)
        {
            pixelsPerUnit = Convert.ToSingle(input);
            hasPPU = true;
        }
        public void SetWrap(string input)
        {
            switch (input.ToLower())//so we don't have to check for capitalization
            {
                case "clamp":
                    wrapMode = TextureWrapMode.Clamp;
                    hasWrap = true;
                    break;
                case "repeat":
                    wrapMode = TextureWrapMode.Repeat;
                    hasWrap = true;
                    break;
                case "mirror":
                    wrapMode = TextureWrapMode.Mirror;
                    hasWrap = true;
                    break;
                case "mirroronce":
                    wrapMode = TextureWrapMode.MirrorOnce;
                    hasWrap = true;
                    break;
                default:
                    EntryPoint.Instance.Log.LogInfo($"SpriteData [{name}]: Invalid wrap mode: {input}.");
                    break;
            }
        }
    }
}
