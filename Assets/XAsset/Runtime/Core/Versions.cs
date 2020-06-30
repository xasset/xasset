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
    public class FileVersion
    {
        public string name;
        public string hash;
        public long len;

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", name, hash, len);
        }

        public bool FromString(string s)
        {
            var fields = s.Split(',');
            if (fields.Length > 3)
            {
                name = fields[0];
                hash = fields[1];
                len = fields[2].UIntValue();
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

        public static VDisk disk { get; set; } 

        public static AssetBundle LoadAssetBundleFromFile(string url)
        {
            if (disk != null)
            {
                var name = Path.GetFileName(url);
                var file = disk.GetFile(name);
                if (file != null && file.off >= 0)
                {
                    return AssetBundle.LoadFromFile(disk.name, 0, (ulong) file.off);
                }
            }
            return AssetBundle.LoadFromFile(url); 
        }

        public static AssetBundleCreateRequest LoadAssetBundleFromFileAsync(string url)
        {
            if (disk != null)
            {
                var name = Path.GetFileName(url);
                var file = disk.GetFile(name);
                if (file != null && file.off >= 0)
                {
                    return AssetBundle.LoadFromFileAsync(disk.name, 0, (ulong) file.off);
                }
            }
            return AssetBundle.LoadFromFileAsync(url); 
        }

        public static void BuildVersions(string outputPath)
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
            var versions = new List<FileVersion>(); 
            var getFiles = Directory.GetFiles(outputPath, "*.unity3d");
            var vfd = new VDisk(dataPath); 
            foreach (var file in getFiles)
            {
                var dfile = vfd.AddFile(file);
                if (dfile == null)
                {
                    continue;
                }
                var version = new FileVersion {name = dfile.name, len = dfile.len, hash = dfile.hash};
                versions.Insert(0, version);
            } 
            vfd.Save();  
            using (var writer = new BinaryWriter(File.OpenWrite(path)))
            {
                writer.Write(versions.Count);
                foreach (var version in versions)
                {
                    writer.Write(version.name);
                    writer.Write(version.len);
                    writer.Write(version.hash);
                }
            }
        }

        public static List<FileVersion> LoadVersions(Stream s)
        {
            using (var reader = new BinaryReader(s))
            {
                var list = new List<FileVersion>();
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var version = new FileVersion
                    {
                        name = reader.ReadString(), len = reader.ReadUInt32(), hash = reader.ReadString()
                    };
                    list.Add(version);
                } 
                return list;
            }
        }

        public static List<FileVersion> LoadVersions(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                return LoadVersions(stream);
            }
        }

        public static bool IsNew(string path, long len, string hash)
        {
            if (disk != null)
            {
                var vdf = disk.GetFile(path);
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

                if (Versions.verifyBy == VerifyBy.Hash)
                {
                    if (Utility.GetCrc32Hash(stream).Equals(hash, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}