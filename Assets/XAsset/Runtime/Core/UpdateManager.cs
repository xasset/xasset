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

namespace libx
{
    public interface IUpdateManager
    {
        void OnMessage(string msg);
        void OnProgress(float progress);
        void OnVersion(string ver);
    }

    public class UpdateManager : MonoBehaviour, IUpdateManager
    {
        private const string Tag = "UpdateManager";

        private static void Log(string s)
        {
            Debug.Log(string.Format("[{0}]{1}", Tag, s));
        }

        [SerializeField] private string downloadUrl = "http://127.0.0.1:7888/";
        [SerializeField] private string gameScene = "Game.unity";
        private string _savePath;
        private Dictionary<string, Record> _serverRecords = new Dictionary<string, Record>();
        private List<Download> _downloads = new List<Download>();
        private List<Download> _prepareToDownload = new List<Download>();
        private int _currentIndex;
        public int maxDownloadsPerFrame = 3;

        public IUpdateManager listener { get; set; }

        public void OnMessage(string msg)
        {
            if (listener != null)
            {
                listener.OnMessage(msg);
            }
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

        private void Start()
        {
            _savePath = Application.persistentDataPath + '/' + Assets.AssetBundles + '/';
            Assets.updatePath = _savePath;
            DontDestroyOnLoad(gameObject);
        }

        public void Clear()
        {
            MessageBox.Show("提示", "清除数据后所有数据需要重新下载，请确认！", "清除").onComplete += id =>
            {
                if (id == MessageBox.EventId.Ok)
                {
                    if (Directory.Exists(_savePath))
                    {
                        Directory.Delete(_savePath, true);
                    }

                    OnMessage("数据清除完毕");
                    StartUpdate();
                }
            };
        }

        public void StartUpdate()
        {
            StartCoroutine(Checking());
        }

        private IEnumerator Checking()
        {
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            yield return ExtractAssetsIfNeed();

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                var mb = MessageBox.Show("提示", "网络状态不可达，请联网后重试。", "重试", "退出");
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

            OnMessage("正在获取服务器版本信息...");
            const string assetName = Versions.Filename;
            var url = downloadUrl + assetName;
            var request = UnityWebRequest.Get(url);
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

            var path = _savePath + assetName;
            var bytes = request.downloadHandler.text;
            File.WriteAllText(path, bytes);
            request.Dispose();
            OnMessage("正在加载版本信息...");
            if (!File.Exists(path))
                yield break;
            var records = Versions.LoadRecords(path);
            OnMessage("正在检查版本信息...");
            _serverRecords.Clear();
            _downloads.Clear();
            foreach (var item in records)
            {
                _serverRecords[item.name] = item;
                if (IsUpdate(item))
                {
                    AddDownload(item);
                }
            }

            if (_downloads.Count > 0)
            {
                var totalSize = 0L;
                foreach (var item in _downloads)
                {
                    totalSize += item.len;
                }

                const float bytesToMb = 1f / (1024 * 1024);
                var tips = string.Format("检查到有{0}个文件需要更新，总计需要下载{1:f2}（MB）内容", _downloads.Count, totalSize * bytesToMb);
                var mb = MessageBox.Show("提示", tips, "下载", "跳过");
                yield return mb;
                if (mb.isOk)
                {
                    PrepareToDownload();
                    yield return UpdateDownloads(bytesToMb, totalSize);
                }
            }

            OnComplete();
        }

        private IEnumerator ExtractAssetsIfNeed()
        {
            var path = _savePath + Versions.BuildVersion;
            var outerVersion = -1;
            if (File.Exists(path))
            {
                outerVersion = File.ReadAllText(path).IntValue();
            }

            var basePath = string.Format("{0}/{1}/", Application.streamingAssetsPath, Assets.AssetBundles);
            if (Application.platform == RuntimePlatform.Android)
            {
                var url = string.Format("{0}{1}", basePath, Versions.BuildVersion);
                var request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();
                var innerVersion = request.downloadHandler.text.IntValue();
                var error = request.error;
                Log(string.Format("版本信息:{0}, {1}, {2}, {3}", url, outerVersion, innerVersion, error));
                if (string.IsNullOrEmpty(error))
                {
                    if (innerVersion > outerVersion)
                    {
                        var mb = MessageBox.Show("提示", "是否将资源解压到本地？", "解压", "跳过");
                        yield return mb;
                        if (mb.isOk)
                        {
                            yield return CopyByRequest(basePath);
                        }
                    }
                }
                else
                {
                    Log(string.Format("{0}加载失败{1}", url, request.error));
                }

                request.Dispose();
            }
            else
            {
                var pathToBuildVersion = string.Format("{0}{1}", basePath, Versions.BuildVersion);
                if (File.Exists(pathToBuildVersion))
                {
                    var innerVersion = File.ReadAllText(pathToBuildVersion).IntValue();
                    if (innerVersion > outerVersion)
                    {
                        var mb = MessageBox.Show("提示", "是否将资源解压到本地？", "解压", "跳过");
                        yield return mb;
                        if (mb.isOk)
                        {
                            yield return CopyByFile(basePath);
                        }
                    }
                }
            }
        }

        private IEnumerator CopyByRequest(string basePath)
        {
            var request = UnityWebRequest.Get(basePath + Versions.Filename);
            request.SendWebRequest();
            yield return request;
            if (string.IsNullOrEmpty(request.error))
            {
                using (var stream = new MemoryStream(request.downloadHandler.data))
                {
                    var records = Versions.LoadRecords(stream);
                    for (var index = 0; index < records.Count; index++)
                    {
                        var item = records[index];
                        var assetName = item.name;
                        var assetRequest = UnityWebRequest.Get(basePath + assetName);
                        yield return assetRequest.SendWebRequest();
                        if (string.IsNullOrEmpty(assetRequest.error))
                        {
                            File.WriteAllBytes(_savePath + assetName, assetRequest.downloadHandler.data);
                        }

                        assetRequest.Dispose();
                        OnMessage(string.Format("正在复制文件{0}/{1}", index, records.Count));
                        OnProgress(index * 1f / records.Count);
                    }
                }
            }

            request.Dispose();
        }

        private IEnumerator CopyByFile(string basePath)
        {
            var path = basePath + Versions.Filename;
            if (!File.Exists(path))
                yield break;
            var records = Versions.LoadRecords(path);
            for (var index = 0; index < records.Count; index++)
            {
                var item = records[index];
                var assetName = item.name;
                var assetPath = basePath + assetName;
                if (File.Exists(assetPath))
                {
                    File.Copy(assetPath, _savePath + assetName, true);
                }

                OnMessage(string.Format("正在复制文件{0}/{1}", index, records.Count));
                OnProgress(index * 1f / records.Count);
                yield return null;
            }
        }

        private static string GetDownloadSpeed(long totalSize, float duration)
        {
            string unit;
            float unitSize;
            const float kb = 1024f;
            const float mb = kb * kb;
            if (totalSize < mb)
            {
                unit = "kb";
                unitSize = totalSize / kb;
            }
            else
            {
                unitSize = totalSize / mb;
                unit = "mb";
            } 
            return string.Format("{0:f2} {1}/s", unitSize/duration, unit);
        }

        private IEnumerator UpdateDownloads(float bytesToMb, long totalSize)
        {
            var startTime = Time.realtimeSinceStartup;
            while (true)
            {
                if (_prepareToDownload.Count > 0)
                {
                    for (var i = 0; i < Math.Min(maxDownloadsPerFrame, _prepareToDownload.Count); i++)
                    {
                        var item = _prepareToDownload[i];
                        item.Start();
                        Log("Start Download：" + item.url);
                        _prepareToDownload.RemoveAt(i);
                        i--;
                    }
                }

                var downloadSize = 0L;
                foreach (var download in _downloads)
                {
                    download.Update();
                    downloadSize += download.position;
                }  
                var elapsed = Time.realtimeSinceStartup - startTime;  
                OnMessage(string.Format("下载中...{0:f2}/{1:f2}(MB, {2})", downloadSize * bytesToMb, totalSize * bytesToMb, GetDownloadSpeed(totalSize, elapsed)));
                OnProgress(downloadSize * 1f / totalSize);

                if (downloadSize == totalSize)
                {
                    break;
                }

                yield return null;
            }
        }

        private void PrepareToDownload()
        {
            var tempPath = Application.persistentDataPath + "/temp";
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            _currentIndex = 0;
            _prepareToDownload.Clear();
            foreach (var item in _downloads)
            {
                _prepareToDownload.Add(item);
                _currentIndex++;
            }
        }

        private void OnComplete()
        {
            OnProgress(1);
            OnMessage("更新完成");
            var path = _savePath + Versions.BuildVersion;
            if (File.Exists(path))
            {
                OnVersion(File.ReadAllText(_savePath + Versions.BuildVersion));
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

        /// <summary>
        /// 是否需要更新本地的版本
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool IsUpdate(Record item)
        {
            var path = _savePath + item.name;
            if (!File.Exists(path))
            {
                return true;
            }
            else
            {
                using (var stream = File.OpenRead(path))
                {
                    if (Versions.verifyBy == VerifyBy.Size)
                    {
                        if (stream.Length != item.len)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        var hash = Utility.GetCrc32Hash(stream);
                        if (!Utility.VerifyCrc32Hash(hash, item.hash))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void AddDownload(Record bundle)
        {
            var download = new Download
            {
                url = downloadUrl + bundle.name, hash = bundle.hash, len = bundle.len, completed = OnDownloadCompleted
            };
            _downloads.Add(download);
        }

        private void OnDownloadCompleted(Download download)
        {
            if (!string.IsNullOrEmpty(download.error))
            {
                Log((string.Format("{0}下载失败:{1}", download.url, download.error)));
                File.Delete(download.tempPath);
                _prepareToDownload.Add(download);
            }
            else
            {
                if (File.Exists(download.tempPath))
                {
                    var error = GetDownloadError(download);
                    if (string.IsNullOrEmpty(error))
                    {
                        File.Copy(download.tempPath, _savePath + Path.GetFileName(download.url), true);
                        File.Delete(download.tempPath);
                        if (_currentIndex < _downloads.Count)
                        {
                            _prepareToDownload.Add(_downloads[_currentIndex]);
                            _currentIndex++;
                        }
                    }
                    else
                    {
                        Log(string.Format("{0}, 大小 {1}, 下载失败:{2}, 开始重新下载。", download.url, download.len, error));
                        File.Delete(download.tempPath);
                        _prepareToDownload.Add(download);
                    }
                }
                else
                {
                    Log(string.Format("{0}, 大小 {1}, 下载失败，未知原因，开始重新下载。", download.url, download.len));
                    _prepareToDownload.Add(download);
                }
            }
        }

        private string GetDownloadError(Download download)
        {
            using (var stream = File.OpenRead(download.tempPath))
            {
                if (Versions.verifyBy == VerifyBy.Hash)
                {
                    var hash = Utility.GetCrc32Hash(stream);
                    if (Utility.VerifyCrc32Hash(download.hash, hash))
                    {
                        return null;
                    }

                    return string.Format("哈希不匹配：{0}, 长度：{1}", hash, stream.Length);
                }
                else
                {
                    if (stream.Length == download.len)
                    {
                        return null;
                    }

                    return "长度不匹配：" + stream.Length;
                }
            }
        }

        private void Quit()
        {
            _prepareToDownload.Clear();
            _prepareToDownload = null;

            _serverRecords.Clear();
            _serverRecords = null;

            _downloads.Clear();
            _downloads = null;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}