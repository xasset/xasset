using System.Collections.Generic;
using UnityEngine;

namespace VEngine
{
    public class PlayerSettings : ScriptableObject
    {
        public List<string> assets = new List<string>();
        public bool offlineMode;
    }
}