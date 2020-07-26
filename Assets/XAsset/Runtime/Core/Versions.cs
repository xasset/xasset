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
        Hash
    }

    public enum PatchId
    {
        Level1,
        Level2,
        Level3,
        Level4,
        Level5
    }

    public class VPatch
    {
        public PatchId id; 
        public List<int> files = new List<int>();
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)id); 
            writer.Write(files.Count);
            foreach (var file in files)
            {
                writer.Write(file);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            id = (PatchId)reader.ReadByte();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var file = reader.ReadInt32();
                files.Add(file);
            }
        }
    }

    public class VFile
    {
        public string hash { get; set; }
        public long len { get; set; }
        public string name { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(len);
            writer.Write(hash);
        }

        public void Deserialize(BinaryReader reader)
        {
            name = reader.ReadString();
            len = reader.ReadInt64();
            hash = reader.ReadString();
        }
    }

    public class Version
    {
        public int ver;
        public List<VFile> files = new List<VFile>();
        public List<VPatch> patches = new List<VPatch>();
        
        private Dictionary<string, VFile> _dataFiles = new Dictionary<string, VFile>();
        private Dictionary<PatchId, VPatch> _dataPatches = new Dictionary<PatchId, VPatch>();

        public VFile GetFile(string path)
        {
            _dataFiles.TryGetValue(path, out var file);
            return file;
        }
        
        public List<VFile> GetFiles(PatchId patchId)
        {
            List<VFile> list = new List<VFile>();
            VPatch patch;
            if (_dataPatches.TryGetValue(patchId, out patch))
            {
                if (patch.files.Count > 0)
                {
                    foreach (var file in patch.files)
                    {
                        var item = files[file];
                        list.Add(item); 
                    }
                }
            } 
            return list;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ver);
            
            writer.Write(files.Count);
            foreach (var file in files) 
                file.Serialize(writer);
            
            writer.Write(patches.Count);
            foreach (var patch in patches)
            {
                writer.Write((byte)patch.id); 
                writer.Write(patch.files.Count);
                foreach (var bundleId in patch.files)
                {
                    writer.Write(bundleId);
                }
            }
        }
        
        public void Deserialize(BinaryReader reader)
        {
            ver = reader.ReadInt32();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var file = new VFile();
                file.Deserialize(reader);
                files.Add(file);
                _dataFiles[file.name] = file;
            } 
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var patch = new VPatch();
                patch.Deserialize(reader);
                patches.Add(patch);
                _dataPatches[patch.id] = patch;
            }
        }
    }

    public static class Versions
    {
        public const string Filename = "ver";
        public static readonly VerifyBy verifyBy = VerifyBy.Hash;
        public static Version serverVersion { get; set; }
        public static Version localVersion { get; set; }
        
        public static int LoadVersion(string filename)
        {
            if (!File.Exists(filename))
                return -1;
            try
            {
                using (var stream = File.OpenRead(filename))
                {
                    var reader = new BinaryReader(stream);
                    return reader.ReadInt32();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return -1;
            }
        }
        
        public static Version LoadFullVersion(string filename)
        { 
            using (var stream = File.OpenRead(filename))
            {
                var reader = new BinaryReader(stream);
                var ver = new Version();
                ver.Deserialize(reader);
                return ver;
            }
        }
        
        public static void BuildVersion(string outputPath, List<BundleRef> bundles, List<VPatch> patches, int version)
        {
            var path = outputPath + "/" + Filename;
            if (File.Exists(path)) File.Delete(path);
            var files = new List<VFile>();
            foreach (var bundle in bundles)
            {
                files.Add(new VFile()
                {
                    name = bundle.name,
                    hash = bundle.crc,
                    len = bundle.len,
                });
            }
            
            patches.Sort((x, y) => x.id.CompareTo(y.id));
            if (patches.Count > 0)
            {
                patches[0].files.Add(bundles.Count - 1);
            }
            
            var ver = new Version();
            ver.ver = version;
            ver.files = files;
            ver.patches = patches; 
            
            using (var stream = File.OpenWrite(path))
            {
                var writer = new BinaryWriter(stream);
                ver.Serialize(writer);
            }
        } 
 
        public static bool IsNew(string path, long len, string hash)
        { 
            if (!File.Exists(path)) return true;

            if (localVersion != null)
            {
                var key = Path.GetFileName(path); 
                var file = localVersion.GetFile(key); 
                if (file != null && 
                    file.len == len && 
                    file.hash.Equals(hash, StringComparison.OrdinalIgnoreCase))  
                    return false;
            } 
            
            using (var stream = File.OpenRead(path))
            {
                if (stream.Length != len) return true;
                if (verifyBy != VerifyBy.Hash)
                    return false;
                return !Utility.GetCRC32Hash(stream).Equals(hash, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static List<VFile> GetNewFiles(PatchId patch, string savePath)
        {
            var list = new List<VFile>();
            var files = serverVersion.GetFiles(patch);
            foreach (var file in files)
            {
                if (IsNew(savePath + file.name, file.len, file.hash))
                {
                    list.Add(file);
                }
            }
            return list;
        }
    }
}