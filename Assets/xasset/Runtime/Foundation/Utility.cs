using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace xasset
{
    public static class Utility
    {
        private static readonly double[] byteUnits = {1073741824.0, 1048576.0, 1024.0, 1};

        private static readonly string[] byteUnitsNames = {"GB", "MB", "KB", "B"};

        public static string FormatBytes(ulong bytes)
        {
            var size = "0 B";
            if (bytes == 0) return size;

            for (var index = 0; index < byteUnits.Length; index++)
            {
                var unit = byteUnits[index];
                if (!(bytes >= unit)) continue;

                size = $"{bytes / unit:##.##} {byteUnitsNames[index]}";
                break;
            }

            return size;
        }

        public static T LoadFromFile<T>(string filename) where T : ScriptableObject
        {
            if (!File.Exists(filename)) return ScriptableObject.CreateInstance<T>();

            var json = File.ReadAllText(filename);
            var asset = ScriptableObject.CreateInstance<T>();
            try
            {
                JsonUtility.FromJsonOverwrite(json, asset);
            }
            catch (Exception e)
            {
                Logger.E(e.Message);
                File.Delete(filename);
            }

            return asset;
        }

        public static T LoadFromJson<T>(string json) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            try
            {
                JsonUtility.FromJsonOverwrite(json, asset);
            }
            catch (Exception e)
            {
                Logger.E(e.Message);
                return null;
            }

            return asset;
        }

        public static void CreateDirectoryIfNecessary(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir) || Directory.Exists(dir)) return;

            Directory.CreateDirectory(dir);
        }

        public static string ToHash(byte[] data)
        {
            var sb = new StringBuilder();
            foreach (var t in data) sb.Append(t.ToString("x2"));

            return sb.ToString();
        }

        public static string ComputeHash(byte[] bytes)
        {
            var data = MD5.Create().ComputeHash(bytes);
            return ToHash(data);
        }

        public static string ComputeHash(string filename)
        {
            if (!File.Exists(filename)) return string.Empty;

            using (var stream = File.OpenRead(filename))
            {
                return ToHash(MD5.Create().ComputeHash(stream));
            }
        }

        public static string GetProtocol()
        {
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.IPhonePlayer) return "file://";

            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
                return "file:///";

            return string.Empty;
        }

        public static Platform GetPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return Platform.Android;
                case RuntimePlatform.WindowsPlayer:
                    return Platform.Windows;
                case RuntimePlatform.OSXPlayer:
                    return Platform.OSX;
                case RuntimePlatform.IPhonePlayer:
                    return Platform.iOS;
                case RuntimePlatform.WebGLPlayer:
                    return Platform.WebGL;
                case RuntimePlatform.LinuxPlayer:
                    return Platform.Linux;
                default:
                    return Platform.Default;
            }
        }
    }
}