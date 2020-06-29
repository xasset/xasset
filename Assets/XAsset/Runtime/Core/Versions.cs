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

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace libx
{
    public class FileVersion
    {
        public string name;
        public uint crc;
        public long len;
        public long offset;

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", name, crc, len, offset);
        }

        public bool FromString(string s)
        {
            var fields = s.Split(',');
            if (fields.Length > 3)
            {
                name = fields[0];
                len = fields[2].UIntValue();
                crc = fields[1].UIntValue();
                offset = fields[3].UIntValue();
                return true;
            }
            else if (fields.Length > 2)
            {
                name = fields[0];
                len = fields[2].UIntValue();
                crc = fields[1].UIntValue();
                offset = 0;
                return true;
            }

            return false;
        }
    }

    public enum VerifyBy
    {
        Size,
        Hash,
    }

    public static class Versions
    {
        public const string Dataname = "data.bin";
        public const string Filename = "versions.unity3d";
        public const string BuildVersion = "build_version.unity3d";
        public static VerifyBy verifyBy = VerifyBy.Hash;
        private static readonly Dictionary<string, FileVersion> _data = new Dictionary<string, FileVersion>();
        public static bool useBin = false;

        private static FileVersion GetVersion(string name)
        {
            FileVersion ver;
            if (_data.TryGetValue(name, out ver))
            {
                return ver;
            }

            return null;
        }

        public static AssetBundle LoadAssetBundleFromFile(string url)
        {
            var name = Path.GetFileName(url);
            var version = GetVersion(name);
            if (useBin && version.offset >= 0)
            {
                var file = url.Replace(name, Versions.Dataname);
                return AssetBundle.LoadFromFile(file, 0, (ulong) version.offset);
            }
            else
            {
                return AssetBundle.LoadFromFile(url);
            }
        }

        public static AssetBundleCreateRequest LoadAssetBundleFromFileAsync(string url)
        {
            var name = Path.GetFileName(url);
            var version = GetVersion(name);
            if (useBin && version.offset >= 0)
            {
                var file = url.Replace(name, Versions.Dataname);
                return AssetBundle.LoadFromFileAsync(file, 0, (ulong) version.offset);
            }
            else
            {
                return AssetBundle.LoadFromFileAsync(url);
            }
        }

        public static void BuildVersions(string outputPath)
        {
            var path = outputPath + "/" + Filename;
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var binPath = outputPath + "/" + Dataname;
            if (File.Exists(binPath))
            {
                File.Delete(binPath);
            }

            var versions = new List<FileVersion>();
            var getFiles = Directory.GetFiles(outputPath, "*");
            using (var writer = new BinaryWriter(File.OpenWrite(binPath)))
            {
                var position = 0L;
                foreach (var file in getFiles)
                {
                    var name = Path.GetFileName(file);
                    var bytes = File.ReadAllBytes(file);
                    var len = bytes.Length;
                    var crc = Utility.GetCrc(bytes);
                    var version = new FileVersion {name = name, len = len, offset = position, crc = crc};
                    versions.Add(version);
                    position += len;
                    writer.Write(bytes);
                }

                writer.Flush();
            }

            {
                var bytes = File.ReadAllBytes(binPath);
                var crc = Utility.GetCrc(File.ReadAllBytes(binPath));
                var version = new FileVersion {name = Dataname, len = bytes.Length, offset = -1L, crc = crc};
                versions.Add(version);
            }

            using (var writer = new StreamWriter(File.OpenWrite(path)))
            {
                foreach (var record in versions)
                {
                    writer.WriteLine(record.ToString());
                }
            }
        }

        public static List<FileVersion> LoadVersions(Stream s)
        {
            _data.Clear();
            using (var reader = new StreamReader(s))
            {
                var records = new List<FileVersion>();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    var record = new FileVersion();
                    if (!record.FromString(line))
                    {
                        continue;
                    }

                    records.Add(record);
                    _data[record.name] = record;
                }

                return records;
            }
        }

        public static List<FileVersion> LoadVersions(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                return LoadVersions(stream);
            }
        }
    }
}