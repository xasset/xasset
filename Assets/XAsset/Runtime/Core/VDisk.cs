using System;
using System.Collections.Generic;
using System.IO;

namespace libx
{
    public class VDisk
    {
        private readonly Dictionary<string, VFile> _files = new Dictionary<string, VFile>();
        public string name { get; private set; }

        private Version _version = new Version(1, 0, 0, 0);

        public VDisk(string path)
        {
            name = path;
        }

        public void Load()
        {
            if (!File.Exists(name)) return;
            using (var stream = File.OpenRead(name))
            {
                Read(stream);
            }
        }

        public void Save()
        {
            using (var stream = File.OpenWrite(name))
            {
                Write(stream);
            }
        }
        
        public VFile AddFile(string path)
        {
            if (!File.Exists(path)) return null;
            using (var fs = File.OpenRead(path))
            {
                var key = Path.GetFileName(path);
                var file = new VFile {name = path, hash = Utility.GetCrc32Hash(fs), len = fs.Length, off = -1};
                _files[key] = file;
                return file;
            } 
        }

        public VFile GetFile(string path)
        {
            var key = Path.GetFileName(path); 
            VFile file;
            _files.TryGetValue(key, out file);
            return file;
        }

        private void Read(Stream stream)
        {
            _files.Clear();
            using (var reader = new BinaryReader(stream))
            {
                _version = new Version(reader.ReadString());
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var file = new VFile
                    {
                        name = reader.ReadString(),
                        hash = reader.ReadString(),
                        len = reader.ReadInt64(),
                        off = reader.ReadInt64()
                    };
                    var key = Path.GetFileName(file.name);
                    _files[key] = file;
                }
            }
        }

        private void Write(Stream stream)
        {
            // get headers lens
            var ms = new MemoryStream();
            long pos;
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(_version.ToString());
                writer.Write(_files.Count);
                foreach (var item in _files)
                {
                    var file = item.Value;
                    writer.Write(file.name);
                    writer.Write(file.hash);
                    writer.Write(file.len);
                    writer.Write(file.off); 
                }
                pos = ms.Length;
            } 
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var item in _files)
                {
                    var file = item.Value;
                    writer.Write(file.name);
                    writer.Write(file.hash);
                    writer.Write(file.len);
                    writer.Write(file.off);
                    file.off = pos;
                    pos += file.len;
                } 
                foreach (var item in _files)
                {
                    var file = item.Value;
                    var bytes = File.ReadAllBytes(file.name);
                    writer.Write(bytes);
                }
            }
        }
    }
}