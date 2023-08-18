using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace xasset
{
    public class Files
    {
        private static readonly Dictionary<string, string> addressWithPaths = new Dictionary<string, string>();

        public static string dirPath { get; set; }

        public static FileRequest Load(string path, FileReadMode readMode)
        {
            var request = LoadAsync(path, readMode);
            request?.WaitForCompletion();
            return request;
        }

        public static FileRequest LoadAsync(string path, FileReadMode readMode)
        {
            return FileRequest.Load($"{dirPath}/{path}", readMode);
        }

        internal static FileAsset GetFileAsset(FileRequest request, TextAsset textAsset)
        {
            FileAsset fileAsset = new FileAsset();
            if (textAsset == null)
            {
                fileAsset.error = $"{request.path} get TextAsset failed";
                return fileAsset;
            }

            switch (request.readMode)
            {
                case FileReadMode.Bytes:
                    fileAsset.bytes = textAsset.bytes;
                    break;
                default:
                    fileAsset.content = textAsset.text;
                    break;
            }

            return fileAsset;
        }

        public static void SetAddress(string assetPath, string address)
        {
            if (!addressWithPaths.TryGetValue(address, out var value))
            {
                addressWithPaths[address] = assetPath;
            }
            else
            {
                if (assetPath != value)
                    Logger.W($"Failed to set address for {assetPath},because the address:{address} already existed");
            }
        }

        private static void GetActualPath(ref string path)
        {
            if (addressWithPaths.TryGetValue(path.ToLower(), out var value)) path = value;
        }

        internal static bool TryGetFile(ref string path, out ManifestAsset asset)
        {
            GetActualPath(ref path);
            if (Assets.RealtimeMode) return Assets.Versions.TryGetAsset(path, out asset);
            asset = null;
            return File.Exists(path);
        }
    }
}