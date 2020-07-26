//
// Updater.cs
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

    [RequireComponent(typeof(Downloader))]
    [RequireComponent(typeof(NetworkMonitor))]
    public class Updater : MonoBehaviour, IUpdater, INetworkMonitorListener
    {
        enum Step
        {
            Wait,
            Version,
            Prepared,
            Download,
        }

        private Step _step;

        [SerializeField] private string baseURL = "http://127.0.0.1:7888/DLC/";
        [SerializeField] private string gameScene = "Game.unity";
        [SerializeField] private bool development;

        public IUpdater listener { get; set; }

        private Downloader _downloader;
        private NetworkMonitor _monitor;
        private string _platform;
        private string _savePath;
        
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
            _downloader = gameObject.GetComponent<Downloader>();
            _downloader.onUpdate = OnUpdate;
            _downloader.onFinished = OnComplete;

            _monitor = gameObject.GetComponent<NetworkMonitor>();
            _monitor.listener = this;

            _savePath = string.Format("{0}/DLC/", Application.persistentDataPath);
            _platform = GetPlatformForAssetBundles(Application.platform);

            _step = Step.Wait;

            Assets.updatePath = _savePath; 
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_reachabilityChanged || _step == Step.Wait)
            {
                return;
            }

            if (hasFocus)
            {
                MessageBox.CloseAll();
                if (_step == Step.Download)
                {
                    _downloader.Restart();
                }
                else
                {
                    StartUpdate();
                }
            }
            else
            {
                if (_step == Step.Download)
                {
                    _downloader.Stop();
                }
            }
        }

        private bool _reachabilityChanged;

        public void OnReachablityChanged(NetworkReachability reachability)
        {
            if (_step == Step.Wait)
            {
                return;
            }

            _reachabilityChanged = true;
            if (_step == Step.Download)
            {
                _downloader.Stop();
            }

            if (reachability == NetworkReachability.NotReachable)
            {
                MessageBox.Show("提示！", "找不到网络，请确保手机已经联网", "确定", "退出").onComplete += delegate(MessageBox.EventId id)
                {
                    if (id == MessageBox.EventId.Ok)
                    {
                        if (_step == Step.Download)
                        {
                            _downloader.Restart();
                        }
                        else
                        {
                            StartUpdate();
                        } 
                        _reachabilityChanged = false;
                    }
                    else
                    {
                        Quit();
                    }
                };
            }
            else
            {
                MessageBox.CloseAll(); 
                if (_step == Step.Download)
                {
                    _downloader.Restart();
                }
                else
                {
                    StartUpdate();
                } 
                _reachabilityChanged = false;
            }
        }

        private void OnUpdate(long progress, long size, float speed)
        {
            OnMessage(string.Format("下载中...{0}/{1}, 速度：{2}",
                Downloader.GetDisplaySize(progress),
                Downloader.GetDisplaySize(size),
                Downloader.GetDisplaySpeed(speed)));

            OnProgress(progress * 1f / size);
        }

        public void Clear()
        {
            MessageBox.Show("提示", "清除数据后所有数据需要重新下载，请确认！", "清除").onComplete += id =>
            {
                if (id != MessageBox.EventId.Ok)
                    return;
                OnClear();
            };
        }

        public void OnClear()
        {
            OnMessage("数据清除完毕");
            OnProgress(0);
            Reset(); 
            _downloader.Clear();
            _step = Step.Wait;
            _reachabilityChanged = false; 
            Assets.Clear(); 
            if (listener != null)
            {
                listener.OnClear();
            }  
            if (Directory.Exists(_savePath + Versions.Filename))
            {
                Directory.Delete(_savePath + Versions.Filename, true);
            }
        }

        public void OnStart()
        {
            if (listener != null)
            {
                listener.OnStart();
            }
        }  

        public void StartUpdate()
        {
            Debug.Log("StartUpdate.Development:" + development);
#if UNITY_EDITOR
            if (development)
            {
                Assets.runtimeMode = false;
                StartCoroutine(LoadGameScene());
                return;
            }
#endif
            OnStart();
            Reset();
            _step = Step.Version; 
        }

        private void Update()
        {
            switch (_step)
            {
                case Step.Wait:
                    break;
                
                case Step.Version:
                    _step = Step.Wait;  
                    OnMessage("正在获取版本信息..."); 
                    if (!Directory.Exists(_savePath))
                    {
                        Directory.CreateDirectory(_savePath);
                    }   
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        MessageBox.Show("提示", "请检查网络连接状态", "重试", "退出").onComplete = OnErrorAction; 
                        return ;
                    } 
                    var request = Download(Versions.Filename);
                    var oper = request.SendWebRequest();
                    oper.completed += delegate(AsyncOperation operation)
                    {
                        if (!string.IsNullOrEmpty(request.error))
                        {
                            MessageBox.Show("提示", string.Format("获取服务器版本失败：{0}", request.error), "重试", "退出").onComplete = OnErrorAction; 
                        }
                        else
                        {
                            try
                            {
                                Versions.serverVersion = Versions.LoadFullVersion(_savePath + Versions.Filename);
                                var newFiles = Versions.GetNewFiles(PatchId.Level1, _savePath); 
                                if (newFiles.Count > 0)
                                {
                                    foreach (var item in newFiles)
                                    {
                                        _downloader.AddDownload(GetDownloadURL(item.name), item.name, _savePath + item.name, item.hash, item.len); 
                                    }
                                    _step = Step.Prepared;  
                                }
                                else
                                {
                                    OnComplete();
                                } 
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                                MessageBox.Show("提示", "版本文件加载失败", "重试", "退出").onComplete += OnErrorAction;
                            }
                        }
                    };
                    break;
                
                case Step.Prepared:
                    OnMessage("正在检查版本信息...");
                    _step = Step.Wait;
                    var totalSize = _downloader.size;
                    if (totalSize > 0)
                    {
                        var tips = string.Format("发现内容更新，总计需要下载 {0} 内容", Downloader.GetDisplaySize(totalSize));
                        MessageBox.Show("提示", tips, "下载", "退出").onComplete += delegate(MessageBox.EventId id)
                        {
                            if (id == MessageBox.EventId.Ok)
                            {
                                _downloader.StartDownload();
                                _step = Step.Download;
                            }
                            else
                            {
                                Quit(); 
                            }
                        }; 
                    }
                    else
                    {
                        OnComplete();
                    }
                    break;
            }
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
                    return "OSX"; // OSX
                default:
                    return null;
            }
        }

        private string GetDownloadURL(string filename)
        {
            return string.Format("{0}{1}/{2}", baseURL, _platform, filename);
        } 

        private List<UnityWebRequest> _downloads = new List<UnityWebRequest>();
        
        UnityWebRequest Download(string filename)
        {
            var request = UnityWebRequest.Get(GetDownloadURL(filename));
            request.downloadHandler = new DownloadHandlerFile(_savePath + filename);
            _downloads.Add(request);
            return request;
        } 
        
        private void OnErrorAction(MessageBox.EventId id)
        {
            if (id == MessageBox.EventId.Ok)
            {
                StartUpdate();
            }
            else
            {
                Quit();
            }
        }

        public static string GetStreamingAssetsPath()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return Application.streamingAssetsPath;
            }

            if (Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return "file:///" + Application.streamingAssetsPath;
            }

            return "file://" + Application.streamingAssetsPath;
        } 
        

        private void OnComplete()
        { 
            OnProgress(1);
            OnMessage("更新完成");
            OnVersion(Versions.LoadVersion(_savePath + Versions.Filename).ToString());
            StartCoroutine(LoadGameScene());
        }

        private IEnumerator LoadGameScene()
        {
            OnMessage("正在初始化");
            var init = Assets.Initialize();
            yield return init;
            if (string.IsNullOrEmpty(init.error))
            {
                init.Release();
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
                init.Release();
                var mb = MessageBox.Show("提示", "初始化异常错误：" + init.error + "请联系技术支持");
                yield return mb;
                Quit();
            }
        }

        private void OnDestroy()
        {
            Reset();
        }

        private void Reset()
        {
            foreach (var download in _downloads)
            {
                download.Dispose();
            }

            _downloads.Clear();

            MessageBox.Dispose();
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
