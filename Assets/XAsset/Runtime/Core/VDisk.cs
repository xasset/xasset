//
// VDisk.cs
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
    public class VFile
    {
        public string hash { get; set; }

        public long id { get; set; }

        public long len { get; set; }

        public string name { get; set; }

        public long offset { get; set; }

        public VFile()
        {
            offset = -1;
        }

        public override string ToString()
        {
            return string.Format("file:{0}, {1}, {2}", name, len, hash);
        }

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

    public class VDisk
    {
        private readonly byte[] _buffers = new byte[1024 * 4];
        private readonly Dictionary<string, VFile> _data = new Dictionary<string, VFile>();

        public string name { get; private set; }

        public List<VFile> files = new List<VFile>();

        private long _pos;
        private long _len;

        public VDisk(string path)
        {
            name = path;
        }

        public VDisk()
        {
        }

        public bool Exists()
        {
            return files.Count > 0;
        }

        public void AddFile(string path, string hash, bool write = false)
        {
            if (_pos > 0)
            {
                var file = GetFile(path, hash);
                if (file != null)
                    UpdateFile(path, hash, file, write);
                else
                    WriteNewFile(path, hash, write);
            }
            else
            {
                WriteNewFile(path, hash, write);
            }
        }

        private void UpdateFile(string path, string hash, VFile file, bool write)
        {
            var fileLen = file.len;
            file.len = new FileInfo(path).Length;
            file.hash = hash;
            if (!write) return;
            var tmpfile = name + ".tmp";
            using (var fs = File.OpenWrite(tmpfile))
            {
                var writer = new BinaryWriter(fs);
                writer.Write(files.Count);
                foreach (var item in files)
                    item.Serialize(writer);
                var pos = writer.BaseStream.Position;

                using (var stream = File.OpenRead(name))
                {
                    stream.Seek(_pos, SeekOrigin.Begin);
                    foreach (var item in files)
                    {
                        if (item.id < file.id)
                        {
                            WriteStream(item.len, stream, writer);
                        }
                        else
                        {
                            WriteFile(path, writer);
                            stream.Seek(fileLen, SeekOrigin.Current);
                            WriteStream(stream.Length - stream.Position, stream, writer);
                            break;
                        }
                    }
                }

                _pos = pos;
            }

            File.Copy(tmpfile, name, true);
            File.Delete(tmpfile);
        }

        private void WriteFile(string path, BinaryWriter writer)
        {
            using (var fs = File.OpenRead(path))
            {
                var len = fs.Length;
                WriteStream(len, fs, writer);
            }
        }

        private void WriteStream(long len, Stream stream, BinaryWriter writer)
        {
            var count = 0L;
            while (count < len)
            {
                var read = (int) Math.Min(len - count, _buffers.Length);
                stream.Read(_buffers, 0, read);
                writer.Write(_buffers, 0, read);
                count += read;
            }
        }

        private void WriteNewFile(string path, string hash, bool write)
        {
            using (var fs = File.OpenRead(path))
            {
                if (string.IsNullOrEmpty(hash))
                {
                    hash = Utility.GetCRC32Hash(fs);
                }

                var file = new VFile {name = Path.GetFileName(path), id = files.Count, hash = hash, len = fs.Length};
                AddFile(file);
                if (!write) return;
                using (var stream = File.OpenWrite(name))
                {
                    var writer = new BinaryWriter(stream);
                    stream.Seek(_pos, SeekOrigin.Begin);
                    file.Serialize(writer);
                    _pos = stream.Position;
                    stream.Seek(0, SeekOrigin.End);
                    WriteStream(fs.Length, fs, writer);
                }
            }
        }

        public bool Load(string path)
        {
            if (!File.Exists(path))
                return false;

            files.Clear();
            name = path;
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var file = new VFile {id = i};
                    file.Deserialize(reader);
                    AddFile(file);
                    Debug.Log(file);
                }

                _pos = reader.BaseStream.Length;
                Reindex();
            }

            return true;
        }

        public void Reindex()
        {
            _len = 0L;
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                file.offset = _pos + _len;
                _len += file.len;
            }
        }

        private void AddFile(VFile file)
        {
            _data[file.name] = file;
            files.Add(file);
        }

        public VFile GetFile(string path, string hash)
        {
            var key = Path.GetFileName(path);
            VFile file;
            _data.TryGetValue(key, out file);
            return file;
        }

        public void Save()
        {
            using (var stream = File.OpenWrite(name))
            {
                var writer = new BinaryWriter(stream);
                var dir = Path.GetDirectoryName(name);

                writer.Write(files.Count);
                foreach (var item in files)
                {
                    item.Serialize(writer);
                }

                foreach (var item in files)
                {
                    writer.Write(File.ReadAllBytes(dir + "/" + item.name));
                }
            }
        }

        public void Clear()
        {
            _data.Clear();
            files.Clear();
        }
    }
}