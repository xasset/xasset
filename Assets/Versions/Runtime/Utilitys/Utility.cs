using System.IO;
using UnityEngine;

namespace Versions
{
    public static class Utility
    {
        public const string buildPath = "Bundles";

        public const string unsupportedPlatform = "Unsupported";

        private static readonly double[] byteUnits =
        {
            1073741824.0, 1048576.0, 1024.0, 1
        };

        private static readonly string[] byteUnitsNames =
        {
            "GB", "MB", "KB", "B"
        };

        public static string GetPlatformName()
        {
            if (Application.platform == RuntimePlatform.Android) return "Android";

            if (Application.platform == RuntimePlatform.WindowsPlayer) return "Windows";

            if (Application.platform == RuntimePlatform.IPhonePlayer) return "iOS";

            return Application.platform == RuntimePlatform.WebGLPlayer ? "WebGL" : unsupportedPlatform;
        }

        public static string FormatBytes(long bytes)
        {
            var size = "0 B";
            if (bytes == 0) return size;

            for (var index = 0; index < byteUnits.Length; index++)
            {
                var unit = byteUnits[index];
                if (bytes >= unit)
                {
                    size = $"{bytes / unit:##.##} {byteUnitsNames[index]}";
                    break;
                }
            }

            return size;
        }

        public static uint ComputeCRC32(Stream stream)
        {
            var crc32 = new CRC32();
            return crc32.Compute(stream);
        }

        public static uint ComputeCRC32(string filename)
        {
            if (!File.Exists(filename)) return 0;

            using (var stream = File.OpenRead(filename))
            {
                return ComputeCRC32(stream);
            }
        }
    }
}