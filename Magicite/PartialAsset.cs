using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Magicite
{
    public interface PartialAsset
    {
        TextAsset ToAsset();
        void MergeData(TextAsset asset);
        void MergeAsset(PartialAsset asset);
    }
}
