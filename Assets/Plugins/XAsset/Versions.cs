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
    public static class Versions
	{
		public const string versionFile = "download.txt";
		public const string appVersionFile = "app_ver.txt";

		private const char splitKey = '=';

		public static Dictionary<string, string> data = new Dictionary<string, string> ();

		public static void Load ()
		{
			var path = Assets.GetRelativeUpdatePath(appVersionFile);
			if (File.Exists (path)) {
				var ver = new System.Version (File.ReadAllText (path));
				if (ver < new System.Version (Application.version)) {
					Clear ();
				} 
			} 

			data.Clear();
			path = Assets.GetRelativeUpdatePath(versionFile); 
			if (File.Exists (path)) { 
				using (var s = new StreamReader (path)) {
					string line;
					while ((line = s.ReadLine ()) != null) {
						if (line == string.Empty)
							continue;
						var fields = line.Split (splitKey);
						if (fields.Length > 1)
							data.Add (fields [0], fields [1]);
					}
				}
			}
		}

		public static void Clear ()
		{
			data.Clear ();
			var dir = Path.GetDirectoryName (Assets.updatePath);
			if (Directory.Exists (dir)) {
				Directory.Delete (dir, true);
			}
		}

		public static void Set (string key, string version)
		{
			data [key] = version;
		}

		public static string Get (string key)
		{
			string version;
			data.TryGetValue (key, out version);
			return version;
		}

		public static void Save ()
		{
			var dir = Path.GetDirectoryName (Assets.updatePath);
			if (! Directory.Exists (dir)) {
				Directory.CreateDirectory(dir);
			}

			var path = Assets.updatePath + versionFile;
			if (File.Exists (path))
				File.Delete (path);

			if (data.Count > 0) {
				using (var s = new StreamWriter (path)) {
					foreach (var item in data)
						s.WriteLine (item.Key + splitKey + item.Value);
					s.Flush ();
					s.Close ();
				} 
			} 

			path = Assets.GetRelativeUpdatePath(appVersionFile); 

			if (File.Exists (path))
				File.Delete (path);
			
			File.WriteAllText (path, Application.version); 
		}
	}
}
