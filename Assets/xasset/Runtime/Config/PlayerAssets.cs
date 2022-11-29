using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    public class PlayerAssets : ScriptableObject
    {
        public static readonly string Filename = $"{nameof(PlayerAssets)}.json";
        public List<string> data = new List<string>();

        public bool Contains(string key)
        {
            return data.Contains(key);
        }
    }
}