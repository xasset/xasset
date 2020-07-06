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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace libx
{
    public interface IUpdater
    {
        void OnStart();

        void OnMessage(string msg);

        void OnProgress(float progress);

        void OnVersion(string ver);

        void OnClear();
    }

    public class Updater : MonoBehaviour, IUpdater
    { 
        [SerializeField] private string baseURL = "http://127.0.0.1:7888/";
        [SerializeField] private string gameScene = "Game.unity";
        [SerializeField] private bool enableVFS;

        private string _platform;
        private string _savePath; 
        private List<VFile> _versions = new List<VFile>();
        private int _downloadIndex;
        private int _finishedIndex;

        public IUpdater listener { get; set; }

        public void OnMessage(string msg)
        {
            if (listener != null)
            {
                listener.OnMessage(msg);
            }

            Debug.Log(msg);
        }

        public void OnProgress(float progress)
        {
            if (listener != null)
            {
                listener.OnProgress(progress);
            }
        }

        public void OnVersion(string ver)
        {
            if (listener != null)
            {
                listener.OnVersion(ver);
            }
        }

        private Downloader _downloader;

        private void Start()
        {
            _downloader = gameObject.AddComponent<Downloader>();
            _downloader.onUpdate = OnUpdate;
            _downloader.onFinished = OnComplete;
            
            _savePath = Application.persistentDataPath + '/';
            
            Assets.updatePath = _savePath;
            _platform = GetPlatformForAssetBundles(Application.platform);
            DontDestroyOnLoad(gameObject);
        }

        private void OnUpdate(long arg1, long arg2, float arg3)
        {
            var speed = Downloader.GetDisplaySpeed(_downloader.speed);
            var pos = _downloader.position;
            var size = _downloader.size;
            OnMessage(string.Format("下载中...{0}/{1}, {2:f4}MB 速度：{3}", pos, size,
                pos * Downloader.BYTES_2_MB, speed));
            OnProgress(pos * 1f / size); 
        }

        public void Clear()
        {
            MessageBox.Show("提示", "清除数据后所有数据需要重新下载，请确认！", "清除").onComplete += id =>
            {
                if (id != MessageBox.EventId.Ok)
                    return;
                if (Directory.Exists(_savePath))
                {
                    Directory.Delete(_savePath, true);
                }

                OnMessage("数据清除完毕");
                OnClear();
            };
        }

        public void OnClear()
        {
            if (listener != null)
            {
                listener.OnClear();
            }
        }

        public void OnStart()
        {
            if (listener != null)
            {
                listener.OnStart();
            }
        }

        private IEnumerator checking;

        public void StartUpdate()
        {
            OnStart();

            if (checking != null)
            {
                StopCoroutine(checking);
            }

            checking = Checking();

            StartCoroutine(checking);
        }

        private void AddDownload(VFile item)
        {
            _downloader.AddDownload(GetDownloadURL(item.name), _savePath + item.name, item.hash, item.len);
        }

        private void PrepareDownloads()
        {
            if (enableVFS)
            {
                if (! File.Exists(_savePath + Versions.Dataname))
                {
                    AddDownload(_versions[0]); 
                }
                Versions.LoadDisk(_savePath + Versions.Dataname);
            }
            
            for (var i = 1; i < _versions.Count; i++)
            {
                var item = _versions[i];
                if (Versions.IsNew(_savePath + item.name, item.len, item.hash))
                {
                    AddDownload(item);
                }
            }   
        }

        private IEnumerator RequestVFS()
        {
            var mb = MessageBox.Show("提示", "是否开启VFS？开启有助于提升IO性能和数据安全。", "开启");
            yield return mb;
            enableVFS = mb.isOk;
        }

        private static string GetPlatformForAssetBundles(RuntimePlatform target)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (target)
            {
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

        private string GetDownloadURL(string filename)
        {
            return string.Format("{0}{1}/{2}", baseURL, _platform, filename);
        }

        private IEnumerator Checking()
        {
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            yield return RequestVFS();
            yield return RequestCopy();

            OnMessage("正在获取服务器版本信息...");
            
            var request = UnityWebRequest.Get(GetDownloadURL(Versions.Filename));
            request.downloadHandler = new DownloadHandlerFile(_savePath + Versions.Filename);
            yield return request.SendWebRequest();
            if (!string.IsNullOrEmpty(request.error))
            {
                var mb = MessageBox.Show("提示", string.Format("获取服务器版本失败：{0}", request.error), "重试", "退出");
                yield return mb;
                if (mb.isOk)
                {
                    StartUpdate();
                }
                else
                {
                    Quit();
                    MessageBox.Dispose();
                } 
                yield break;
            }

            request.Dispose();

            OnMessage("正在检查版本信息...");  

            _versions = Versions.LoadVersions(_savePath + Versions.Filename, true); 
            if (_versions.Count > 0)
            {
                PrepareDownloads();
                var totalSize = _downloader.size;
                if (totalSize > 0)
                {
                    var tips = string.Format("发现内容更新，总计需要下载 {0}（B）内容", totalSize);
                    var mb = MessageBox.Show("提示", tips, "下载", "跳过");
                    yield return mb;
                    if (mb.isOk)
                    {
                        _downloader.StartDownload(); 
                        yield break;
                    }
                }
            } 
            
            OnComplete();
        } 

        private static string GetStreamingAssetsPath()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return Application.streamingAssetsPath;
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer ||
                     Application.platform == RuntimePlatform.WindowsEditor)
            {
                return "file:///" + Application.streamingAssetsPath;
            }
            else
            {
                return "file://" + Application.streamingAssetsPath;
            }
        }

        private IEnumerator RequestCopy()
        {
            var v1 = Versions.LoadVersion(_savePath + Versions.Filename);
            var basePath = GetStreamingAssetsPath() + "/";
            var request = UnityWebRequest.Get(basePath + Versions.Filename);
            var path = _savePath + Versions.Filename + ".tmp";
            request.downloadHandler = new DownloadHandlerFile(path);
            yield return request.SendWebRequest();
            if (string.IsNullOrEmpty(request.error))
            {
                var v2 = Versions.LoadVersion(path);
                if (v2 > v1)
                {
                    var mb = MessageBox.Show("提示", "是否将资源解压到本地？", "解压", "跳过");
                    yield return mb;
                    if (mb.isOk)
                    {
                        var versions = Versions.LoadVersions(path);
                        yield return UpdateCopy(versions, basePath);
                    }
                }
                else
                {
                    Versions.LoadVersions(path);
                }
            }

            request.Dispose();
        }

        private IEnumerator UpdateCopy(IList<VFile> versions, string basePath)
        {
            var version = versions[0];
            if (version.name.Equals(Versions.Dataname))
            {
                var request = UnityWebRequest.Get(basePath + version.name);
                request.downloadHandler = new DownloadHandlerFile(_savePath + version.name);
                var req = request.SendWebRequest();
                while (!req.isDone)
                {
                    OnMessage("正在复制文件");
                    OnProgress(req.progress);
                    yield return null;
                }

                request.Dispose();
            }
            else
            {
                for (var index = 0; index < versions.Count; index++)
                {
                    var item = versions[index];
                    var request = UnityWebRequest.Get(basePath + item.name);
                    request.downloadHandler = new DownloadHandlerFile(_savePath + item.name);
                    yield return request.SendWebRequest();
                    request.Dispose();
                    OnMessage(string.Format("正在复制文件：{0}/{1}", index, versions.Count));
                    OnProgress(index * 1f / versions.Count);
                }
            }
        } 

        private void OnComplete()
        {
            if (enableVFS)
            {
                var dataPath = _savePath + Versions.Dataname;
                var downloads = _downloader.downloads;
                if (downloads.Count > 0 && File.Exists(dataPath))
                {
                    OnMessage("更新本地版本信息");
                    var files = new List<VFile>(downloads.Count);
                    foreach (var download in downloads)
                    {
                        files.Add(new VFile
                        {
                            name = Path.GetFileName(download.savePath),
                            hash = download.hash,
                            len = download.len,
                        });
                    }
                    Versions.UpdateDisk(dataPath, files); 
                } 
                Versions.LoadDisk(dataPath);
            }

            OnProgress(1);
            OnMessage("更新完成");
            var version = Versions.LoadVersion(_savePath + Versions.Filename);
            if (version > 0)
            {
                OnVersion(version.ToString());
            }

            StartCoroutine(LoadGameScene());
        }

        private IEnumerator LoadGameScene()
        {
            OnMessage("正在初始化");
            Assets.runtimeMode = true;
            var init = Assets.Initialize();
            yield return init;
            if (string.IsNullOrEmpty(init.error))
            {
                OnProgress(0);
                OnMessage("加载游戏场景");
                var scene = Assets.LoadSceneAsync(gameScene, false);
                while (!scene.isDone)
                {
                    OnProgress(scene.progress);
                    yield return null;
                }
            }
            else
            {
                var mb = MessageBox.Show("提示", "初始化异常错误：" + init.error + "请联系技术支持");
                yield return mb;
                Quit();
            }
        }

        private void Quit()
        { 
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}