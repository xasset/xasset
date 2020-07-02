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
using libx;

namespace libx
{
	public enum VerifyBy
	{
		Size,
		Hash,
	}

	public static class Versions
	{
		public const string Dataname = "data";
		public const string Filename = "vers";
		public static VerifyBy verifyBy = VerifyBy.Hash;

		public static VDisk disk { get; set; }

		private static Dictionary<string, VFile> updateData = new Dictionary<string, VFile> ();
		public static Dictionary<string, VFile> baseData = new Dictionary<string, VFile> ();


		public static AssetBundle LoadAssetBundleFromFile (string url)
		{
			if (disk != null) {
				var name = Path.GetFileName (url);
				var file = disk.GetFile (name, string.Empty);
				if (file != null) {
					return AssetBundle.LoadFromFile (disk.name, 0, (ulong)file.offset);
				}
			}
			return AssetBundle.LoadFromFile (url); 
		}

		public static AssetBundleCreateRequest LoadAssetBundleFromFileAsync (string url)
		{
			if (disk != null) {
				var name = Path.GetFileName (url);
				var file = disk.GetFile (name, string.Empty);
				if (file != null) {
					return AssetBundle.LoadFromFileAsync (disk.name, 0, (ulong)file.offset);
				}
			}
			return AssetBundle.LoadFromFileAsync (url); 
		}

		public static void BuildVersions (string outputPath, int version)
		{
			var path = outputPath + "/" + Filename;
			if (File.Exists (path)) {
				File.Delete (path);
			}

			var dataPath = outputPath + "/" + Dataname;
			if (File.Exists (dataPath)) {
				File.Delete (dataPath);
			}  
			var getFiles = Directory.GetFiles (outputPath, "*.unity3d");
			var vd = new VDisk (dataPath); 
			foreach (var file in getFiles) {
				vd.AddFile (file, string.Empty); 
			}  
			vd.Save ();  
            
			using (var stream = File.OpenWrite (path)) {
                    
				var writer = new BinaryWriter (stream); 
				writer.Write (version);
				writer.Write (vd.files.Count + 1);
                
				using (var fs = File.OpenRead (dataPath)) {
					VFile file = new VFile ();
					file.name = Dataname;
					file.len = fs.Length;
					file.hash = Utility.GetCRC32Hash (fs); 
					writer.Write (file.ToString ());
				}
                
				foreach (var file in vd.files) {
					file.Serialize (writer);
				}
			}
		}

		public static int LoadVersion (string filename)
		{
			if (File.Exists (filename)) {
				using (var stream = File.OpenRead (filename)) {
					var reader = new BinaryReader (stream);
					return reader.ReadInt32 ();
				}
			}
			return -1;
		}

		public static List<VFile> LoadVersions (string filename, bool update = false)
		{
			var data = update ? updateData : baseData; 
			data.Clear ();
			using (var stream = File.OpenRead (filename)) {
				var reader = new BinaryReader (stream);
				var list = new List<VFile> (); 
				var verion = reader.ReadInt32 ();
				Debug.Log ("LoadVesions:" + verion);
				var count = reader.ReadInt32 ();
				for (var i = 0; i < count; i++) {
					var version = new VFile ();
					version.Deserialize (reader);
					list.Add (version);
					data [version.name] = version;
				} 
				return list;
			}
		}

		public static void LoadDisk (string filename)
		{
			disk = new VDisk ();
			disk.Load (filename);
		}

		public static void OnFileDownload (Download download, string savePath)
		{
			if (disk != null) {
				disk.AddFile (download.tempPath, download.hash);
			} else {
				File.Copy (download.tempPath, savePath + Path.GetFileName (download.url), true);
			}
			File.Delete (download.tempPath);
		}

		public static bool IsNew (string path, long len, string hash)
		{
			VFile file;
			var key = Path.GetFileName (path);
			if (baseData.TryGetValue (key, out file)) {
				if (key.Equals (Dataname) || file.len == len && file.hash.Equals (hash, StringComparison.OrdinalIgnoreCase)) {
					return false;
				}
			}
            
			if (disk != null) {
				var vdf = disk.GetFile (path, hash);
				if (vdf != null && vdf.len == len && vdf.hash.Equals (hash, StringComparison.OrdinalIgnoreCase)) {
					return false;
				}
			}
            
			if (!File.Exists (path)) {
				return true;
			}

			using (var stream = File.OpenRead (path)) {
				if (stream.Length != len) {
					return true;
				} 
				if (verifyBy == VerifyBy.Hash) {
					if (!Utility.GetCRC32Hash (stream).Equals (hash, StringComparison.OrdinalIgnoreCase)) {
						return true;
					}
				} 
				return false; 
			} 
		}
	}
}