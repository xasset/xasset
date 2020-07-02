using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using libx;

namespace vfs
{
	public class VFile
	{
		public string hash { get; set; }

		public long id { get; set; }

		public long len { get; set; }

		public string name { get; set; }

		public long offset { get; set; }

		public VFile ()
		{
			offset = -1;
		}

		public override string ToString ()
		{
			return string.Format ("{0},{1},{2}", name, len, hash);
		}

		public void Serialize (BinaryWriter writer)
		{
			writer.Write (ToString ()); 
		}

		public void Deserialize (BinaryReader reader)
		{
			var fields = reader.ReadString ().Split (',');
			if (fields.Length > 2) {
				name = fields [0];
				len = fields [1].Int64Value ();
				hash = fields [2];
			}
		}
	}

	public class VDisk
	{
		private readonly byte[] _buffers = new byte[1024 * 4];
		private readonly Dictionary<string, VFile> _data = new Dictionary<string, VFile> ();

		public string name { get; private set; }

		public List<VFile> files { get; private set; }

		private long _pos;
		private long _len;

		public VDisk (string path)
		{
			files = new List<VFile> ();
			name = path; 
		}

		public VDisk()
		{
			
		}

		public void AddFile (string path, string hash)
		{
			if (_pos > 0) {
				var file = GetFile (path, hash);
				if (file != null)
					UpdateFile (path, hash, file);
				else
					WriteNewFile (path, hash);
			} else {
				WriteNewFile (path, hash);
			}
		}

		private void UpdateFile (string path, string hash, VFile file)
		{
			var fileLen = file.len;
			file.len = new FileInfo (path).Length;
			file.hash = hash;
			using (var fs = File.OpenWrite (name + ".tmp")) {
				var writer = new BinaryWriter (fs);
				writer.Write (files.Count);
				foreach (var item in files)
					item.Serialize (writer); 
				var pos = writer.BaseStream.Position;
				using (var stream = File.OpenRead (name)) {
					stream.Seek (_pos, SeekOrigin.Begin);
					foreach (var item in files) {
						if (item.id < file.id) {
							WriteStream (item.len, stream, writer);
						} else {
							WriteFile (path, writer); 
							stream.Seek (fileLen, SeekOrigin.Current);
							WriteStream (stream.Length - stream.Position, stream, writer);
							break;
						}
					} 
				} 
				_pos = pos; 
			}

			File.Copy (name + ".tmp", name, true);
		}

		private void WriteFile (string path, BinaryWriter writer)
		{
			using (var fs = File.OpenRead (path)) {
				var len = fs.Length;
				WriteStream (len, fs, writer);
			}
		}

		private void WriteStream (long len, Stream stream, BinaryWriter writer)
		{
			var count = 0L;
			while (count < len) {
				var read = (int)Math.Min (len - count, _buffers.Length);
				stream.Read (_buffers, 0, read);
				writer.Write (_buffers, 0, read);
				count += read;
			}
		}

		private void WriteNewFile (string path, string hash)
		{
			using (var stream = File.OpenWrite (name)) {
				var writer = new BinaryWriter (stream);
				stream.Seek (_pos, SeekOrigin.Begin);
				using (var fs = File.OpenRead (path)) {
					if (string.IsNullOrEmpty (hash)) {
						hash = Utility.GetCRC32Hash (fs);
					} 
					var len = fs.Length;
					WriteStream (len, fs, writer);

					var file = new VFile { name = Path.GetFileName (path), id = files.Count, hash = hash, len = len };
					file.Serialize (writer);

					_pos = stream.Position;
					stream.Seek (0, SeekOrigin.End);
					WriteStream (len, fs, writer);

					AddFile (file);
				}
			}
		}

		public bool Load (string path)
		{
			if (!File.Exists (path))
				return false;
			
			files = new List<VFile> ();
			name = path;
			using (var reader = new BinaryReader (File.OpenRead (path))) { 
				var count = reader.ReadInt32 ();
				for (var i = 0; i < count; i++) {
					var file = new VFile { id = i };
					file.Deserialize (reader);
					AddFile (file); 
				}
				_pos = reader.BaseStream.Length;
				Reindex ();
			} 
			return true;
		}

		public void Reindex ()
		{
			_len = 0L;
			for (var i = 0; i < files.Count; i++) {
				var file = files [i];
				file.offset = _pos + _len;
				_len += file.len;
			}
		}

		private void AddFile (VFile file)
		{
			_data [file.name] = file;
			files.Add (file);
		}

		public VFile GetFile (string path, string hash)
		{
			var key = Path.GetFileName (path);
			VFile file;
			_data.TryGetValue (key, out file);
			return file;
		}

		public void Save ()
		{
			using (var stream = File.OpenWrite (name)) {
				var writer = new BinaryWriter (stream);
				stream.Seek (0, SeekOrigin.Begin); 
				writer.Write (files.Count);
				writer.Flush (); 
				writer.Close ();
			}  
		}
	}
}