//
// UpdateManager.cs
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
using vfs;

namespace libx
{
	public interface IUpdateManager
	{
		void OnMessage (string msg);

		void OnProgress (float progress);

		void OnVersion (string ver);
	}

	public class UpdateManager : MonoBehaviour, IUpdateManager
	{
		private const string Tag = "UpdateManager";

		private static void Log (string s)
		{
			Debug.Log (string.Format ("[{0}]{1}", Tag, s));
		}

		[SerializeField] private string downloadUrl = "http://127.0.0.1:7888/";
		[SerializeField] private string gameScene = "Game.unity";
		private string _savePath;
		private List<Download> _downloads = new List<Download> ();
		private List<Download> _prepareToDownload = new List<Download> ();
		private int _currentIndex;
		public int maxDownloadsPerFrame = 3;

		public IUpdateManager listener { get; set; }

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
			_savePath = Application.persistentDataPath + '/' + Assets.AssetBundles + '/';
			Assets.updatePath = _savePath;
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

		private IEnumerator Checking ()
		{
			if (!Directory.Exists (_savePath)) {
				Directory.CreateDirectory (_savePath);
			}

			yield return RequestCopy ();
            
			OnMessage ("正在获取服务器版本信息...");
            
			var request = UnityWebRequest.Get (downloadUrl + Versions.Filename);
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

			Versions.LoadDisk (_savePath + Versions.Dataname);

			var path = _savePath + Versions.Filename;
			var bytes = request.downloadHandler.text;
			File.WriteAllText (path, bytes);
			request.Dispose ();
			OnMessage ("正在加载版本信息...");
			if (!File.Exists (path))
				yield break;
			var versions = Versions.LoadVersions (path, true);
			OnMessage ("正在检查版本信息...");
			_downloads.Clear ();
			foreach (var item in versions) {
				if (Versions.IsNew (_savePath + item.name, item.len, item.hash)) {
					var download = new Download {
						url = downloadUrl + item.name, hash = item.hash, len = item.len, completed = OnFinished
					};
					_downloads.Add (download);
				}
			}

			if (_downloads.Count > 0) {
				var totalSize = 0L;
				foreach (var item in _downloads) {
					totalSize += item.len;
				}

				const float bytesToMb = 1f / (1024 * 1024);
				var tips = string.Format ("检查到有{0}个文件需要更新，总计需要下载{1:f2}（MB）内容", _downloads.Count, totalSize * bytesToMb);
				var mb = MessageBox.Show ("提示", tips, "下载", "跳过");
				yield return mb;
				if (mb.isOk) { 
					yield return UpdateDownload (bytesToMb, totalSize);
				}
			}

			OnComplete ();
		}

		private void OnFinished (Download download)
		{
			if (!string.IsNullOrEmpty (download.error)) {
				Log ((string.Format ("{0} 下载失败:{1}, 开始重新下载。", download, download.error)));
				File.Delete (download.tempPath);
				_prepareToDownload.Add (download);
			} else {
				Versions.OnFileDownload (download, _savePath); 
				if (_currentIndex < _downloads.Count) {
					_prepareToDownload.Add (_downloads [_currentIndex]);
					_currentIndex++;
				}  
			}
		}

		private string GetStreamingAssetsPath ()
		{
			if (Application.platform == RuntimePlatform.Android) {
				return Application.streamingAssetsPath;
			} else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
				return "file:///" + Application.streamingAssetsPath;
			} else {
				return "file://" + Application.streamingAssetsPath;
			}
		}

		private IEnumerator RequestCopy ()
		{
			var v1 = Versions.LoadVersion (_savePath + Versions.Filename); 
			var basePath = string.Format ("{0}/{1}/", GetStreamingAssetsPath (), Assets.AssetBundles); 
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
			} else {
				Log (string.Format ("{0}加载失败{1}", request.url, request.error));
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
					OnMessage (string.Format ("正在复制文件{0}/{1}", index, versions.Count));
					OnProgress (index * 1f / versions.Count);
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

		private IEnumerator UpdateDownload (float bytesToMb, long totalSize)
		{
			var tempPath = Application.persistentDataPath + "/temp";
			if (!Directory.Exists (tempPath)) {
				Directory.CreateDirectory (tempPath);
			} 
			_currentIndex = 0;
			_prepareToDownload.Clear ();
			foreach (var item in _downloads) {
				_prepareToDownload.Add (item);
				_currentIndex++;
			}
			var startTime = Time.realtimeSinceStartup;
			while (true) {
				if (_prepareToDownload.Count > 0) {
					for (var i = 0; i < Math.Min (maxDownloadsPerFrame, _prepareToDownload.Count); i++) {
						var item = _prepareToDownload [i];
						item.Start ();
						Log ("Start Download：" + item.url);
						_prepareToDownload.RemoveAt (i);
						i--;
					}
				}

				var downloadSize = 0L;
				foreach (var download in _downloads) { 
					downloadSize += download.position;
				}

				var elapsed = Time.realtimeSinceStartup - startTime;
				OnMessage (string.Format ("下载中...{0:f2}/{1:f2}(MB, {2})", downloadSize * bytesToMb, totalSize * bytesToMb,
					GetSpeed (totalSize, elapsed)));
				OnProgress (downloadSize * 1f / totalSize);

				if (downloadSize == totalSize) {
					break;
				}

				yield return null;
			}
		}

		private void OnComplete ()
		{
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