//
// Versions.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace libx
{
    public enum VerifyBy
    {
        Size,
        Hash,
    }

    public static class Versions
    {
        public const string Dataname = "res";
        public const string Filename = "ver";
        public static VerifyBy verifyBy = VerifyBy.Size;

        private static VDisk _disk = new VDisk();
        private static Dictionary<string, VFile> updateData = new Dictionary<string, VFile>();
        public static Dictionary<string, VFile> baseData = new Dictionary<string, VFile>();


        public static AssetBundle LoadAssetBundleFromFile(string url)
        {
            if (_disk != null)
            {
                var name = Path.GetFileName(url);
                var file = _disk.GetFile(name, string.Empty);
                if (file != null)
                {
                    return AssetBundle.LoadFromFile(_disk.name, 0, (ulong) file.offset);
                }
            }

            return AssetBundle.LoadFromFile(url);
        }

        public static AssetBundleCreateRequest LoadAssetBundleFromFileAsync(string url)
        {
            if (_disk != null)
            {
                var name = Path.GetFileName(url);
                var file = _disk.GetFile(name, string.Empty);
                if (file != null)
                {
                    return AssetBundle.LoadFromFileAsync(_disk.name, 0, (ulong) file.offset);
                }
            }

            return AssetBundle.LoadFromFileAsync(url);
        }

        public static void BuildVersions(string outputPath, int version)
        {
            var path = outputPath + "/" + Filename;
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var dataPath = outputPath + "/" + Dataname;
            if (File.Exists(dataPath))
            {
                File.Delete(dataPath);
            }

            var getFiles = Directory.GetFiles(outputPath, "*.unity3d");
            var vd = new VDisk(dataPath);
            foreach (var file in getFiles)
            {
                vd.AddFile(file, string.Empty);
            }

            vd.Save();

            using (var stream = File.OpenWrite(path))
            {
                var writer = new BinaryWriter(stream);
                writer.Write(version);
                writer.Write(vd.files.Count + 1);
                using (var fs = File.OpenRead(dataPath))
                {
                    var file = new VFile {name = Dataname, len = fs.Length, hash = Utility.GetCRC32Hash(fs)};
                    file.Serialize(writer);
                }

                foreach (var file in vd.files)
                {
                    file.Serialize(writer);
                }
            }
        }

        public static int LoadVersion(string filename)
        {
            if (!File.Exists(filename)) return -1;
            using (var stream = File.OpenRead(filename))
            {
                var reader = new BinaryReader(stream);
                return reader.ReadInt32();
            }
        }

        public static List<VFile> LoadVersions(string filename, bool update = false)
        {
            var data = update ? updateData : baseData;
            data.Clear();
            using (var stream = File.OpenRead(filename))
            {
                var reader = new BinaryReader(stream);
                var list = new List<VFile>();
                var verion = reader.ReadInt32();
                Debug.Log("LoadVesions:" + verion);
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var version = new VFile();
                    version.Deserialize(reader);
                    list.Add(version);
                    data[version.name] = version;
                }

                return list;
            }
        }

        public static bool LoadDisk(string filename)
        {
            return _disk.Load(filename);
        }

        public static void OnFileDownload(Download download, string savePath)
        {
            if (_disk.Exists())
            {
                _disk.AddFile(download.tempPath, download.hash);
            }
            else
            {
                var name = Path.GetFileName(download.url);
                var path = string.Format("{0}{1}", savePath, name);
                File.Copy(download.tempPath, path, true);
                if (name.Equals(Dataname))
                {
                    _disk.Load(path);
                }
            }

            File.Delete(download.tempPath);
        }

        public static bool IsNew(string path, long len, string hash)
        {
            VFile file;
            var key = Path.GetFileName(path);
            if (baseData.TryGetValue(key, out file))
            {
                if (key.Equals(Dataname) ||
                    file.len == len && file.hash.Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            if (_disk.Exists())
            {
                var vdf = _disk.GetFile(path, hash);
                if (vdf != null && vdf.len == len && vdf.hash.Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            if (!File.Exists(path))
            {
                return true;
            }

            using (var stream = File.OpenRead(path))
            {
                if (stream.Length != len)
                {
                    return true;
                }

                if (verifyBy != VerifyBy.Hash) return false;
                return !Utility.GetCRC32Hash(stream).Equals(hash, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}