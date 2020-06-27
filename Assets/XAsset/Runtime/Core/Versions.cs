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

namespace libx
{
	public class Record
	{
		public string name;
		public string hash;
		public long len;
	}

	public enum VerifyBy
	{
		Size,
		Hash,
	}

	public static class Versions
	{
		public const string Filename = "versions.unity3d";
		public const string BuildVersion = "build_version.unity3d";
		public static VerifyBy verifyBy = VerifyBy.Hash;

		public static void MakeRecords (string outputPath)
		{
			var path = outputPath + "/" + Filename;
			if (File.Exists (path)) {
				File.Delete (path);
			}

			var getFiles = Directory.GetFiles (outputPath, "*");
			using (var writer = new StreamWriter (File.OpenWrite (path))) {
				foreach (var file in getFiles) {
					var name = Path.GetFileName (file);
					using (var stream = File.OpenRead (file)) {
						var len = stream.Length;
						var crc = Utility.GetCrc32Hash (stream);
						writer.WriteLine ("{0},{1},{2}", name, crc, len);
					}
				}
			}
		}

		public static List<Record> LoadRecords (Stream s)
		{
			using (var reader = new StreamReader (s)) {
				var records = new List<Record> ();
				string line;
				while ((line = reader.ReadLine ()) != null) {
					if (string.IsNullOrEmpty (line)) {
						continue;
					}

					var fields = line.Split (',');
					if (fields.Length <= 2)
						continue;

					var record = new Record
                        { name = fields [0], hash = fields [1], len = int.Parse (fields [2]) };
					records.Add (record);
				}

				return records;
			}
		}

		public static List<Record> LoadRecords (string filename)
		{
			using (var stream = File.OpenRead (filename)) {
				return LoadRecords (stream);
			}
		}
	}
}