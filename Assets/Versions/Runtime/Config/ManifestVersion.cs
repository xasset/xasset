using System;
using System.IO;
using UnityEngine;

namespace VEngine
{
    public class ManifestVersion : ScriptableObject
    {
        public uint crc;
        public long size;
        public int version;
        public string appVersion;

        public static ManifestVersion Load(string filename)
        {
            if (!File.Exists(filename)) return CreateInstance<ManifestVersion>();

            var json = File.ReadAllText(filename);
            var manifestVersion = CreateInstance<ManifestVersion>();
            try
            {
                JsonUtility.FromJsonOverwrite(json, manifestVersion);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                File.Delete(filename);
            }

            return manifestVersion;
        }
    }
}