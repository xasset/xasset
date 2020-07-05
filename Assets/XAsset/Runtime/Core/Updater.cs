//
// Updater.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2019 fjy
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace libx
{
	public interface IUpdater
	{
		void OnMessage (string msg);

		void OnProgress (float progress);

		void OnVersion (string ver);
	}

	public class Updater : MonoBehaviour, IUpdater
	{
		private const string TAG = "Updater";

		private static void Log (string s)
		{
			Debug.Log (string.Format ("[{0}]{1}", TAG, s));
		}

		[SerializeField] private string baseURL = "http://127.0.0.1:7888/";
		[SerializeField] private string gameScene = "Game.unity";
		[SerializeField] private int maxDownloads = 3;
		[SerializeField] private bool enableVFS;

		private string _platform;
		private string _savePath;
		private List<Download> _downloads = new List<Download> ();
		private List<Download> _prepareToDownload = new List<Download> ();
		private List<VFile> _versions = new List<VFile> ();
		private VDisk _disk = new VDisk ();
		private int _downloadIndex;
		private int _finishedIndex;

		private const float BYTES_2_MB = 1f / (1024 * 1024);


		public IUpdater listener { get; set; }

		public void OnMessage (string msg)
		{
			if (listener != null) {
				listener.OnMessage (msg);
			}
		}

		public void OnProgress (float progress)
		{
			if (listener != null) {
				listener.OnProgress (progress);
			}
		}

		public void OnVersion (string ver)
		{
			if (listener != null) {
				listener.OnVersion (ver);
			}
		}

		private void Start ()
		{
			_savePath = Application.persistentDataPath + '/';
			Assets.updatePath = _savePath;
			_platform = GetPlatformForAssetBundles (Application.platform);
			DontDestroyOnLoad (gameObject);
		}

		public void Clear ()
		{
			MessageBox.Show ("提示", "清除数据后所有数据需要重新下载，请确认！", "清除").onComplete += id => {
				if (id != MessageBox.EventId.Ok)
					return;
				if (Directory.Exists (_savePath)) {
					Directory.Delete (_savePath, true);
				}

				OnMessage ("数据清除完毕");
				StartUpdate ();
			};
		}

		public void StartUpdate ()
		{
			StartCoroutine (Checking ());
		}

		void AddDownload (VFile item)
		{
			var download = new Download {
				url = GetDownloadURL (item.name),
				hash = item.hash,
				len = item.len,
				completed = OnFinished
			};
			_downloads.Add (download);
		}

		long PrepareDownloads ()
		{
			var size = 0L;
			if (enableVFS && !File.Exists (_savePath + Versions.Dataname)) {
				AddDownload (_versions [0]);
				size += _versions [0].len;
			} else {
				if (enableVFS) {
					Versions.LoadDisk (_savePath + Versions.Dataname);
				}
				for (var i = 1; i < _versions.Count; i++) {
					var item = _versions [i];
					if (Versions.IsNew (_savePath + item.name, item.len, item.hash)) {
						AddDownload (item);
						size += item.len;
					}
				}
			}
			return size;
		}

		IEnumerator RequestVFS ()
		{
			var mb = MessageBox.Show ("提示", "是否开启VFS？开启有助于提升IO性能和数据安全。", "开启");
			yield return mb;
			enableVFS = mb.isOk;
		}

		private static string GetPlatformForAssetBundles (RuntimePlatform target)
		{
			// ReSharper disable once SwitchStatementMissingSomeCases
			switch (target) {
			case RuntimePlatform.Android:
				return "Android";
			case RuntimePlatform.IPhonePlayer:
				return "iOS";
			case RuntimePlatform.WebGLPlayer:
				return "WebGL";
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsEditor:
				return "Windows";
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
				return "OSX"; 
			default:
				return null;
			}
		}

		private string GetDownloadURL (string filename)
		{
			return string.Format ("{0}{1}/{2}", baseURL, _platform, filename);
		}

		private IEnumerator Checking ()
		{
			if (!Directory.Exists (_savePath)) {
				Directory.CreateDirectory (_savePath);
			}

			yield return RequestVFS (); 
			yield return RequestCopy ();  

			OnMessage ("正在获取服务器版本信息..."); 
			var request = UnityWebRequest.Get (GetDownloadURL (Versions.Filename));
			request.downloadHandler = new DownloadHandlerFile (_savePath + Versions.Filename);
			yield return request.SendWebRequest ();
			if (!string.IsNullOrEmpty (request.error)) {
				var mb = MessageBox.Show ("提示", string.Format ("获取服务器版本失败：{0}", request.error), "重试", "退出");
				yield return mb;
				if (mb.isOk) {
					StartUpdate ();
				} else {
					Quit ();
					MessageBox.Dispose ();
				} 
				yield break;
			}
			request.Dispose (); 
			
			OnMessage ("正在检查版本信息...");
			_downloads.Clear ();
			if (enableVFS) {
				_disk.Clear ();
			}
			
			_versions = Versions.LoadVersions (_savePath + Versions.Filename, true); 
			if (_versions.Count > 0) {
				var totalSize = PrepareDownloads (); 
				if (totalSize > 0) { 
					var tips = string.Format ("检查到有{0}个文件需要更新，总计需要下载{1}（Bytes）内容", _downloads.Count, totalSize);
					var mb = MessageBox.Show ("提示", tips, "下载", "跳过");
					yield return mb;
					if (mb.isOk) {
						yield return UpdateDownloads (totalSize);
					} 
				}  
			}

			OnComplete (); 
		}

		private string GetStreamingAssetsPath ()
		{
			if (Application.platform == RuntimePlatform.Android) {
				return Application.streamingAssetsPath;
			} else if (Application.platform == RuntimePlatform.WindowsPlayer ||
			           Application.platform == RuntimePlatform.WindowsEditor) {
				return "file:///" + Application.streamingAssetsPath;
			} else {
				return "file://" + Application.streamingAssetsPath;
			}
		}

		private IEnumerator RequestCopy ()
		{
			var v1 = Versions.LoadVersion (_savePath + Versions.Filename);
			var basePath = GetStreamingAssetsPath () + "/";
			var request = UnityWebRequest.Get (basePath + Versions.Filename);
			var path = _savePath + Versions.Filename + ".tmp";
			request.downloadHandler = new DownloadHandlerFile (path);
			yield return request.SendWebRequest ();
			if (string.IsNullOrEmpty (request.error)) {
				var v2 = Versions.LoadVersion (path);
				if (v2 > v1) {
					var mb = MessageBox.Show ("提示", "是否将资源解压到本地？", "解压", "跳过");
					yield return mb;
					if (mb.isOk) {
						var versions = Versions.LoadVersions (path);
						yield return UpdateCopy (versions, basePath);
					}
				} else {
					Versions.LoadVersions (path);
				}
			} 
			request.Dispose ();
		}

		private IEnumerator UpdateCopy (List<VFile> versions, string basePath)
		{
			var version = versions [0];
			if (version.name.Equals (Versions.Dataname)) {
				var request = UnityWebRequest.Get (basePath + version.name);
				request.downloadHandler = new DownloadHandlerFile (_savePath + version.name);
				var req = request.SendWebRequest ();
				while (!req.isDone) {
					OnMessage ("正在复制文件");
					OnProgress (req.progress);
					yield return null;
				}

				request.Dispose ();
			} else {
				for (var index = 0; index < versions.Count; index++) {
					var item = versions [index];
					var request = UnityWebRequest.Get (basePath + item.name);
					request.downloadHandler = new DownloadHandlerFile (_savePath + item.name);
					yield return request.SendWebRequest ();
					request.Dispose ();
					OnMessage (string.Format ("正在复制文件：{0}/{1}", index, versions.Count));
					OnProgress (index * 1f / versions.Count);
				}
			}
		}

		private IEnumerator UpdateDownloads (long totalSize)
		{
			_downloadIndex = 0;
			_prepareToDownload.Clear ();
			_finishedIndex = 0;
			for (var i = 0; i < _downloads.Count; i++) {
				if (i < maxDownloads) {
					var item = _downloads [i];
					_prepareToDownload.Add (item);
					_downloadIndex++;
				} 
			} 
			var startTime = Time.realtimeSinceStartup; 
			while (_finishedIndex < _downloads.Count) {
				if (_prepareToDownload.Count > 0) {
					for (var i = 0; i < Math.Min (maxDownloads, _prepareToDownload.Count); i++) {
						var item = _prepareToDownload [i];
						item.Start ();
						Log ("Start Download：" + item.url);
						_prepareToDownload.RemoveAt (i);
						i--;
					}
				}
				var downloadSize = GetDownloadSize (); 
				var elapsed = Time.realtimeSinceStartup - startTime;
				var speed = GetSpeed (totalSize, elapsed);  
				OnMessage (string.Format ("下载中...{0}/{1}, 速度：{2}", downloadSize, totalSize, speed));
				OnProgress (downloadSize * 1f / totalSize);
				yield return null;
			}
		}

		private long GetDownloadSize ()
		{
			var downloadSize = 0L;
			foreach (var download in _downloads) {
				downloadSize += download.position;
			} 
			return downloadSize;
		}

		private void OnFinished (Download download)
		{
			if (!string.IsNullOrEmpty (download.error)) {
				Log ((string.Format ("{0} 下载失败:{1}, 开始重新下载。", download, download.error)));
				File.Delete (download.tempPath);
				_prepareToDownload.Add (download);
			} else {
				var filename = Path.GetFileName (download.url);
				var path = string.Format ("{0}{1}", _savePath, filename);
				File.Copy (download.tempPath, path, true);
				File.Delete (download.tempPath);   
				Debug.Log (string.Format ("Copy {0} to {1}.", download.tempPath, path));
				if (!filename.Equals (Versions.Dataname) && enableVFS) {
					_disk.AddFile (path, download.len, download.hash); 
				} 
				_finishedIndex++;
				if (_downloadIndex < _downloads.Count) {
					_prepareToDownload.Add (_downloads [_downloadIndex]);
					_downloadIndex++;
				}  
			}
		}

		private static string GetSpeed (long totalSize, float duration)
		{
			string unit;
			float unitSize;
			const float kb = 1024f;
			const float mb = kb * kb;
			if (totalSize < mb) {
				unit = "kb";
				unitSize = totalSize / kb;
			} else {
				unitSize = totalSize / mb;
				unit = "mb";
			}

			return string.Format ("{0:f2} {1}/s", unitSize / duration, unit);
		}

		private void OnComplete ()
		{  
			if (enableVFS) {			
				var dataPath = _savePath + Versions.Dataname; 
				if (_disk.Exists ()) {
					OnMessage ("更新本地版本信息"); 
					if (File.Exists (dataPath)) {
						var files = Versions.GetActivedFiles (); 
						var buffers = new byte[1024 * 4]; 
						using (var stream = File.OpenRead (dataPath)) {
							foreach (var item in files) {
								var path = _savePath + item.name;
								if (_disk.GetFile (path, item.hash) != null) {
									continue;
								} 
								using (var fs = File.OpenWrite (path)) { 
									stream.Seek (item.offset, SeekOrigin.Begin);
									var count = 0L;
									var len = item.len;
									while (count < len) {
										var read = (int)Math.Min (len - count, buffers.Length);
										stream.Read (buffers, 0, read);
										fs.Write (buffers, 0, read);
										count += read;
									}
								}
								_disk.AddFile (path, item.len, item.hash);  
							}
						} 
						File.Delete (dataPath); 
						_disk.name = dataPath;
						_disk.Save (true);    
					}  
				}   
				Versions.LoadDisk (dataPath); 
			} 
			OnProgress (1);
			OnMessage ("更新完成");
			var version = Versions.LoadVersion (_savePath + Versions.Filename);
			if (version > 0) {
				OnVersion (version.ToString ());
			} 
			
			StartCoroutine (LoadGameScene ());
		}

		private IEnumerator LoadGameScene ()
		{
			OnMessage ("正在初始化");
			Assets.runtimeMode = true;
			var init = Assets.Initialize ();
			yield return init;
			if (string.IsNullOrEmpty (init.error)) {
				OnProgress (0);
				OnMessage ("加载游戏场景");
				var scene = Assets.LoadSceneAsync (gameScene, false);
				while (!scene.isDone) {
					OnProgress (scene.progress);
					yield return null;
				}
			} else {
				var mb = MessageBox.Show ("提示", "初始化异常错误：" + init.error + "请联系技术支持");
				yield return mb;
				Quit ();
			}
		}

		private void Quit ()
		{
			_prepareToDownload.Clear ();
			_prepareToDownload = null;
			_downloads.Clear ();
			_downloads = null;
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
		}
	}
}